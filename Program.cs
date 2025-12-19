using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:8080");

builder.Services.AddControllers().AddJsonOptions(opt => 
    opt.JsonSerializerOptions.PropertyNamingPolicy = null);
builder.Services.AddSignalR();

// ✅ ФИКС CORS (убрать AllowCredentials)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
    // ❌ УБРАТЬ .AllowCredentials()!
});

var app = builder.Build();

app.UseCors("AllowAll");
app.UseStaticFiles();
app.MapControllers();
app.MapHub<ChatHub>("/chatHub");
app.MapFallbackToFile("index.html");

app.Run();
