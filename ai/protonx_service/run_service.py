#!/usr/bin/env python3
"""
Script để chạy ProtonX service với cấu hình mặc định
"""
import os
import uvicorn

# Thiết lập environment variables mặc định
os.environ.setdefault("PROTONX_TOKEN", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJlbWFpbCI6Imx1YW5oYTIwMDNAZ21haWwuY29tIiwiaWF0IjoxNzU4MTc4Mjk0LCJleHAiOjE3NjA3NzAyOTR9.XDDIz1U9QlAh4fxggffneS5xwkBm9Gg9XmSSg-6WW-c")
os.environ.setdefault("CHROMA_PATH", os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "vector_db")))
os.environ.setdefault("COLLECTION", "phones")

if __name__ == "__main__":
    print("🚀 Starting ProtonX Service...")
    print(f"📁 ChromaDB Path: {os.environ['CHROMA_PATH']}")
    print(f"📚 Collection: {os.environ['COLLECTION']}")
    print(f"🔑 ProtonX Token: {'Set' if os.environ.get('PROTONX_TOKEN') else 'Not set (will use free tier)'}")
    print("=" * 50)
    
    uvicorn.run(
        "main:app",
        host="127.0.0.1",
        port=8089,
        reload=True,
        log_level="info"
    )
