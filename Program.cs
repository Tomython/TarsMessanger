using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:8080");

// Controllers + PostgreSQL
builder.Services.AddControllers();  // ← ДОБАВИТЬ ЭТУ СТРОКУ
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql("Host=host.docker.internal;Port=5432;Database=messenger;Username=messenger;Password=SecurePass123"));

// JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("super-secret-key-min-32-chars-long")),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddSignalR();
builder.Services.AddCors(options => 
    options.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.MapHub<ChatHub>("/chatHub");
app.MapControllers();  // Теперь работает!
app.MapFallbackToFile("index.html");
app.Run();