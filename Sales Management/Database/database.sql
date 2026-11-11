USE master;
GO

IF EXISTS (SELECT * FROM sys.databases WHERE name = 'SalesManagement')
    DROP DATABASE SalesManagement;
GO

CREATE DATABASE SalesManagement;
GO

USE SalesManagement;
GO

-- ====================================================
-- 0. EF Migrations History
-- ====================================================
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

-- ====================================================
-- I. Authentication & User Management
-- ====================================================

-- Bảng Users
CREATE TABLE Users (
    UserId       INT IDENTITY(1,1) PRIMARY KEY,
    Username     NVARCHAR(50)  NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Email        NVARCHAR(100) NOT NULL UNIQUE,
    FullName     NVARCHAR(100),
    PhoneNumber  NVARCHAR(20),
    Avatar       NVARCHAR(255),
    GoogleId     NVARCHAR(100),
    Role         NVARCHAR(20)  NOT NULL
                     CHECK (Role IN ('Admin', 'Sales', 'Customer'))
                     DEFAULT 'Customer',
    IsActive     BIT  NOT NULL DEFAULT 1,
    IsDeleted    BIT  NOT NULL DEFAULT 0,
    LastLogin    DATETIME,
    CreatedDate  DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedDate  DATETIME NOT NULL DEFAULT GETDATE()
);
GO

CREATE INDEX IX_Users_Email    ON Users(Email);
CREATE INDEX IX_Users_Username ON Users(Username);
GO

-- ====================================================
-- II. HR – Shifts & Employees
-- ====================================================

-- Bảng Shifts (ca làm việc)
CREATE TABLE Shifts (
    ShiftId   INT IDENTITY(1,1) PRIMARY KEY,
    ShiftName NVARCHAR(50) NOT NULL,
    StartTime TIME NOT NULL,
    EndTime   TIME NOT NULL
);
GO

-- Bảng Employees
CREATE TABLE Employees (
    EmployeeId       INT IDENTITY(1,1) PRIMARY KEY,
    UserId           INT  NOT NULL UNIQUE,
    Position         NVARCHAR(100),
    BasicSalary      DECIMAL(15, 2) DEFAULT 0,
    HourlyWage       DECIMAL(18, 2),          -- lương theo giờ
    StartWorkingDate DATE,
    Department       NVARCHAR(100),
    ShiftId          INT,                     -- ca làm việc mặc định
    ContractType     NVARCHAR(50),            -- Full-time, Part-time, Intern
    ContractFile     NVARCHAR(255),
    IsDeleted        BIT NOT NULL DEFAULT 0,
    ChangeHistory    NVARCHAR(MAX),
    FOREIGN KEY (UserId)  REFERENCES Users(UserId)   ON DELETE CASCADE,
    FOREIGN KEY (ShiftId) REFERENCES Shifts(ShiftId)
);
GO

-- ====================================================
-- III. Customer Management (CRM)
-- ====================================================

-- Bảng Customers
CREATE TABLE Customers (
    CustomerId    INT IDENTITY(1,1) PRIMARY KEY,
    UserId        INT UNIQUE,
    FullName      NVARCHAR(100) NOT NULL,
    Email         NVARCHAR(100),
    PhoneNumber   NVARCHAR(20),
    Address       NVARCHAR(MAX),
    Type          NVARCHAR(20) CHECK (Type IN ('Personal', 'Business')) DEFAULT 'Personal',
    CustomerLevel NVARCHAR(20) CHECK (CustomerLevel IN ('Regular', 'VIP', 'Potential')) DEFAULT 'Regular',
    Note          NVARCHAR(MAX),
    CreatedDate   DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE SET NULL
);
GO

-- ====================================================
-- IV. HR – Attendance, Payroll & Leave
-- ====================================================

