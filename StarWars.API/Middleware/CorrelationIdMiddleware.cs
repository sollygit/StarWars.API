using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using StarWars.Model;
using System;
using System.Threading.Tasks;

namespace StarWars.API.Middleware
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private const string CorrelationHeader = Constants.X_CORRELATION_ID;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ILogger<CorrelationIdMiddleware> logger)
        {
            // Check for incoming correlation ID
            if (!context.Request.Headers.TryGetValue(CorrelationHeader, out var correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
            }

            // Store in HttpContext.Items for access in controllers/services
            context.Items[CorrelationHeader] = correlationId.ToString();

            // Add to response headers so clients can see it
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[CorrelationHeader] = correlationId.ToString();
                return Task.CompletedTask;
            });

            using (logger.BeginScope("{CorrelationId}", correlationId!))
            {
                await _next(context);
            }
        }
    }
}
