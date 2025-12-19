# TarsMessenger 🚀

**Реал-тайм чат + WebRTC звонки** (SignalR + PostgreSQL). Raspberry Pi → Tailscale → Render.com.

## 🎯 Текущий статус MVP
✅ SignalR чат 1:1 (iMessage UI)
✅ Список онлайн пользователей
✅ Приватные сообщения (пузыри)
✅ Responsive дизайн (мобильный)
✅ WebRTC звонки (SDP signaling готов)
✅ Docker (SD-карта Jarvis)
✅ Tailscale: http://100.101.109.97:5000
⏳ PostgreSQL БД
⏳ JWT авторизация
⏳ Render.com deploy


## 🏗️ Архитектура
Frontend: index.html (SignalR + WebRTC)
Backend: ASP.NET Core 8 + SignalR + EF Core
Database: PostgreSQL 15 (Users, Messages, JWT)
Infra: Docker → Jarvis:5000 → Tailscale


## 🚀 Быстрый старт (ЧАТ БЕЗ БД)
cd ~/TarsMessanger
docker build -t tarsmessenger .
docker run -d -p 5000:8080 --name tarsmessenger tarsmessenger

**Тест**: http://100.101.109.97:5000 → alice + bob чатят!

## 🗄️ PostgreSQL + JWT (Полный стек)
1. Docker Compose (БД + App)
docker-compose up -d

2. Миграции EF Core (авто)
docker logs tarsmessanger_app # "Applied migration InitialCreate"

3. Регистрация
curl -X POST http://localhost:5000/api/auth/register
-d '{"username":"alice","password":"123"}'

→ {"message":"Registered","token":"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."}

## 📝 docker-compose.yml (создать)
version: '3.8'
services:
postgres:
image: postgres:15
environment:
POSTGRES_DB: messenger
POSTGRES_USER: messenger
POSTGRES_PASSWORD: SecurePass123
volumes:
- postgres_data:/var/lib/postgresql/data
ports:
- "5433:5432"

tarsmessenger:
build: .
ports:
- "5000:8080"
environment:
ConnectionStrings__DefaultConnection: "Host=postgres;Database=messenger;Username=messenger;Password=SecurePass123"
depends_on:
- postgres

volumes:
postgres_data:


## 🔐 JWT + EF Core Models
// User.cs
public class User
{
public int Id { get; set; }
public string Username { get; set; } = "";
public string Email { get; set; } = "";
public string PasswordHash { get; set; } = "";
public string? RefreshToken { get; set; }
public DateTime? RefreshTokenExpiry { get; set; }
}

// Миграции
dotnet ef migrations add InitialCreate
dotnet ef database update


## 📱 Функционал MVP
✅ Чат 1:1 (пузыри sent/received)
✅ Список онлайн (клик → чат)
✅ Enter отправляет
✅ Автокомплит usernames
✅ Responsive (iPhone/десктоп)
⏳ JWT /api/auth/register+login
⏳ Регистрация и авторизация
⏳ Чтение сообщений
⏳ Дизайн на телефон и компьютер
⏳ WebRTC 📞 звонки (SDP готов)
⏳ История сообщений (БД)


## 📞 Tailscale / Prod
🔸 Jarvis dev: http://100.101.109.97:5000
🔸 Render prod: tarsmessenger.onrender.com
🔸 БД: postgres.render.com:5432