-- Bảng TimeAttendances (chấm công)
CREATE TABLE TimeAttendances (
    AttendanceId    INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId      INT  NOT NULL,
    ShiftId         INT,                              -- ca làm việc trong ngày
    Date            DATE NOT NULL,
    CheckInTime     DATETIME,
    CheckOutTime    DATETIME,
    Status          NVARCHAR(50),                     -- Present, Absent, Late, LeftEarly
    Platform        NVARCHAR(50),                     -- Web, Mobile
    Notes           NVARCHAR(MAX),
    WorkHours       FLOAT NOT NULL DEFAULT 0,         -- số giờ làm thực tế
    OvertimeHours   FLOAT NOT NULL DEFAULT 0,         -- số giờ tăng ca
    MinutesLate     INT   NOT NULL DEFAULT 0,         -- số phút đi muộn
    DeductionAmount DECIMAL(18, 2) NOT NULL DEFAULT 0,-- số tiền trừ lương
    FOREIGN KEY (EmployeeId) REFERENCES Employees(EmployeeId),
    FOREIGN KEY (ShiftId)    REFERENCES Shifts(ShiftId)
);
GO

-- Bảng SalaryComponents (thành phần lương: phụ cấp / khấu trừ)
CREATE TABLE SalaryComponents (
    SalaryComponentId INT IDENTITY(1,1) PRIMARY KEY,
    Name              NVARCHAR(100) NOT NULL,
    Type              NVARCHAR(20)  NOT NULL DEFAULT 'Allowance'
                          CHECK (Type IN ('Allowance', 'Deduction')),
    DefaultAmount     DECIMAL(18, 2) NOT NULL DEFAULT 0,
    IsPercentage      BIT NOT NULL DEFAULT 0, -- nếu = 1 thì tính % trên BasicSalary
    Description       NVARCHAR(255)
);
GO

-- Bảng EmployeeSalaryComponents (phụ cấp/khấu trừ riêng của từng nhân viên)
CREATE TABLE EmployeeSalaryComponents (
    Id                INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId        INT NOT NULL,
    SalaryComponentId INT NOT NULL,
    Amount            DECIMAL(18, 2) NOT NULL DEFAULT 0,
    FOREIGN KEY (EmployeeId)        REFERENCES Employees(EmployeeId) ON DELETE CASCADE,
    FOREIGN KEY (SalaryComponentId) REFERENCES SalaryComponents(SalaryComponentId)
);
GO

-- Bảng Payrolls (bảng lương tháng)
CREATE TABLE Payrolls (
    PayrollId       INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId      INT NOT NULL,
    Month           INT NOT NULL,
    Year            INT NOT NULL,
    BaseSalary      DECIMAL(18, 2) NOT NULL DEFAULT 0,
    Benefits        DECIMAL(18, 2) NOT NULL DEFAULT 0,   -- phúc lợi
    Bonus           DECIMAL(18, 2) NOT NULL DEFAULT 0,   -- thưởng
    Penalty         DECIMAL(18, 2) NOT NULL DEFAULT 0,   -- phạt
    TotalAllowances DECIMAL(18, 2) NOT NULL DEFAULT 0,   -- tổng phụ cấp
    TotalDeductions DECIMAL(18, 2) NOT NULL DEFAULT 0,   -- tổng khấu trừ
    TaxAmount       DECIMAL(18, 2) NOT NULL DEFAULT 0,   -- thuế TNCN
    NetSalary       DECIMAL(18, 2) NOT NULL DEFAULT 0,   -- lương thực lĩnh
    TotalSalary     DECIMAL(18, 2) NOT NULL DEFAULT 0,   -- tổng lương gross
    Status          NVARCHAR(50) DEFAULT 'Pending'       -- Pending, Paid
                        CHECK (Status IN ('Pending', 'Paid')),
    CreatedDate     DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (EmployeeId) REFERENCES Employees(EmployeeId)
);
GO

-- Bảng LeaveRequests (đơn xin nghỉ phép)
CREATE TABLE LeaveRequests (
    LeaveRequestId INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId     INT NOT NULL,
    StartDate      DATE NOT NULL,
    EndDate        DATE NOT NULL,
    Reason         NVARCHAR(MAX) NOT NULL,
    Status         NVARCHAR(20) NOT NULL DEFAULT 'Pending'
                       CHECK (Status IN ('Pending', 'Approved', 'Rejected')),
    LeaveType      NVARCHAR(50) NOT NULL DEFAULT 'Annual'
                       CHECK (LeaveType IN ('Annual', 'Sick', 'Unpaid', 'Maternity')),
    AdminComment   NVARCHAR(MAX),
    CreatedDate    DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (EmployeeId) REFERENCES Employees(EmployeeId)
);
GO

