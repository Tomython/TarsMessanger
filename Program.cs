using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:8080");
builder.Services.AddSignalR();
builder.Services.AddCors(options => 
    options.AddDefaultPolicy(p => 
        p.WithOrigins("http://192.168.0.150:5000")  // ← ТОЧНЫЙ URL
         .AllowAnyMethod().AllowAnyHeader()
         .AllowCredentials()));  // ← КРИТИЧНО

var app = builder.Build();
app.UseCors();
app.UseStaticFiles();
app.MapHub<ChatHub>("/chatHub");
app.MapFallbackToFile("index.html");
app.Run();

