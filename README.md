# 🛒 Sales Management System - ASP.NET Core MVC

![Banner](https://img.shields.io/badge/ASP.NET%20Core%208.0-MVC-blue?style=for-the-badge&logo=dotnet)
![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)
![Status](https://img.shields.io/badge/Status-Developing-orange?style=for-the-badge)

Hệ thống quản lý bán hàng được xây dựng bằng ASP.NET Core MVC trên .NET 8, tổ chức theo kiến trúc 3 lớp. Ứng dụng hỗ trợ mua hàng trực tuyến, bán hàng tại quầy, quản lý sản phẩm, đơn hàng và nhân sự với ba vai trò: **Admin**, **Sales** và **Customer**.

---

## 🚀 Tính năng nổi bật

### 📦 Quản lý Sản phẩm & Kho hàng
- **Danh mục & Sản phẩm:** Quản lý danh mục phân cấp, thông tin, giá bán, hình ảnh và trạng thái sản phẩm; hỗ trợ hiển thị ảnh WebP.
- **Tra cứu sản phẩm:** Tìm kiếm, lọc theo danh mục, sắp xếp và phân trang.
- **Kho hàng:** Kiểm tra tồn kho khi đặt hàng, trừ kho khi tạo đơn và hoàn kho khi hủy đơn hoặc ghi nhận hoàn tiền; lưu lịch sử biến động kho.
- **Smart Alert:** Cảnh báo khi tồn kho xuống mức thấp.

### 💰 Bán hàng & Thanh toán
- **Bán hàng tại quầy:** Nhân viên chọn khách hàng, sản phẩm và số lượng để tạo đơn.
- **Giỏ hàng:** Hỗ trợ giỏ hàng cho khách chưa đăng nhập, gộp giỏ hàng khi đăng nhập và đặt mua các sản phẩm được chọn.
- **Mua ngay & Mã giảm giá:** Đặt hàng từ trang sản phẩm hoặc áp mã giảm giá khi thanh toán giỏ hàng. Giá bán và tổng tiền tính bằng VNĐ.
- **Thanh toán:** Đơn mới thanh toán tiền mặt khi nhận hàng, ban đầu ở trạng thái chưa thanh toán. Nhân viên chỉ chọn "Đã thu tiền & Hoàn thành" sau khi thực nhận tiền.
- **Đơn hàng & Hóa đơn:** Tra cứu lịch sử đơn hàng, quản lý trạng thái, tạo hóa đơn cùng đơn hàng và xuất PDF.
- **Hoàn tiền:** Chỉ ghi nhận sau khi đã hoàn tiền trực tiếp cho khách; ứng dụng không tự chuyển tiền.

### 👥 Quản trị Nhân sự (HRM)
- **Hồ sơ:** Quản lý thông tin nhân viên, vị trí công việc.
- **Chấm công:** Ghi nhận giờ vào/ra khi nhân viên Sales đăng nhập/đăng xuất; tra cứu báo cáo theo ngày và nhân viên.
- **Bảng lương:** Admin tạo bảng lương theo tháng dựa trên lương cơ bản, phụ cấp, khoản khấu trừ và thuế; xem chi tiết bảng lương.
- **Nghỉ phép:** Admin xem danh sách, duyệt hoặc từ chối đơn nghỉ phép.

### 👤 Tài khoản & Trải nghiệm khách hàng
- **Tài khoản:** Đăng ký, đăng nhập bằng tài khoản/mật khẩu, cập nhật hồ sơ và ảnh đại diện.
- **Phân quyền:** Phân chia chức năng cho Admin, nhân viên bán hàng (Sales) và khách hàng (Customer).
- **Google OAuth:** Hỗ trợ đăng nhập Google khi được cấu hình thông tin OAuth hợp lệ.
- **Wishlist & Review:** Lưu sản phẩm yêu thích, gửi đánh giá và hiển thị nhãn đã mua hàng với đánh giá đủ điều kiện.

### 📊 Báo cáo & Thông báo
- **SignalR Realtime:** Tích hợp thông báo và cập nhật giao diện theo thời gian thực.
- **Dashboard:** Thống kê doanh thu, sản phẩm bán chạy qua biểu đồ trực quan (Chart.js).

---

## 🏗️ Kiến trúc dự án (Architecture)

Dự án được chia làm 3 tầng tách biệt:

1. **SalesManagement.DAL (Data Access Layer):** 
   - Quản lý database sử dụng **Entity Framework Core**.
   - Repository Pattern giúp tách biệt logic truy vấn dữ liệu.
2. **SalesManagement.BLL (Business Logic Layer):** 
    - Xử lý nghiệp vụ đặt hàng, thanh toán tiền mặt, hoàn tiền, mã giảm giá, tồn kho và bảng lương.
   - Sử dụng DTOs để trao đổi dữ liệu an toàn.
3. **Sales Management (Web Layer):** 
    - Giao diện Razor Views, MVC Controllers, các action trả JSON và SignalR Hubs.
    - Tách khu vực quản trị (Admin) và bán hàng (Sale) bằng Areas.
   - Tích hợp Authentication & Google OAuth.

---

## 🛠️ Công nghệ sử dụng

- **Backend:** .NET 8 (C#), ASP.NET Core MVC.
- **Database:** SQL Server / Azure SQL.
- **Frontend:** HTML5, Vanilla CSS, JavaScript (ES6+), Bootstrap 5.
- **Thư viện chính:** 
  - Entity Framework Core.
  - SignalR (Realtime notifications).
   - iText 7 (xuất hóa đơn PDF).
   - BCrypt.Net-Next (băm mật khẩu).
  - SweetAlert2 (Interactive UI).

---

## 💻 Hướng dẫn cài đặt

1. **Clone repository:**
   ```bash
   git clone https://github.com/nhotungdo/sales-management-system.git
   ```
2. **Cấu hình Database:**
   - Mở file `appsettings.json` trong project `Sales Management`.
   - Cập nhật chuỗi kết nối `ConnectionStrings:DBDefault` phù hợp với SQL Server của bạn.
3. **Tạo database mới:**
    - Mở `Sales Management/Database/database.sql` trong SQL Server Management Studio và chạy trên database thử nghiệm mới.
    - **Cảnh báo:** script có lệnh xóa database `SalesManagement` nếu đã tồn tại. Không chạy lại trên dữ liệu cần giữ.
    - Không cần chạy thêm EF migration nếu đã tạo database bằng script này.
4. **Chạy ứng dụng:**
   ```bash
    dotnet run --project "Sales Management/SalesManagement.Web.csproj" --launch-profile http
    ```
    Mở `http://localhost:8000`.

### Kiểm tra hồi quy

Chạy từ thư mục chứa solution:

```bash
dotnet build "Sales Management.sln"
dotnet run --project tests/SalesManagement.Checks/SalesManagement.Checks.csproj
```

Bộ kiểm tra dùng SQLite trong bộ nhớ, không đọc cấu hình ứng dụng hay chạm vào SQL Server thật. Phạm vi gồm đăng ký/đăng nhập, hồ sơ khách hàng, giá VNĐ, tạo và cập nhật sản phẩm, đặt hàng tiền mặt, mã giảm giá, tồn kho, hủy đơn, ghi nhận hoàn tiền và rollback khi tạo hóa đơn thất bại. Chưa thay thế kiểm thử giao diện, script SQL và truy cập đồng thời trên SQL Server.

---

## 🤝 Đóng góp

1. Fork dự án.
2. Tạo nhánh tính năng (`git checkout -b feature/AmazingFeature`).
3. Commit thay đổi (`git commit -m 'Add some AmazingFeature'`).
4. Push lên nhánh (`git push origin feature/AmazingFeature`).
5. Mở một Pull Request.

---

## 📄 Giấy phép

Phân phối theo giấy phép MIT. Xem `LICENSE` để biết thêm thông tin.

---
⭐ **Nếu bạn thấy dự án hữu ích, hãy tặng chúng mình 1 sao nhé!**
