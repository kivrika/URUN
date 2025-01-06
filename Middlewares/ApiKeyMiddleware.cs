using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace URUN.Middlewares
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // appsettings.json'dan API Key'i al
            var expectedApiKey = _configuration["ApiKey"];
            Console.WriteLine($"Beklenen API Anahtari: {expectedApiKey}"); // API Anahtari logla

            if (string.IsNullOrEmpty(expectedApiKey))
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync("Hata: API anahtari eksik.");
                return;
            }

            if (!context.Request.Headers.TryGetValue("X-Api-Key", out var extractedApiKey))
            {
                Console.WriteLine("Istek header'i 'X-Api-Key' eksik."); // Eksik header logla
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Yetkisiz istek. API anahtari eksik.");
                return;
            }

            Console.WriteLine($"Istekte cikarilan API Anahtari: {extractedApiKey}"); // Gelen API Anahtari logla

            if (extractedApiKey != expectedApiKey)
            {
                Console.WriteLine($"Eslesme yok: Beklenen '{expectedApiKey}', ancak alinan '{extractedApiKey}'.");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Yetkisiz istek. API anahtari gecersiz.");
                return;
            }

            Console.WriteLine("API Anahtari gecerli.");
            await _next(context);
        }
    }
}