-- ====================================================
-- VI. Product & Inventory Management
-- ====================================================

-- Bảng Categories
CREATE TABLE Categories (
    CategoryId   INT IDENTITY(1,1) PRIMARY KEY,
    Name         NVARCHAR(100) NOT NULL,
    Description  NVARCHAR(MAX),
    ImageUrl     NVARCHAR(255),
    Status       NVARCHAR(20) DEFAULT 'Active',
    IsDeleted    BIT NOT NULL DEFAULT 0,
    DisplayOrder INT,
    CreatedDate  DATETIME DEFAULT GETDATE(),
    UpdatedDate  DATETIME DEFAULT GETDATE(),
    ParentId     INT,
    FOREIGN KEY (ParentId) REFERENCES Categories(CategoryId)
);
GO

-- Bảng Products
CREATE TABLE Products (
    ProductId     INT IDENTITY(1,1) PRIMARY KEY,
    Code          NVARCHAR(50)  NOT NULL UNIQUE,
    Name          NVARCHAR(150) NOT NULL,
    Description   NVARCHAR(MAX),
    CategoryId    INT,
    ImportPrice   DECIMAL(15, 2) DEFAULT 0,
    SellingPrice  DECIMAL(15, 2) NOT NULL,
    VATRate       DECIMAL(5, 2)  DEFAULT 0,
    StockQuantity INT DEFAULT 0,
    Status        NVARCHAR(20) DEFAULT 'Active'
                      CHECK (Status IN ('Active', 'Inactive')),
    CreatedBy     INT,
    CreatedDate   DATETIME DEFAULT GETDATE(),
    UpdatedDate   DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId),
    FOREIGN KEY (CreatedBy)  REFERENCES Users(UserId)
);
GO

CREATE INDEX IX_Products_Status      ON Products(Status);
CREATE INDEX IX_Products_CreatedDate ON Products(CreatedDate);
GO

-- Bảng ProductImages
CREATE TABLE ProductImages (
    ImageId     INT IDENTITY(1,1) PRIMARY KEY,
    ProductId   INT NOT NULL,
    ImageUrl    NVARCHAR(255) NOT NULL,
    IsPrimary   BIT DEFAULT 0,
    CreatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ProductId) REFERENCES Products(ProductId) ON DELETE CASCADE
);
GO

-- Bảng InventoryTransactions
CREATE TABLE InventoryTransactions (
    TransId     INT IDENTITY(1,1) PRIMARY KEY,
    ProductId   INT NOT NULL,
    Quantity    INT NOT NULL,
    Type        NVARCHAR(50) NOT NULL
                    CHECK (Type IN ('Import', 'Export_Order', 'Audit_Adjustment')),
    Note        NVARCHAR(MAX),
    CreatedBy   INT,
    CreatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ProductId)  REFERENCES Products(ProductId),
    FOREIGN KEY (CreatedBy)  REFERENCES Users(UserId)
);
GO

-- ====================================================
-- VII. Promotion & Sales Order
-- ====================================================

-- Bảng Promotions
CREATE TABLE Promotions (
    PromotionId       INT IDENTITY(1,1) PRIMARY KEY,
    Code              NVARCHAR(20) NOT NULL UNIQUE,
    DiscountType      NVARCHAR(20) NOT NULL CHECK (DiscountType IN ('Percent', 'Amount')),
    Value             DECIMAL(15, 2) NOT NULL,
    StartDate         DATETIME,
    EndDate           DATETIME,
    MinOrderValue     DECIMAL(15, 2) DEFAULT 0,
    MaxDiscountAmount DECIMAL(15, 2),
    Status            NVARCHAR(20) DEFAULT 'Active'
                          CHECK (Status IN ('Active', 'Expired', 'Disabled'))
);
GO

