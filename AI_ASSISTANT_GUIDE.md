# AI Writing Assistant - Hướng dẫn sử dụng

## Tổng quan
AI Writing Assistant là một hệ thống tích hợp AI giúp tạo nội dung, phân tích SEO và nghiên cứu từ khóa cho website tin tức.

## Tính năng chính

### 1. Tạo nội dung AI
- **Vị trí**: Admin Panel > AI Assistant > Tạo Nội Dung AI
- **Chức năng**: Tự động tạo tiêu đề, nội dung, tóm tắt dựa trên chủ đề và từ khóa
- **Cách sử dụng**:
  1. Nhập chủ đề bài viết
  2. Thêm từ khóa chính (tùy chọn)
  3. Chọn phong cách viết (chuyên nghiệp, thân thiện, học thuật)
  4. Chọn số từ mong muốn
  5. Nhấn "Tạo nội dung"
  6. Xem kết quả và nhấn "Sử dụng nội dung này" để áp dụng vào form

### 2. Phân tích SEO
- **Vị trí**: Admin Panel > AI Assistant > Phân Tích SEO
- **Chức năng**: Kiểm tra và đánh giá tối ưu hóa SEO cho nội dung
- **Cách sử dụng**:
  1. Nhập tiêu đề và nội dung cần phân tích
  2. Thêm từ khóa chính và từ khóa phụ
  3. Nhấn "Phân tích SEO"
  4. Xem điểm số SEO và các gợi ý cải thiện

### 3. Nghiên cứu từ khóa
- **Vị trí**: Admin Panel > AI Assistant > Nghiên Cứu Từ Khóa
- **Chức năng**: Tìm kiếm và phân tích từ khóa liên quan
- **Cách sử dụng**:
  1. Nhập chủ đề hoặc từ khóa gốc
  2. Chọn ngôn ngữ và số lượng kết quả
  3. Nhấn "Tìm kiếm từ khóa"
  4. Chọn từ khóa phù hợp và xuất Excel

### 4. AI Assistant trong Editor
- **Vị trí**: Admin Panel > Bài viết > Tạo/Sửa bài viết
- **Chức năng**: Tích hợp AI trực tiếp vào form tạo bài viết
- **Cách sử dụng**:
  1. Mở form tạo/sửa bài viết
  2. Sử dụng panel AI Assistant ở trên cùng
  3. Tạo nội dung hoặc phân tích SEO trực tiếp
  4. Nội dung sẽ được tự động điền vào form

## Cấu hình AI

### Thiết lập API Key
1. Mở file `appsettings.json`
2. Thêm API key của OpenAI vào phần `AI.ApiKey`:
```json
{
  "AI": {
    "ApiKey": "your-openai-api-key-here",
    "Model": "gpt-3.5-turbo",
    "MaxTokens": 2000,
    "Temperature": 0.7
  }
}
```

### Fallback Mode
Nếu không có API key, hệ thống sẽ sử dụng dữ liệu mẫu để demo các tính năng.

## API Endpoints

### 1. Tạo nội dung
```
POST /api/ai/generate-content
Content-Type: application/json

{
  "topic": "Chủ đề bài viết",
  "style": "professional",
  "wordCount": 500,
  "tone": "neutral",
  "keywords": ["từ khóa 1", "từ khóa 2"],
  "language": "vi"
}
```

### 2. Phân tích SEO
```
POST /api/ai/analyze-seo
Content-Type: application/json

{
  "title": "Tiêu đề bài viết",
  "content": "Nội dung bài viết",
  "keywords": ["từ khóa 1", "từ khóa 2"],
  "targetKeyword": "từ khóa chính"
}
```

### 3. Gợi ý nội dung
```
POST /api/ai/content-suggestions
Content-Type: application/json

{
  "topic": "Chủ đề",
  "currentContent": "Nội dung hiện tại",
  "keywords": ["từ khóa 1", "từ khóa 2"]
}
```

### 4. Nghiên cứu từ khóa
```
GET /api/ai/keyword-suggestions?topic=chủ đề
```

## Tính năng nâng cao

### Auto-analyze
- Hệ thống tự động phân tích SEO khi người dùng nhập tiêu đề hoặc nội dung
- Cập nhật điểm SEO real-time

### Lưu trữ thống kê
- Thống kê số bài viết đã tạo
- Thống kê số nội dung đã phân tích
- Điểm SEO trung bình
- Lịch sử hoạt động

### Responsive Design
- Giao diện tối ưu cho desktop và mobile
- Slider cho danh sách bài viết
- Modal popup cho nội dung AI

## Troubleshooting

### Lỗi thường gặp

1. **"Có lỗi xảy ra khi tạo nội dung"**
   - Kiểm tra kết nối internet
   - Kiểm tra API key OpenAI
   - Thử lại sau vài phút

2. **"Không tìm thấy từ khóa nào"**
   - Thử chủ đề khác
   - Kiểm tra kết nối internet
   - Sử dụng từ khóa tiếng Việt

3. **AI Assistant không hiển thị trong form**
   - Kiểm tra file `_AIAssistant.cshtml` có tồn tại
   - Kiểm tra partial view được include đúng cách

### Performance Tips

1. **Tối ưu API calls**
   - Sử dụng debounce cho auto-analyze
   - Cache kết quả phân tích SEO

2. **Giảm tải server**
   - Sử dụng mock data khi không có API key
   - Implement rate limiting

## Cập nhật và bảo trì

### Thêm model AI mới
1. Cập nhật `AIWritingService.cs`
2. Thêm configuration trong `appsettings.json`
3. Test với model mới

### Thêm tính năng mới
1. Tạo interface trong `IAIWritingService.cs`
2. Implement trong `AIWritingService.cs`
3. Thêm API endpoint trong `AIController.cs`
4. Cập nhật giao diện frontend

## Liên hệ hỗ trợ
Nếu gặp vấn đề, vui lòng liên hệ team phát triển hoặc tạo issue trên repository.
