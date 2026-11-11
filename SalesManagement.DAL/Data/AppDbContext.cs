using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SalesManagement.DAL.Entities;

namespace SalesManagement.DAL.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Server=NHOTUNG\\SQLEXPRESS;Database=SalesManagement;User Id=sa;Password=123;TrustServerCertificate=True;");
        }
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<InventoryTransaction> InventoryTransactions { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<OrderPromotion> OrderPromotions { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductImage> ProductImages { get; set; }

    public virtual DbSet<Promotion> Promotions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<TimeAttendance> TimeAttendances { get; set; }

    public virtual DbSet<Payroll> Payrolls { get; set; }

    public virtual DbSet<SystemSetting> SystemSettings { get; set; }



    public virtual DbSet<Shift> Shifts { get; set; }

    public virtual DbSet<LeaveRequest> LeaveRequests { get; set; }

    public virtual DbSet<SalaryComponent> SalaryComponents { get; set; }

    public virtual DbSet<EmployeeSalaryComponent> EmployeeSalaryComponents { get; set; }

    public virtual DbSet<Cart> Carts { get; set; }
    public virtual DbSet<CartItem> CartItems { get; set; }

    public virtual DbSet<Wishlist> Wishlists { get; set; }
    public virtual DbSet<ProductReview> ProductReviews { get; set; }
    public virtual DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__19093A0B6A3254B2");

            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.ImageUrl).HasMaxLength(255);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
            entity.Property(e => e.UpdatedDate).HasDefaultValueSql("(getdate())").HasColumnType("datetime");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("FK__Categorie__Paren__5FB337D6");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PK__Customer__A4AE64D8E06D21A9");

            entity.HasIndex(e => e.UserId, "UQ__Customer__1788CC4DFB907F10").IsUnique();

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CustomerLevel)
                .HasMaxLength(20)
                .HasDefaultValue("Regular");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Type)
                .HasMaxLength(20)
                .HasDefaultValue("Personal");

            entity.HasOne(d => d.User).WithOne(p => p.Customer)
                .HasForeignKey<Customer>(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__Customers__UserI__4BAC3F29");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("PK__Employee__7AD04F11523887C0");

            entity.HasIndex(e => e.UserId, "UQ__Employee__1788CC4D33A8FEBD").IsUnique();

            entity.Property(e => e.BasicSalary)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(15, 2)");
            entity.Property(e => e.HourlyWage).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Position).HasMaxLength(100);

            entity.HasOne(d => d.User).WithOne(p => p.Employee)
                .HasForeignKey<Employee>(d => d.UserId)
                .HasConstraintName("FK__Employees__UserI__4316F928");

            entity.HasOne(d => d.Shift).WithMany(p => p.Employees)
                .HasForeignKey(d => d.ShiftId)
                .HasConstraintName("FK_Employees_Shifts");
        });

        modelBuilder.Entity<InventoryTransaction>(entity =>
        {
            entity.HasKey(e => e.TransId).HasName("PK__Inventor__9E5DDB3CB8966C99");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Type).HasMaxLength(50);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.InventoryTransactions)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__Inventory__Creat__75A278F5");

            entity.HasOne(d => d.Product).WithMany(p => p.InventoryTransactions)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inventory__Produ__74AE54BC");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.InvoiceId).HasName("PK__Invoices__D796AAB5BD005D0B");

            entity.Property(e => e.Amount).HasColumnType("decimal(15, 2)");
            entity.Property(e => e.InvoiceDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Issued");

            entity.HasOne(d => d.Order).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Invoices__OrderI__1AD3FDA4");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Orders__C3905BCF616BF451");

            entity.HasIndex(e => e.OrderDate, "IX_Orders_OrderDate");
            entity.HasIndex(e => e.Status, "IX_Orders_Status");

            entity.Property(e => e.DiscountAmount)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(15, 2)");
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(20)
                .HasDefaultValue("Cash");
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(20)
                .HasDefaultValue("Unpaid");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Draft");
            entity.Property(e => e.SubTotal)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(15, 2)");
            entity.Property(e => e.TaxAmount)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(15, 2)");
            entity.Property(e => e.TotalAmount)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(15, 2)");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__Orders__CreatedB__0A9D95DB");

            entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Orders__Customer__09A971A2");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => e.OrderDetailId).HasName("PK__OrderDet__D3B9D36CEFE3A47D");

            // Composite index: speeds up the top-selling products GROUP BY
            entity.HasIndex(e => new { e.OrderId, e.ProductId }, "IX_OrderDetails_OrderId_ProductId");

            entity.Property(e => e.Total).HasColumnType("decimal(15, 2)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(15, 2)");
            entity.Property(e => e.Vatrate)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("VATRate");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK__OrderDeta__Order__0E6E26BF");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderDeta__Produ__0F624AF8");
        });

        modelBuilder.Entity<OrderPromotion>(entity =>
        {
            entity.HasKey(e => e.OrderPromotionId).HasName("PK__OrderPro__CE84A101BF8FF200");

            entity.HasIndex(e => new { e.OrderId, e.PromotionId }, "UQ_Order_Promotion").IsUnique();

            entity.Property(e => e.AppliedValue).HasColumnType("decimal(15, 2)");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderPromotions)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK__OrderProm__Order__14270015");

            entity.HasOne(d => d.Promotion).WithMany(p => p.OrderPromotions)
                .HasForeignKey(d => d.PromotionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderProm__Promo__151B244E");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__Products__B40CC6CD13EA28AD");

            entity.HasIndex(e => e.Code, "UQ__Products__A25C5AA7E59F43F4").IsUnique();
            entity.HasIndex(e => e.Status, "IX_Products_Status");
            entity.HasIndex(e => e.CreatedDate, "IX_Products_CreatedDate");

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ImportPrice)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(15, 2)");
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.SellingPrice).HasColumnType("decimal(15, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");
            entity.Property(e => e.StockQuantity).HasDefaultValue(0);
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Vatrate)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("VATRate");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__Products__Catego__6A30C649");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Products)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__Products__Create__6B24EA82");
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("PK__ProductI__7516F70C980B3A71");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ImageUrl).HasMaxLength(255);
            entity.Property(e => e.IsPrimary).HasDefaultValue(false);

            entity.HasOne(d => d.Product).WithMany(p => p.ProductImages)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__ProductIm__Produ__6FE99F9F");
        });

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasKey(e => e.PromotionId).HasName("PK__Promotio__52C42FCF0EEAC269");

            entity.HasIndex(e => e.Code, "UQ__Promotio__A25C5AA75DF7DCD5").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(20);
            entity.Property(e => e.DiscountType).HasMaxLength(20);
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.MaxDiscountAmount).HasColumnType("decimal(15, 2)");
            entity.Property(e => e.MinOrderValue)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(15, 2)");
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");
            entity.Property(e => e.Value).HasColumnType("decimal(15, 2)");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4CD261EB97");

            entity.HasIndex(e => e.Email, "IX_Users_Email");

            entity.HasIndex(e => e.Username, "IX_Users_Username");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E472BD1BD6").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534F5621627").IsUnique();

            entity.Property(e => e.Avatar).HasMaxLength(255);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.GoogleId).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastLogin).HasColumnType("datetime");
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasDefaultValue("Customer");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        modelBuilder.Entity<TimeAttendance>(entity =>
        {
            entity.HasKey(e => e.AttendanceId);
            entity.HasOne(d => d.Employee).WithMany(p => p.TimeAttendances)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            entity.Property(e => e.Date).HasColumnType("date");

            entity.HasOne(d => d.Shift).WithMany(p => p.TimeAttendances)
                .HasForeignKey(d => d.ShiftId)
                .HasConstraintName("FK_TimeAttendances_Shifts");
            entity.Property(e => e.DeductionAmount).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<Shift>(entity =>
        {
            entity.HasKey(e => e.ShiftId);
            entity.Property(e => e.ShiftName).HasMaxLength(50);
            entity.Property(e => e.StartTime).HasColumnType("time");
            entity.Property(e => e.EndTime).HasColumnType("time");
        });

        modelBuilder.Entity<Payroll>(entity =>
        {
            entity.HasKey(e => e.PayrollId);
            entity.HasOne(d => d.Employee).WithMany(p => p.Payrolls)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            entity.Property(e => e.BaseSalary).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Benefits).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Bonus).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Penalty).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalAllowances).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalDeductions).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TaxAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.NetSalary).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalSalary).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<SalaryComponent>(entity =>
        {
            entity.Property(e => e.DefaultAmount).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<EmployeeSalaryComponent>(entity =>
        {
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.CartId);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.HasOne(d => d.User).WithMany().HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => e.CartItemId);
            entity.HasOne(d => d.Cart).WithMany(p => p.CartItems).HasForeignKey(d => d.CartId);
            entity.HasOne(d => d.Product).WithMany().HasForeignKey(d => d.ProductId);
        });

        OnModelCreatingPartial(modelBuilder);
        modelBuilder.Entity<User>().HasData(
    new User
    {
        UserId = 1,
        Username = "admin",
        PasswordHash = "$2a$11$V2vX7h6X5C8X7iY6A/X9ueTf8S8iA8vE8f7kS8l7U8v8G8iE8f7kS", // Hashed "123456"
        Email = "admin@gmail.com",
        Role = "Admin",
        IsActive = true,
        CreatedDate = DateTime.Now
    },
    new User
    {
        UserId = 2,
        Username = "sale",
        PasswordHash = "$2a$11$V2vX7h6X5C8X7iY6A/X9ueTf8S8iA8vE8f7kS8l7U8v8G8iE8f7kS", // Hashed "123456"
        Email = "sale@gmail.com",
        Role = "Sales",
        IsActive = true,
        CreatedDate = DateTime.Now
    }

); 
modelBuilder.Entity<Category>().HasData(
    new Category { CategoryId = 1, Name = "Đồ uống", Status = "Active", IsDeleted = false },
    new Category { CategoryId = 2, Name = "Đồ ăn vặt", Status = "Active", IsDeleted = false },
    new Category { CategoryId = 3, Name = "Thực phẩm", Status = "Active", IsDeleted = false },
    new Category { CategoryId = 4, Name = "Điện tử", Status = "Active", IsDeleted = false },
    new Category { CategoryId = 5, Name = "Thời trang", Status = "Active", IsDeleted = false },
    new Category { CategoryId = 6, Name = "Đồ gia dụng", Status = "Active", IsDeleted = false },
    new Category { CategoryId = 7, Name = "Sách", Status = "Active", IsDeleted = false }
);

        modelBuilder.Entity<Employee>().HasData(
    new Employee
    {
        EmployeeId = 1,
        UserId = 2, // Linked to 'sale' user
        Position = "Sales Staff",
        BasicSalary = 5000000,
        StartWorkingDate = DateOnly.FromDateTime(DateTime.Now),
        Department = "Sales",
        ContractType = "Full-time",
        IsDeleted = false
    }
);


    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
