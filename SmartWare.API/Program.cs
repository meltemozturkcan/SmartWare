using Microsoft.EntityFrameworkCore;
using SmartWare.API.Data;
using Microsoft.OpenApi.Models;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // appsettings.json'dan connection string'i oku
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    // SQL Server kullan
    options.UseSqlServer(connectionString);

    // Development modunda detaylı loglar (opsiyonel)
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging(); // SQL parametrelerini göster
        options.EnableDetailedErrors();       // Detaylı hata mesajları
    }
});
// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
// ============================================
// 1. DbContext
// ============================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});
// ============================================
// 2. Controllers + JSON Options (Circular Reference Fix)
// ============================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Circular reference'ları ignore et
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;

        // Null değerleri ignore et (opsiyonel)
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });
// ============================================
// 3. Swagger (API Documentation)
// ============================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Version = "v1",
        Title = "SmartWare API",
        Description = "Akıllı Depo Yönetim Sistemi - Blog ve Dashboard API",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "SmartWare Team",
            Email = "info@smartware.com"
        }
    });
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy => 
 // Add this using directive
    {
        policy.WithOrigins("http://localhost:4200") // Angular default port
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Swagger - Sadece Development'ta
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartWare API v1");
        options.RoutePrefix = string.Empty;  // Swagger root'ta açılır: https://localhost:5001/
    });
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowAngular"); // CORS'u aktif et
app.UseAuthorization();

app.MapControllers();

app.Run();

// ============================================
// AÇIKLAMALAR
// ============================================

// AddDbContext:
//   - DbContext'i DI container'a ekler
//   - Controller'larda constructor injection ile kullanılabilir
//   - Default olarak Scoped lifetime (her HTTP request için yeni instance)

// UseSqlServer:
//   - SQL Server provider'ı aktif eder
//   - EF Core'a hangi veritabanını kullanacağını söyler

// EnableSensitiveDataLogging:
//   - SQL sorgularında parametre değerlerini gösterir
//   - SADECE development'ta kullan (güvenlik riski)
//   - Örnek: SELECT * FROM Posts WHERE Id = 1 (parametre görünür)

// EnableDetailedErrors:
//   - Hata mesajlarında daha fazla detay
//   - Production'da kapatılmalı

// CORS:
//   - Cross-Origin Resource Sharing
//   - Angular (localhost:4200) → API (localhost:5000) çağrıları için gerekli
//   - Production'da sadece gerçek domain'i ekle