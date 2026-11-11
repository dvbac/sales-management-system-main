using System.Security.Claims;
using SalesManagement.BLL.Interfaces;

namespace SalesManagement.Web.Middleware
{
    public class SalesSessionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SalesSessionMiddleware> _logger;

        public SalesSessionMiddleware(RequestDelegate next, ILogger<SalesSessionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider)
        {
            var user = context.User;
            var path = context.Request.Path.Value?.ToLower() ?? "";

            _logger.LogInformation($"Processing request: {path} User: {user.Identity?.Name} Role: {(user.IsInRole("Sales") ? "Sales" : "Other")}");

            // Bỏ qua các trang Account, logoff, static files
            if (user.Identity == null || !user.Identity.IsAuthenticated || 
                !user.IsInRole("Sales") || 
                path.StartsWith("/account") || 
                path.StartsWith("/lib") || 
                path.StartsWith("/css") || 
                path.StartsWith("/js") ||
                path.StartsWith("/images"))
            {
                await _next(context);
                return;
            }

            // Logic kiểm tra Check-in có thể được thêm ở đây
            // Hiện tại pass-through để đảm bảo không chặn request hợp lệ
            await _next(context);
        }
    }
}
