# Hướng Dẫn Cấu Hình Gemini API

## 🚀 Tích Hợp Gemini API vào AI Writing Assistant

### 1. Lấy API Key từ Google AI Studio

1. Truy cập: https://aistudio.google.com/
2. Đăng nhập bằng tài khoản Google
3. Nhấn "Get API Key" 
4. Tạo API Key mới
5. Copy API Key

### 2. Cấu Hình API Key

Mở file `appsettings.json` và cập nhật:

```json
{
  "AI": {
    "ApiKey": "YOUR_GEMINI_API_KEY_HERE",
    "Model": "gemini-1.5-flash",
    "MaxTokens": 2000,
    "Temperature": 0.7,
    "Provider": "gemini"
  }
}
```

### 3. Các Model Gemini Khả Dụng

- `gemini-1.5-flash` - Nhanh, rẻ, phù hợp cho hầu hết tác vụ
- `gemini-1.5-pro` - Mạnh hơn, chậm hơn, phù hợp cho tác vụ phức tạp
- `gemini-1.0-pro` - Phiên bản ổn định

### 4. Tính Năng Đã Tích Hợp

✅ **Tạo nội dung thông minh** - Dựa trên input thực tế
✅ **Phân tích SEO** - Điểm số và gợi ý tối ưu
✅ **Nghiên cứu từ khóa** - Tìm từ khóa liên quan
✅ **Gợi ý nội dung** - Cải thiện bài viết

### 5. Fallback System

Nếu không có API Key hoặc lỗi kết nối:
- Hệ thống sẽ sử dụng mock data thông minh
- Tạo nội dung dựa trên input thực tế
- Vẫn hoạt động đầy đủ chức năng

### 6. Test Cấu Hình

1. Khởi động ứng dụng: `dotnet run`
2. Truy cập: `https://localhost:5000/Admin/AIAssistant/ContentGenerator`
3. Nhập chủ đề: "Công nghệ blockchain"
4. Nhấn "Tạo nội dung"
5. Kiểm tra nội dung được tạo có phù hợp với chủ đề

### 7. Troubleshooting

**Lỗi API Key không hợp lệ:**
- Kiểm tra API Key có đúng không
- Đảm bảo API Key có quyền truy cập Gemini API

**Lỗi kết nối:**
- Kiểm tra kết nối internet
- Kiểm tra firewall/antivirus

**Nội dung không phù hợp:**
- Thử điều chỉnh Temperature (0.1-1.0)
- Thử model khác (gemini-1.5-pro)
- Cải thiện prompt input

### 8. Chi Phí

- Gemini 1.5 Flash: ~$0.075/1M tokens input, $0.30/1M tokens output
- Gemini 1.5 Pro: ~$1.25/1M tokens input, $5.00/1M tokens output
- Có free tier: 15 requests/phút

### 9. Bảo Mật

- Không commit API Key vào Git
- Sử dụng User Secrets cho development:
  ```bash
  dotnet user-secrets set "AI:ApiKey" "YOUR_API_KEY"
  ```
- Sử dụng Azure Key Vault cho production

### 10. Monitoring

Kiểm tra logs để theo dõi:
- Số lượng requests
- Thời gian phản hồi
- Lỗi API
- Chi phí sử dụng

---

**Lưu ý:** Hệ thống đã được thiết kế để hoạt động tốt cả với và không có API Key. Mock data sẽ tạo nội dung phù hợp với input thực tế của người dùng.
