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

builder.Services.AddControllers();
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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
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