-- Bảng Orders
CREATE TABLE Orders (
    OrderId         INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId      INT NOT NULL,
    CreatedBy       INT,
    OrderDate       DATETIME DEFAULT GETDATE(),
    SubTotal        DECIMAL(15, 2) DEFAULT 0,
    TaxAmount       DECIMAL(15, 2) DEFAULT 0,
    DiscountAmount  DECIMAL(15, 2) DEFAULT 0,
    TotalAmount     DECIMAL(15, 2) DEFAULT 0,
    Status          NVARCHAR(20) DEFAULT 'Draft'
                        CHECK (Status IN ('Draft', 'Confirmed', 'Paid', 'Cancelled', 'Completed')),
    PaymentMethod   NVARCHAR(20) DEFAULT 'Cash'
                        CHECK (PaymentMethod IN ('Cash', 'BankTransfer')),
    PaymentStatus   NVARCHAR(20) DEFAULT 'Unpaid'
                        CHECK (PaymentStatus IN ('Unpaid', 'Paid', 'Refunded')),
    ShippingAddress NVARCHAR(MAX),
    Note            NVARCHAR(MAX),
    FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId),
    FOREIGN KEY (CreatedBy)  REFERENCES Users(UserId)
);
GO

CREATE INDEX IX_Orders_OrderDate ON Orders(OrderDate);
CREATE INDEX IX_Orders_Status    ON Orders(Status);
GO

-- Bảng OrderDetails
CREATE TABLE OrderDetails (
    OrderDetailId INT IDENTITY(1,1) PRIMARY KEY,
    OrderId       INT NOT NULL,
    ProductId     INT NOT NULL,
    Quantity      INT NOT NULL,
    UnitPrice     DECIMAL(15, 2) NOT NULL,
    VATRate       DECIMAL(5, 2)  DEFAULT 0,
    Total         DECIMAL(15, 2),
    FOREIGN KEY (OrderId)   REFERENCES Orders(OrderId) ON DELETE CASCADE,
    FOREIGN KEY (ProductId) REFERENCES Products(ProductId)
);
GO

CREATE INDEX IX_OrderDetails_OrderId_ProductId ON OrderDetails(OrderId, ProductId);
GO

-- Bảng OrderPromotions
CREATE TABLE OrderPromotions (
    OrderPromotionId INT IDENTITY(1,1) PRIMARY KEY,
    OrderId          INT NOT NULL,
    PromotionId      INT NOT NULL,
    AppliedValue     DECIMAL(15, 2) NOT NULL, -- số tiền giảm thực tế
    CreatedDate      DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT UQ_Order_Promotion UNIQUE (OrderId, PromotionId),
    FOREIGN KEY (OrderId)     REFERENCES Orders(OrderId)         ON DELETE CASCADE,
    FOREIGN KEY (PromotionId) REFERENCES Promotions(PromotionId)
);
GO

-- ====================================================
-- VIII. Finance
-- ====================================================

-- Bảng Invoices
CREATE TABLE Invoices (
    InvoiceId   INT IDENTITY(1,1) PRIMARY KEY,
    OrderId     INT NOT NULL,
    InvoiceDate DATETIME DEFAULT GETDATE(),
    Amount      DECIMAL(15, 2) NOT NULL,
    Status      NVARCHAR(20) DEFAULT 'Issued'
                    CHECK (Status IN ('Issued', 'Paid', 'Overdue', 'Cancelled')),
    FOREIGN KEY (OrderId) REFERENCES Orders(OrderId)
);
GO

-- ====================================================
-- IX. System & Configuration
-- ====================================================

-- Bảng SystemSettings
CREATE TABLE SystemSettings (
    SettingKey   NVARCHAR(50)  PRIMARY KEY,
    SettingValue NVARCHAR(MAX),
    GroupName    NVARCHAR(100),
    Description  NVARCHAR(255)
);
GO



-- ====================================================
-- X. SEED DATA (Dữ liệu mẫu)
-- ====================================================

-- 1. Users
INSERT INTO Users (Username, PasswordHash, Email, FullName, Role, IsActive) VALUES
('admin', '123456', 'admin@salesmanager.com', N'System Administrator', 'Admin', 1);
INSERT INTO Users (Username, PasswordHash, Email, FullName, Role, IsActive) VALUES
('sale',  '123456', 'sale@salesmanager.com',  N'Sales Staff',          'Sales', 1);
GO

-- 2. Shifts
INSERT INTO Shifts (ShiftName, StartTime, EndTime) VALUES
(N'Ca sáng', '08:00', '12:00'),
(N'Ca chiều', '13:00', '17:00'),
(N'Ca toàn thời gian', '08:00', '17:00');
GO

