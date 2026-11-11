using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;

namespace SalesManagement.Web.Filters
{
    public class PerformanceActionFilter : IAsyncActionFilter
    {
        private readonly ILogger<PerformanceActionFilter> _logger;

        public PerformanceActionFilter(ILogger<PerformanceActionFilter> logger)
        {
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var stopwatch = Stopwatch.StartNew();
            
            var resultContext = await next();

            stopwatch.Stop();
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            if (elapsedMilliseconds > 500)
            {
                var controllerName = context.RouteData.Values["controller"];
                var actionName = context.RouteData.Values["action"];
                _logger.LogWarning("SLOW ACTION DETECTED: {Controller}.{Action} took {ElapsedMatch}ms", 
                    controllerName, actionName, elapsedMilliseconds);
            }
        }
    }
}
