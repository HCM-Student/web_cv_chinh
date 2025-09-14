# Tính năng Người dùng Online

## Tổng quan
Tính năng này cho phép theo dõi và hiển thị số lượng người dùng đang online trên website, tương tự như trong hình ảnh mẫu.

## Các thành phần đã thêm

### 1. Model và Database
- **OnlineUser.cs**: Model để lưu trữ thông tin người dùng online
- **Migration**: Bảng `OnlineUsers` đã được tạo trong database
- **Indexes**: Tối ưu hóa truy vấn với các index trên SessionId, LastSeen, IsActive

### 2. Services
- **IOnlineUserService**: Interface định nghĩa các phương thức quản lý người dùng online
- **OnlineUserService**: Service thực hiện logic theo dõi người dùng
- **OnlineUserCleanupService**: Background service dọn dẹp người dùng không hoạt động

### 3. Controller
- **OnlineUserController**: API controller cung cấp các endpoint:
  - `GET /api/OnlineUser/count`: Lấy số lượng người dùng online
  - `GET /api/OnlineUser/list`: Lấy danh sách người dùng online
  - `POST /api/OnlineUser/track`: Theo dõi người dùng hiện tại
  - `POST /api/OnlineUser/cleanup`: Dọn dẹp người dùng không hoạt động (Admin only)

### 4. Middleware
- **OnlineUserTrackingMiddleware**: Tự động theo dõi người dùng khi họ truy cập website

### 5. Views
- **_OnlineUsersModal.cshtml**: Modal hiển thị số người dùng online với giao diện đẹp
- **Header**: Thêm nút "Online" để mở modal

## Cách hoạt động

### 1. Theo dõi người dùng
- Khi người dùng truy cập website (GET request), middleware sẽ tự động ghi nhận:
  - Session ID
  - Email (nếu đã đăng nhập)
  - Họ tên (nếu đã đăng nhập)
  - IP Address
  - User Agent
  - Thời gian truy cập cuối

### 2. Cập nhật real-time
- JavaScript tự động cập nhật số người online mỗi 10 giây
- Theo dõi người dùng hiện tại mỗi 30 giây
- Modal có thể làm mới thủ công

### 3. Dọn dẹp tự động
- Background service dọn dẹp người dùng không hoạt động mỗi 5 phút
- Người dùng được coi là "không hoạt động" nếu không truy cập trong 10 phút

## Giao diện

### Modal hiển thị
- **Tiêu đề**: "Người dùng online" với icon
- **Số lượng**: Hiển thị số người online lớn và nổi bật
- **Thời gian**: "(trong X phút qua)"
- **Danh sách**: Có thể xem danh sách chi tiết người dùng online
- **Nút làm mới**: Cập nhật thông tin thủ công

### Nút trong header
- Nút "Online" màu xanh lá với icon người dùng
- Responsive: ẩn text trên mobile, chỉ hiện icon

## Cấu hình

### Session
```csharp
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
```

### Cleanup
- Dọn dẹp mỗi 5 phút
- Người dùng không hoạt động: 10 phút
- Có thể thay đổi trong `OnlineUserCleanupService`

## API Endpoints

### Lấy số người online
```
GET /api/OnlineUser/count?minutes=10
Response: { "count": 15, "minutes": 10 }
```

### Lấy danh sách người online
```
GET /api/OnlineUser/list?minutes=10
Response: { 
  "users": [
    {
      "id": 1,
      "hoTen": "Nguyễn Văn A",
      "email": "user@example.com",
      "lastSeen": "2024-01-01T10:00:00Z",
      "ipAddress": "192.168.1.1"
    }
  ],
  "count": 1,
  "minutes": 10
}
```

### Theo dõi người dùng
```
POST /api/OnlineUser/track
Body: { "email": "user@example.com", "hoTen": "Nguyễn Văn A" }
Response: { "success": true }
```

## Bảo mật
- Chỉ theo dõi GET requests (tránh spam API calls)
- Không theo dõi các request đến `/api`
- IP Address được lưu trữ để phân tích
- Session ID được sử dụng để định danh duy nhất

## Hiệu suất
- Sử dụng background tasks để tránh block request
- Indexes trên database để tối ưu truy vấn
- Cleanup tự động để tránh tích lũy dữ liệu
- Caching trong JavaScript để giảm API calls

## Tùy chỉnh
- Thay đổi thời gian cleanup trong `OnlineUserCleanupService`
- Thay đổi interval cập nhật trong JavaScript
- Tùy chỉnh giao diện modal trong `_OnlineUsersModal.cshtml`
- Thay đổi logic theo dõi trong `OnlineUserTrackingMiddleware`
