using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:8080");

// ТОЛЬКО SignalR + Controllers
builder.Services.AddControllers().AddJsonOptions(opt => 
    opt.JsonSerializerOptions.PropertyNamingPolicy = null);
builder.Services.AddSignalR();

// CORS для фронта + SignalR
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});

var app = builder.Build();

// Middleware порядок
app.UseCors("AllowAll");
app.UseStaticFiles();
app.MapControllers();
app.MapHub<ChatHub>("/chatHub");
app.MapFallbackToFile("index.html");

app.Run();
