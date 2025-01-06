using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace URUN.Middlewares
{
    public class CustomAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;

        public CustomAuthorizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Authorization hatalarını yakala
            context.Response.OnStarting(() =>
            {
                if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
                {
                    context.Response.ContentType = "application/json";
                    var response = new
                    {
                        Message = "Erisim yetkiniz yok." // 403 için mesaj
                    };
                    var jsonResponse = System.Text.Json.JsonSerializer.Serialize(response);
                    return context.Response.WriteAsync(jsonResponse);
                }
                else if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
                {
                    context.Response.ContentType = "application/json";
                    var response = new
                    {
                        Message = "Kimlik dogrulama basarısız. Lütfen giriş yapın." // 401 için mesaj
                    };
                    var jsonResponse = System.Text.Json.JsonSerializer.Serialize(response);
                    return context.Response.WriteAsync(jsonResponse);
                }

                return Task.CompletedTask;
            });

            await _next(context);
        }
    }
}
