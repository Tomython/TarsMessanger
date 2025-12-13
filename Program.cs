using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:8080");  // Фикс порта
builder.Services.AddSignalR();
builder.Services.AddCors(options => 
    options.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

app.UseCors();
app.UseStaticFiles();  // ← КРИТИЧНО для wwwroot
app.MapHub<ChatHub>("/chatHub");
app.MapFallbackToFile("index.html");  // ← КРИТИЧНО для /

app.Run();
