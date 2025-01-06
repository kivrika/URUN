using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using URUN.Data;
using URUN.Middlewares; 
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog yapılandırması
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console() // Konsola loglama
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day) // Dosyaya günlük loglama
    .CreateLogger();

// ASP.NET Core log sistemine Serilog'u entegre et
builder.Host.UseSerilog();

// JWT Ayarlarını yükleyin
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// JSON ve Swagger Ayarları
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Veritabanı Ayarı
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Ortam ayarları
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware'ler
app.UseMiddleware<ApiKeyMiddleware>();      // API Key kontrolü
app.UseMiddleware<ExceptionMiddleware>();  // Global hata yönetimi
app.UseMiddleware<LoggingMiddleware>();    // İstek/yanıt loglaması
app.UseMiddleware<RateLimitingMiddleware>();// Rate limiting

app.UseMiddleware<CustomAuthorizationMiddleware>();


app.UseHttpsRedirection();

// JWT Authentication Middleware
app.UseAuthentication(); // Kimlik doğrulama
app.UseAuthorization();  // Yetkilendirme

// Controller'leri haritala
app.MapControllers();
app.MapGet("/", () => "SALAMM");    

app.Run();
