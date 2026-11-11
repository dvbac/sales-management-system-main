using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;

using System.Security.Claims;
using SalesManagement.DAL.Data;
using SalesManagement.DAL.Entities;
using SalesManagement.DAL.Interfaces;
using SalesManagement.DAL.Repositories;
using SalesManagement.BLL.Interfaces;
using SalesManagement.BLL.Services;
using SalesManagement.Web.Hubs;
using SalesManagement.Web.Filters;
using System;

var builder = WebApplication.CreateBuilder(args);

// 1. Add services to the container.
builder.Services.AddControllersWithViews(options => {
    options.Filters.Add<PerformanceActionFilter>();
})
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();
builder.Services.AddSignalR();
builder.Services.AddMemoryCache();

// Register encoding provider for extra code pages (like Windows-1258 if needed)
System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

// 1. Dependency Injection: DbContext (from DAL)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DBDefault"),
        b => b.MigrationsAssembly("SalesManagement.DAL")));

// 2. Dependency Injection: Repositories (DAL)
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

// 3. Dependency Injection: Services (BLL)
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPayrollService, PayrollService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IPromotionService, PromotionService>();
builder.Services.AddScoped<ISettingService, SettingService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<ILeaveRequestService, LeaveRequestService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IWishlistService, WishlistService>();
builder.Services.AddScoped<IProductReviewService, ProductReviewService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
// Authentication & Session Configuration
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(10);
        options.SlidingExpiration = true;
        options.Cookie.Name = "SalesManagement.Auth";
    })
    .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

        options.Scope.Add("email");
        options.Scope.Add("profile");

        options.Events.OnCreatingTicket = async context =>
        {
            var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();

            var googleSub = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? context.Principal?.FindFirstValue("sub");
            var email = context.Principal?.FindFirstValue(ClaimTypes.Email)
                ?? context.Principal?.FindFirstValue("email");
            var fullName = context.Principal?.FindFirstValue(ClaimTypes.Name)
                ?? context.Principal?.FindFirstValue("name");

            if (string.IsNullOrWhiteSpace(googleSub) || string.IsNullOrWhiteSpace(email))
            {
                throw new InvalidOperationException("Google authentication did not return required claims (sub/email).");
            }

            var normalizedEmail = email.Trim();
            var user = await db.Users.FirstOrDefaultAsync(u => u.GoogleId == googleSub);
            if (user == null)
            {
                user = await db.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);
            }

            if (user != null && (user.IsDeleted || !user.IsActive))
            {
                throw new InvalidOperationException("User account is disabled.");
            }

            if (user == null)
            {
                user = new User
                {
                    Username = normalizedEmail,
                    Email = normalizedEmail,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString("N")),
                    FullName = fullName,
                    GoogleId = googleSub,
                    Role = "Customer",
                    IsActive = true,
                    IsDeleted = false,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                db.Users.Add(user);
                await db.SaveChangesAsync();

                var customer = new Customer
                {
                    UserId = user.UserId,
                    FullName = user.FullName ?? user.Username,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    CreatedDate = DateTime.Now,
                    Type = "Personal",
                    CustomerLevel = "Regular"
                };
                db.Customers.Add(customer);
                await db.SaveChangesAsync();

            }
            else if (string.IsNullOrWhiteSpace(user.GoogleId))
            {
                user.GoogleId = googleSub;
                user.UpdatedDate = DateTime.Now;
                await db.SaveChangesAsync();
            }

            // Replace principal with app cookie identity
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("FullName", user.FullName ?? "")
            };

            context.Principal = new ClaimsPrincipal(
                new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        };
    });

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// 2. Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
provider.Mappings[".webp"] = "image/webp";
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

// Custom Middlewares
app.UseMiddleware<SalesManagement.Web.Middleware.SalesSessionMiddleware>();

// Cấu hình Routing
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapHub<SystemHub>("/systemHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed default admin account from appsettings.json on first run
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try 
    {
        // Tự động tạo bảng Carts và CartItems nếu chưa có
        await context.Database.ExecuteSqlRawAsync(@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Carts')
            BEGIN
                CREATE TABLE Carts (
                    CartId INT PRIMARY KEY IDENTITY(1,1),
                    UserId INT NOT NULL,
                    CreatedDate DATETIME DEFAULT GETDATE(),
                    UpdatedDate DATETIME DEFAULT GETDATE(),
                    CONSTRAINT FK_Carts_Users FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
                );
            END

            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CartItems')
            BEGIN
                CREATE TABLE CartItems (
                    CartItemId INT PRIMARY KEY IDENTITY(1,1),
                    CartId INT NOT NULL,
                    ProductId INT NOT NULL,
                    Quantity INT NOT NULL,
                    CONSTRAINT FK_CartItems_Carts FOREIGN KEY (CartId) REFERENCES Carts(CartId) ON DELETE CASCADE,
                    CONSTRAINT FK_CartItems_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId)
                );
            END
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Wishlists')
            BEGIN
                CREATE TABLE Wishlists (
                    WishlistId INT PRIMARY KEY IDENTITY(1,1),
                    UserId INT NOT NULL,
                    ProductId INT NOT NULL,
                    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
                    CONSTRAINT FK_Wishlists_Users FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
                    CONSTRAINT FK_Wishlists_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId) ON DELETE CASCADE
                );
            END

            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProductReviews')
            BEGIN
                CREATE TABLE ProductReviews (
                    ReviewId INT PRIMARY KEY IDENTITY(1,1),
                    ProductId INT NOT NULL,
                    UserId INT NOT NULL,
                    Rating INT NOT NULL CHECK(Rating >= 1 AND Rating <= 5),
                    Comment NVARCHAR(1000) NULL,
                    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
                    IsVerifiedPurchase BIT NOT NULL DEFAULT 0,
                    CONSTRAINT FK_ProductReviews_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId) ON DELETE CASCADE,
                    CONSTRAINT FK_ProductReviews_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
                );
            END

            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Notifications')
            BEGIN
                CREATE TABLE Notifications (
                    NotificationId INT PRIMARY KEY IDENTITY(1,1),
                    UserId INT NULL,
                    Title NVARCHAR(200) NOT NULL,
                    Message NVARCHAR(MAX) NOT NULL,
                    IsRead BIT NOT NULL DEFAULT 0,
                    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
                    NotificationType NVARCHAR(50) NULL,
                    ActionUrl NVARCHAR(255) NULL,
                    CONSTRAINT FK_Notifications_Users FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
                );
            END
        ");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error updating schema: {ex.Message}");
    }
}

await SalesManagement.Web.Extensions.AdminSeeder.SeedDefaultAdminAsync(
    app.Services,
    builder.Configuration);

app.Run();
