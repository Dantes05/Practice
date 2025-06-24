using Serilog.Context;

namespace ToDoApi.Middleware
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private const string CorrelationIdHeader = "X-Correlation-ID";

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
                              ?? Guid.NewGuid().ToString();

            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                context.Response.Headers.Append(CorrelationIdHeader, correlationId);
                await _next(context);
            }
        }
    }
}
