using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Security.Cryptography;
using System.Text;

namespace SalesManagement.Web.Filters
{
    /**
     * PURE C# PROTECTION against rapid-clicks and duplicate submissions.
     * Use this instead of JavaScript to prevent multiple API calls.
     */
    public class PreventDuplicateSubmissionAttribute : ActionFilterAttribute
    {
        private readonly int _lockWindowMs;
        
        public PreventDuplicateSubmissionAttribute(int lockWindowMs = 1000)
        {
            _lockWindowMs = lockWindowMs;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var cache = context.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();
            var request = context.HttpContext.Request;

            // Generate a unique fingerprint for this specific user request
            var userId = context.HttpContext.User.Identity?.Name ?? "Anonymous";
            var requestPath = request.Path.Value;
            var requestMethod = request.Method;
            
            var keyBuilder = new StringBuilder();
            keyBuilder.Append(userId).Append("|").Append(requestPath).Append("|").Append(requestMethod);

            // Fingerprint with Query and Form data
            foreach (var key in request.Query.Keys)
                keyBuilder.Append(key).Append("=").Append(request.Query[key]);

            if (request.HasFormContentType)
            {
                foreach (var key in request.Form.Keys)
                    keyBuilder.Append(key).Append("=").Append(request.Form[key]);
            }

            var cacheKey = "SubmitLock_" + ComputeHash(keyBuilder.ToString());

            if (cache.TryGetValue(cacheKey, out _))
            {
                // Conflict detected: Double click or rapid interaction
                context.Result = new BadRequestObjectResult("Your request is being processed. Please wait a moment.");
                return;
            }

            // Set lock for the specified window
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMilliseconds(_lockWindowMs));
            
            cache.Set(cacheKey, true, cacheOptions);

            base.OnActionExecuting(context);
        }

        private string ComputeHash(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(bytes);
        }
    }
}