-- 3. Employees
INSERT INTO Employees (UserId, Position, BasicSalary, HourlyWage, StartWorkingDate, Department, ShiftId, ContractType)
VALUES (2, N'Nhân viên kinh doanh', 8000000, 50000, GETDATE(), N'Kinh doanh', 3, 'Full-time');
GO

-- 4. SalaryComponents
INSERT INTO SalaryComponents (Name, Type, DefaultAmount, IsPercentage, Description) VALUES
(N'Phụ cấp ăn trưa',    'Allowance', 500000, 0, N'Hỗ trợ bữa trưa hàng tháng'),
(N'Phụ cấp đi lại',     'Allowance', 300000, 0, N'Hỗ trợ xăng xe, đi lại'),
(N'Bảo hiểm xã hội',    'Deduction', 8,      1, N'8% lương cơ bản - BHXH nhân viên đóng'),
(N'Thuế thu nhập cá nhân','Deduction',10,     1, N'Thuế TNCN ước tính');
GO

-- 5. Categories
INSERT INTO Categories (Name, Description) VALUES
(N'Điện tử',              N'Điện thoại, máy tính, thiết bị điện tử'),
(N'Thời trang',           N'Quần áo, giày dép nam nữ'),
(N'Gia dụng',             N'Đồ dùng gia đình, nội thất'),
(N'Thực phẩm & Đồ uống', N'Thực phẩm khô, tươi sống, đồ uống'),
(N'Mỹ phẩm & Làm đẹp',   N'Chăm sóc da, trang điểm, làm đẹp'),
(N'Thể thao & Du lịch',  N'Dụng cụ thể thao, đồ du lịch'),
(N'Sách & Văn phòng phẩm',N'Sách, dụng cụ học tập, văn phòng');
GO

