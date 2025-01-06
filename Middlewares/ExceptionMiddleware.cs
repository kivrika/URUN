using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;
namespace URUN.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context); // Sonraki middleware'e geç
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex); // Hata durumunda yakala ve işle
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        // 500 Internal Server Error durumu
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        // Hata mesajını JSON formatında hazırla
        var response = new
        {
            StatusCode = context.Response.StatusCode,
            Message = "Beklenmeyen bir hata olustu.",
            Details = exception.Message // Geliştirme ortamında hata detayı gösterilebilir
        };

        return context.Response.WriteAsJsonAsync(response); // Yanıtı JSON formatında döndür
    }
}
