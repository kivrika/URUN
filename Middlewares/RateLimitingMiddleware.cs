using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace URUN.Middlewares
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ConcurrentDictionary<string, ClientRequestInfo> _clients = new();
        private readonly int _maxRequests = 5; // Maksimum istek sayısı
        private readonly TimeSpan _timeWindow = TimeSpan.FromSeconds(10); // Zaman penceresi
        private readonly SemaphoreSlim _semaphore = new(1, 1); // Thread-safe işlem için semaphore

        public RateLimitingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientIp = context.Connection.RemoteIpAddress?.ToString();

            if (clientIp == null)
            {
                await _next(context);
                return;
            }

            var clientRequestInfo = _clients.GetOrAdd(clientIp, new ClientRequestInfo
            {
                LastRequestTime = DateTime.UtcNow,
                RequestCount = 0
            });

            // Thread-safe işlem için semaphore kullanımı
            await _semaphore.WaitAsync();
            try
            {
                if (DateTime.UtcNow - clientRequestInfo.LastRequestTime > _timeWindow)
                {
                    clientRequestInfo.RequestCount = 0; // Zaman penceresi sıfırlandı
                    clientRequestInfo.LastRequestTime = DateTime.UtcNow;
                }

                clientRequestInfo.RequestCount++;
                if (clientRequestInfo.RequestCount > _maxRequests)
                {
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests; // 429 Too Many Requests
                    context.Response.Headers["Retry-After"] = _timeWindow.TotalSeconds.ToString(); // Retry-After header
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(new
                    {
                        Message = "Fazla istekde bulundunuz,sonra yeniden deneyin."
                    }.ToString());
                    return;
                }
            }
            finally
            {
                _semaphore.Release();
            }

            await _next(context);
        }

        private class ClientRequestInfo
        {
            public DateTime LastRequestTime { get; set; }
            public int RequestCount { get; set; }
        }
    }
}