-- 6. Products
INSERT INTO Products (Code, Name, Description, CategoryId, ImportPrice, SellingPrice, VATRate, StockQuantity, CreatedBy) VALUES
('PROD001', N'Iphone 15 Pro Max',    N'Điện thoại cao cấp Apple, Titan tự nhiên',    1, 25000000, 30000000, 10, 50, 1),
('PROD002', N'Macbook Air M2',       N'Laptop mỏng nhẹ, màu Midnight',               1, 18000000, 24000000, 10, 30, 1),
('PROD003', N'Áo thun Polo',         N'Áo thun nam cotton 100%, thoáng mát',         2,   150000,   350000,  8, 100, 1),
('PROD004', N'Nồi cơm điện Sharp',   N'Nồi cơm điện tử 1.8L, nấu ngon',             3,  1200000,  1800000, 10, 40, 1);
GO
-- 1. Thêm sản phẩm vào danh mục Điện tử (CategoryId = 1)
INSERT INTO Products (Code, Name, Description, CategoryId, ImportPrice, SellingPrice, VATRate, StockQuantity, Status, CreatedBy)
VALUES 
('E001', N'Tai Nghe Bluetooth Pro', N'Tai nghe chống ồn chủ động', 1, 800000, 1200000, 10, 20, 'Active', 1),
('E002', N'Sạc Dự Phòng 20.000mAh', N'Sạc nhanh PD 20W', 1, 300000, 450000, 10, 40, 'Active', 1);
Go
-- 2. Thêm sản phẩm vào danh mục Thời trang (CategoryId = 2)
INSERT INTO Products (Code, Name, Description, CategoryId, ImportPrice, SellingPrice, VATRate, StockQuantity, Status, CreatedBy)
VALUES 
('FA001', N'Áo Thun Cotton Trơn', N'Chất liệu 100% cotton thoáng mát, thấm mồ hôi', 2, 60000, 120000, 10, 300, 'Active', 1),
('FA002', N'Quần Jean Nam Dáng Straight', N'Vải denim co giãn nhẹ, giữ form tốt', 2, 150000, 280000, 10, 120, 'Active', 1);
Go
-- 3. Thêm sản phẩm vào danh mục Gia dụng (CategoryId = 3)
INSERT INTO Products (Code, Name, Description, CategoryId, ImportPrice, SellingPrice, VATRate, StockQuantity, Status, CreatedBy)
VALUES 
('GD001', N'Nồi Chiên Không Dầu 5L', N'Nồi chiên công nghệ Rapid Air, chống dính cao cấp', 3, 800000, 1500000, 10, 30, 'Active', 1),
('GD002', N'Máy Hút Bụi Cầm Tay', N'Lực hút mạnh mẽ, pin sạc nhanh 4000mAh', 3, 450000, 750000, 10, 45, 'Active', 1);
GO
-- 4. Thêm sản phẩm vào danh mục Thực phẩm & Đồ uống (CategoryId = 4)
INSERT INTO Products (Code, Name, Description, CategoryId, ImportPrice, SellingPrice, VATRate, StockQuantity, Status, CreatedBy)
VALUES 
('D001', N'Trà Sữa Trân Châu', N'Trà sữa truyền thống kèm trân châu đen', 4, 15000, 35000, 10, 100, 'Active', 1),
('D002', N'Cà Phê Muối', N'Cà phê đặc sản xứ Huế', 4, 12000, 25000, 10, 50, 'Active', 1),
('SN01', N'Bánh Tráng Trộn', N'Bánh tráng trộn Tây Ninh đầy đủ topping', 4, 10000, 20000, 5, 200, 'Active', 1),
('SN02', N'Cơm Cháy Chà Bông', N'Cơm cháy giòn tan, nhiều chà bông', 4, 25000, 45000, 5, 80, 'Active', 1);
Go
-- 5. Thêm sản phẩm vào danh mục Mỹ phẩm & Làm đẹp (CategoryId = 5)
INSERT INTO Products (Code, Name, Description, CategoryId, ImportPrice, SellingPrice, VATRate, StockQuantity, Status, CreatedBy)
VALUES 
('MP001', N'Sữa Rửa Mặt Tràm Trà', N'Dành cho da mụn, làm sạch sâu và dịu da', 5, 120000, 250000, 10, 150, 'Active', 1),
('MP002', N'Kem Chống Nắng SPF 50+', N'Chống nắng phổ rộng, nâng tone tự nhiên', 5, 180000, 320000, 10, 100, 'Active', 1);
GO
-- 6. Thêm sản phẩm vào danh mục Thể thao & Du lịch (CategoryId = 6)
INSERT INTO Products (Code, Name, Description, CategoryId, ImportPrice, SellingPrice, VATRate, StockQuantity, Status, CreatedBy)
VALUES 
('TT001', N'Thảm Tập Yoga TPE', N'Thảm TPE 6mm siêu bám, kèm túi đựng', 6, 100000, 190000, 10, 60, 'Active', 1),
('TT002', N'Vali Du Lịch 20 Inch', N'Nhựa ABS chống xước, bánh xe xoay 360 độ', 6, 350000, 650000, 10, 25, 'Active', 1);
GO
-- 7. Thêm sản phẩm vào danh mục Sách & Văn phòng phẩm (CategoryId = 7)
INSERT INTO Products (Code, Name, Description, CategoryId, ImportPrice, SellingPrice, VATRate, StockQuantity, Status, CreatedBy)
VALUES 
('VP001', N'Sổ Tay Bìa Da Cao Cấp', N'Sổ tay A5 giấy vàng chống lóa 100 trang', 7, 35000, 75000, 5, 200, 'Active', 1),
('VP002', N'Bút Bi Mực Nước Gel', N'Bút bi ngòi 0.5mm, mực ra đều không tắc', 7, 4000, 10000, 5, 500, 'Active', 1);
GO
-- 7. ProductImages
INSERT INTO ProductImages (ProductId, ImageUrl, IsPrimary) VALUES
(1, 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/iphone-15-pro-max_2__4_1.jpg', 1),
(1, 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/iphone-15-pro-max_4__4_1.jpg', 0),
(1, 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/iphone-15-pro-max_5__4_1.jpg', 0),
(1, 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/i/p/iphone-15-pro-max_7__4_1.jpg', 0),

(2, 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/v/n/vn0d33_1_1.jpg', 1),
(2, 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/v/n/vnb70f_1_1.jpg', 0),
(2, 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/v/n/vn_mac_1_3.jpg', 0),
(2, 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/v/n/vn380f_1_1.jpg', 0),
(2, 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/v/n/vn_mac_2_3.jpg', 0),
(2, 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/v/n/vnab9d_1_1.jpg', 0),

(3, 'https://dongphuctienbao.com/wp-content/uploads/2021/06/ao-polo-dep-2.jpg', 1),
(3, 'https://static.sonkimfashion.vn/static/file/image/jockey/jockey-ao-thun-polo-9441-trang-ao-9_b0b12c71eb48443a9daa2f29af858532_master.jpg', 0),
(3, 'https://salt.tikicdn.com/ts/tmp/61/67/b5/ae638ebf36f789c2e1d5ec30bc60340c.jpeg', 0),
(3, 'https://aoxuanhe.com/upload/product/axh-098/ao-thun-nam-polo-den-tron-cotton.jpg', 0),

(4, '/images/d388a63d-c5bc-466f-83f5-49d47abac6dd.jpg', 1),
(5, 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=500&q=80', 1),
(6, '/images/eae45629-936b-429b-9d2b-93823d6abca7.jpg', 1),
(7, 'https://images.unsplash.com/photo-1521572267360-ee0c2909d518?w=500&q=80', 1),
(8, '/images/792be640-a102-46d8-96a8-2e71cac3f333.jpg', 1),
(9, 'https://images.unsplash.com/photo-1584269600464-37b1b58a9fe7?w=500&q=80', 1),
(10, 'https://images.unsplash.com/photo-1558317374-067fb5f30001?w=500&q=80', 1),
(11, '/images/c95b0b84-afec-4c95-a8a3-bc82e12c5857.jpg', 1),
(12, 'https://images.unsplash.com/photo-1514432324607-a09d9b4aefdd?w=500&q=80', 1),
(13, 'https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?w=500&q=80', 1),
(14, 'https://images.unsplash.com/photo-1604382354936-07c5d9983bd3?w=500&q=80', 1),
(15, 'https://images.unsplash.com/photo-1556228720-195a672e8a03?w=500&q=80', 1),
(16, 'https://images.unsplash.com/photo-1598440947619-2c35fc9aa908?w=500&q=80', 1),
(17, 'https://images.unsplash.com/photo-1601925260368-ae2f83cf8b7f?w=500&q=80', 1),
(18, 'https://images.unsplash.com/photo-1565026057447-bc90a3dceb87?w=500&q=80', 1),
(19, 'https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=500&q=80', 1),
(20, 'https://images.unsplash.com/photo-1583485088034-697b5bc54ccd?w=500&q=80', 1);
GO

-- 8. InventoryTransactions
INSERT INTO InventoryTransactions (ProductId, Quantity, Type, Note, CreatedBy) VALUES
(1, 50, 'Import', N'Nhập kho lô hàng đầu tiên', 1),
(2, 30, 'Import', N'Nhập kho lô hàng đầu tiên', 1),
(3, 100,'Import', N'Nhập kho lô hàng đầu tiên', 1),
(4, 40, 'Import', N'Nhập kho lô hàng đầu tiên', 1);
GO

-- 9. Promotions
INSERT INTO Promotions (Code, DiscountType, Value, StartDate, EndDate, MinOrderValue, Status) VALUES
('SALEKHAI', 'Percent', 10,    GETDATE(), DATEADD(day, 30, GETDATE()), 500000, 'Active'),
('CHAOMUNG', 'Amount',  50000, GETDATE(), DATEADD(day, 90, GETDATE()), 200000, 'Active');
GO

-- 10. SystemSettings
INSERT INTO SystemSettings (SettingKey, SettingValue, GroupName, Description) VALUES
('CompanyName',    N'Sales Management',  'General',  N'Tên công ty'),
('CompanyAddress', N'Hà Nội, Việt Nam',  'General',  N'Địa chỉ công ty'),
('VATRate',        '10',                 'Finance',  N'Thuế VAT mặc định (%)'),
('CurrencySymbol', 'VND',               'Finance',  N'Đơn vị tiền tệ');
GO



-- ====================================================
-- XI. Finalize EF Migrations Tracking
-- ====================================================

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'InitialCreate', N'8.0.8');

GO
