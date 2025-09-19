from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from typing import List, Optional
import os, chromadb
from protonx import ProtonX

# ====== Cấu hình ======
PROTONX_TOKEN = os.getenv("PROTONX_TOKEN") or os.getenv("PROTONX_API_KEY")  # đặt bằng Env/Secret
CHROMA_PATH   = os.getenv("CHROMA_PATH", os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "vector_db")))
COLLECTION    = os.getenv("COLLECTION", "phones")  # đúng với tập dữ liệu bạn đã build

# ====== Khởi tạo client ======
client = None
if PROTONX_TOKEN:
    try:
        client = ProtonX(api_key=PROTONX_TOKEN)
        print("✅ ProtonX client initialized with API key")
    except Exception as e:
        print(f"❌ Failed to initialize ProtonX with API key: {e}")
        client = None
else:
    print("⚠️  No PROTONX_TOKEN provided. Service will run in limited mode.")
    print("   To get full functionality, set PROTONX_TOKEN environment variable")
    print("   Get your API key at: https://platform.protonx.io/")
chroma  = chromadb.PersistentClient(path=CHROMA_PATH)
collection = chroma.get_or_create_collection(name=COLLECTION)

class SearchRequest(BaseModel):
    query: str
    n_results: int = 5

class EmbedRequest(BaseModel):
    text: str

app = FastAPI(title="ProtonX wrapper", version="1.0")

@app.get("/health")
def health():
    return {"status": "ok", "collection": COLLECTION}

@app.post("/embed")
def embed(req: EmbedRequest):
    if not client:
        # Mock embedding for testing without API key
        import hashlib
        import random
        # Tạo embedding giả dựa trên hash của text
        text_hash = hashlib.md5(req.text.encode()).hexdigest()
        random.seed(int(text_hash[:8], 16))  # Sử dụng hash làm seed
        mock_embedding = [random.random() for _ in range(384)]  # 384 dimensions
        return {"embedding": mock_embedding, "note": "Mock embedding - get PROTONX_TOKEN for real embeddings"}
    
    data = client.embeddings.create(req.text).get("data")
    if not data:
        raise HTTPException(400, "Embedding failed.")
    return {"embedding": data[0].get("embedding")}

@app.post("/search")
def search(req: SearchRequest):
    try:
        # Kiểm tra xem collection có dữ liệu không
        count = collection.count()
        if count == 0:
            return {"results": [], "message": "Chưa có dữ liệu trong cơ sở dữ liệu vector. Vui lòng thêm dữ liệu trước."}
        
        if not client:
            # Mock search without API key - tìm kiếm text-based
            print(f"🔍 Mock search for: {req.query}")
            # Lấy tất cả documents và tìm kiếm text-based
            all_docs = collection.get(include=["metadatas", "documents"])
            out = []
            
            query_lower = req.query.lower()
            for i, doc in enumerate(all_docs.get("documents", [])):
                if not doc:
                    continue
                    
                # Tìm kiếm trong document text
                doc_text = doc.lower() if isinstance(doc, str) else str(doc).lower()
                if query_lower in doc_text:
                    meta = all_docs.get("metadatas", [{}])[i] or {}
                    out.append({
                        "id": all_docs.get("ids", [f"mock_{i}"])[i],
                        "score": 0.8,  # Mock score
                        "title": meta.get("title", f"Document {i+1}"),
                        "url": meta.get("url", ""),
                        "snippet": (meta.get("information") or meta.get("product_specs") or str(doc))[:400]
                    })
            
            # Giới hạn kết quả
            out = out[:req.n_results]
            return {"results": out, "note": "Mock search - get PROTONX_TOKEN for vector search"}
        
        # Real search with ProtonX API
        data = client.embeddings.create(req.query).get("data")
        if not data:
            raise HTTPException(400, "Embedding failed.")
        emb = data[0].get("embedding")
        
        hits = collection.query(
            query_embeddings=[emb],
            n_results=max(1, min(20, req.n_results)),
            include=["metadatas", "distances", "documents"]
        )
        out = []
        if not hits or not hits.get("ids") or not hits["ids"][0]: 
            return {"results": out, "message": "Không tìm thấy kết quả phù hợp."}
            
        for i in range(len(hits["ids"][0])):
            dist  = hits["distances"][0][i]
            score = 1 - dist
            meta  = (hits["metadatas"][0][i] or {})
            out.append({
                "id": hits["ids"][0][i],
                "score": score,
                "title": meta.get("title"),
                "url": meta.get("url"),
                "snippet": (meta.get("information") or meta.get("product_specs") or "")[:400]
            })
        return {"results": out}
    except Exception as e:
        raise HTTPException(500, f"Search error: {str(e)}")