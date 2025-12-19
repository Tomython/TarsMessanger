# TarsMessenger 🚀

**Реал-тайм мессенджер 1:1 с WebRTC-звонками** на базе ASP.NET Core 8, SignalR и PostgreSQL.

## 📋 Описание проекта

TarsMessenger — это современный веб-мессенджер для приватного общения один-на-один с поддержкой:
- Реал-тайм обмена сообщениями через SignalR
- История сообщений с сохранением в PostgreSQL
- Онлайн-статусы пользователей
- WebRTC-звонки (SDP signaling)
- JWT-авторизация с refresh tokens
- Современный адаптивный UI

## ✅ Текущий статус функционала

### Реализовано

- ✅ **База данных PostgreSQL**
  - Таблицы `Users` и `Messages` через EF Core
  - Автоматическое применение миграций при запуске
  - Индексы для оптимизации запросов истории чатов

- ✅ **JWT-авторизация**
  - Регистрация пользователей (`/api/auth/register`)
  - Вход в систему (`/api/auth/login`)
  - JWT-токены с поддержкой в SignalR-хабе
  - Refresh tokens для обновления сессий
  - Хеширование паролей через BCrypt

- ✅ **SignalR чат 1:1**
  - Реал-тайм отправка и получение сообщений
  - Загрузка истории сообщений (последние 50)
  - Отметка сообщений как прочитанных
  - Автоматическое сохранение в БД

- ✅ **Онлайн-статусы**
  - Отслеживание подключенных пользователей
  - Обновление списка онлайн в реальном времени
  - Уведомления о входе/выходе пользователей

- ✅ **WebRTC signaling**
  - Передача SDP offer/answer через SignalR
  - Обмен ICE candidates для установки соединения
  - Готово для реализации полноценных звонков

- ✅ **Редизайн UI**
  - Экран авторизации (логин/регистрация)
  - Интерфейс чата с пузырями сообщений (sent/received)
  - Адаптивный дизайн для мобильных и десктопов
  - Список онлайн-пользователей

- ✅ **Docker Compose**
  - Полный стек: PostgreSQL + приложение
  - Health checks для БД
  - Volume для сохранения данных

### В планах

- ⏳ Групповые чаты
- ⏳ Отправка медиафайлов (изображения, файлы)
- ⏳ Push-уведомления
- ⏳ Улучшения безопасности (rate limiting, валидация)
- ⏳ Профили пользователей (аватары, статусы)
- ⏳ Поиск по сообщениям
- ⏳ Звонки (полная реализация WebRTC)

## 🏗️ Архитектура

### Frontend

Файлы находятся в `wwwroot/`:
- `index.html` — основной HTML с экранами авторизации и чата
- `js/app.js` — клиентская логика (SignalR, WebRTC, UI)
- `css/design-system.css` — дизайн-система (цвета, типографика)
- `css/components.css` — компоненты UI (кнопки, формы, чат)

### Backend

**Слои архитектуры:**

1. **Controllers** (`Controllers/`)
   - `AuthController.cs` — регистрация и авторизация
   - `UsersController.cs` — получение списка пользователей (требует авторизации)

2. **SignalR Hub** (`Hubs/`)
   - `ChatHub.cs` — реал-тайм коммуникация:
     - `SendMessage` — отправка сообщений
     - `LoadChatHistory` — загрузка истории
     - `SendOffer/SendAnswer/SendIceCandidate` — WebRTC signaling

3. **Services** (`Services/`)
   - `IMessengerService.cs` — интерфейс сервиса
   - `MessengerService.cs` — бизнес-логика:
     - Управление онлайн-пользователями
     - Сохранение и загрузка сообщений
     - Работа с историей чатов

4. **Data Layer** (`Data/`)
   - `MessengerDbContext.cs` — EF Core DbContext с конфигурацией моделей

5. **Models** (`Models/`)
   - `User.cs` — модель пользователя (Id, Username, Email, PasswordHash, RefreshToken)
   - `Message.cs` — модель сообщения (Id, SenderId, ReceiverId, Text, CreatedAt, IsRead)

6. **Configuration** (`Program.cs`)
   - Настройка JWT-авторизации
   - Регистрация SignalR
   - CORS для фронтенда
   - Автоматическое применение миграций

### Database

**PostgreSQL 15** с таблицами:

- **Users**
  - `Id` (PK, int)
  - `Username` (unique, required, max 50)
  - `Email` (unique, required, max 100)
  - `PasswordHash` (required)
  - `RefreshToken` (nullable)
  - `RefreshTokenExpiry` (nullable)

- **Messages**
  - `Id` (PK, int)
  - `SenderId` (FK → Users)
  - `ReceiverId` (FK → Users)
  - `Text` (required, max 2000)
  - `CreatedAt` (required, DateTime)
  - `IsRead` (bool, default false)
  - Индексы на `(SenderId, ReceiverId, CreatedAt)` и `(ReceiverId, SenderId, CreatedAt)` для быстрых запросов истории

## 🚀 Запуск приложения

### Вариант A: Локально без Docker

#### Требования

- .NET 8 SDK
- PostgreSQL 15 (установлен и запущен)

#### Шаги

1. **Установите зависимости:**
   ```bash
   # Установите .NET 8 SDK с https://dotnet.microsoft.com/download
   # Установите PostgreSQL 15
   ```

2. **Настройте строку подключения:**

   Отредактируйте `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=messenger;Username=messenger;Password=SecurePass123"
     },
     "Jwt": {
       "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!ChangeInProduction!",
       "Issuer": "TarsMessenger",
       "Audience": "TarsMessenger"
     }
   }
   ```

   Или используйте переменные окружения:
   ```bash
   export ConnectionStrings__DefaultConnection="Host=localhost;Database=messenger;Username=messenger;Password=SecurePass123"
   export Jwt__Key="YourSuperSecretKeyThatIsAtLeast32CharactersLong!ChangeInProduction!"
   ```

3. **Создайте базу данных:**
   ```bash
   # Подключитесь к PostgreSQL и создайте БД
   psql -U postgres
   CREATE DATABASE messenger;
   CREATE USER messenger WITH PASSWORD 'SecurePass123';
   GRANT ALL PRIVILEGES ON DATABASE messenger TO messenger;
   ```

4. **Примените миграции EF Core:**
   ```bash
   dotnet ef database update
   ```
   
   Если миграций еще нет, создайте их:
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

5. **Запустите приложение:**
   ```bash
   dotnet run
   ```

6. **Откройте в браузере:**
   ```
   http://localhost:5000
   ```

### Вариант B: Через Docker Compose (полный стек)

#### Требования

- Docker
- Docker Compose

#### Шаги

1. **Запустите полный стек:**
   ```bash
   docker-compose up -d
   ```

2. **Проверьте логи миграций:**
   ```bash
   docker logs tarsmessanger_app
   ```
   
   Должна быть строка: `✅ Database migrations applied successfully` или `Applied migration InitialCreate`

3. **Проверьте, что всё работает:**

   **Тест регистрации:**
   ```bash
   curl -X POST http://localhost:5000/api/auth/register \
     -H "Content-Type: application/json" \
     -d '{"username":"alice","password":"123","email":"alice@example.com"}'
   ```
   
   Ожидаемый ответ:
   ```json
   {
     "message": "Registered successfully",
     "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
     "refreshToken": "...",
     "user": {"id": 1, "username": "alice", "email": "alice@example.com"}
   }
   ```

   **Откройте приложение в браузере:**
   ```
   http://localhost:5000
   ```

4. **Остановка:**
   ```bash
   docker-compose down
   ```
   
   **Остановка с удалением данных:**
   ```bash
   docker-compose down -v
   ```

## 📡 API

### Авторизация

#### `POST /api/auth/register`
Регистрация нового пользователя.

**Request:**
```json
{
  "username": "alice",
  "password": "123",
  "email": "alice@example.com"  // необязательно
}
```

**Response:**
```json
{
  "message": "Registered successfully",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "...",
  "user": {"id": 1, "username": "alice", "email": "alice@example.com"}
}
```

#### `POST /api/auth/login`
Вход в систему.

**Request:**
```json
{
  "username": "alice",
  "password": "123"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "...",
  "user": {"id": 1, "username": "alice", "email": "alice@example.com"}
}
```

### Пользователи

#### `GET /api/users`
Получение списка всех пользователей (требует авторизации).

**Headers:**
```
Authorization: Bearer <token>
```

**Response:**
```json
[
  {"id": 1, "username": "alice", "email": "alice@example.com"},
  {"id": 2, "username": "bob", "email": "bob@example.com"}
]
```

### SignalR Hub

**URL хаба:** `/chatHub`

**Подключение с JWT:**
```javascript
const connection = new signalR.HubConnectionBuilder()
  .withUrl("/chatHub?access_token=" + token)
  .build();
```

**Методы хаба (клиент → сервер):**
- `SendMessage(toUsername, message)` — отправить сообщение
- `LoadChatHistory(toUsername)` — загрузить историю чата
- `SendOffer(toUsername, offer)` — WebRTC offer
- `SendAnswer(toUsername, answer)` — WebRTC answer
- `SendIceCandidate(toUsername, candidate)` — WebRTC ICE candidate

**События хаба (сервер → клиент):**
- `ReceiveMessage(fromUsername, toUsername, message, createdAt)` — новое сообщение
- `ChatHistoryLoaded(toUsername, history)` — загружена история
- `UserListUpdate(usernames[])` — обновлен список онлайн
- `UserJoined(username)` — пользователь подключился
- `UserLeft(username)` — пользователь отключился
- `ReceiveOffer/ReceiveAnswer/ReceiveIceCandidate` — WebRTC signaling

## 🛠️ Разработка

### Запуск в режиме разработки

1. **Локально:**
   ```bash
   dotnet watch run
   ```
   
   Приложение будет автоматически перезапускаться при изменении кода.

2. **С Docker Compose:**
   ```bash
   # Измените ASPNETCORE_ENVIRONMENT=Development в docker-compose.yml
   docker-compose up -d --build
   ```

### Работа с миграциями

**Создание новой миграции:**
```bash
dotnet ef migrations add <MigrationName>
```

**Применение миграций:**
```bash
dotnet ef database update
```

**Откат миграции:**
```bash
dotnet ef database update <PreviousMigrationName>
```

**Удаление последней миграции (если еще не применена):**
```bash
dotnet ef migrations remove
```

### Перезапуск Docker-стека без потери данных

Данные PostgreSQL сохраняются в Docker volume `postgres_data`:

```bash
# Остановка без удаления volume
docker-compose down

# Перезапуск (данные сохранятся)
docker-compose up -d

# Полная очистка с удалением данных
docker-compose down -v
```

### Просмотр логов

```bash
# Логи приложения
docker logs tarsmessanger_app -f

# Логи PostgreSQL
docker logs tarsmessanger_postgres -f

# Все логи
docker-compose logs -f
```

## 🗺️ Roadmap / TODO

### Ближайшие планы

- [ ] **Групповые чаты**
  - Создание групп
  - Управление участниками
  - Групповые сообщения

- [ ] **Медиафайлы**
  - Загрузка изображений
  - Отправка файлов
  - Хранение в объектном хранилище (S3/MinIO)

- [ ] **Push-уведомления**
  - Web Push API
  - Уведомления о новых сообщениях

- [ ] **Улучшения безопасности**
  - Rate limiting для API
  - Валидация входных данных
  - Защита от XSS/CSRF
  - Двухфакторная аутентификация (2FA)

- [ ] **Профили пользователей**
  - Аватары
  - Статусы (онлайн/офлайн/занят)
  - О себе

- [ ] **Поиск и фильтрация**
  - Поиск по сообщениям
  - Фильтрация чатов
  - Архивирование чатов

- [ ] **WebRTC звонки**
  - Полная реализация видеозвонков
  - Аудиозвонки
  - Экран в экране

- [ ] **Дополнительные функции**
  - Реакции на сообщения
  - Ответы на сообщения (reply)
  - Пересылка сообщений
  - Удаление сообщений

---

**Разработано с использованием:**
- ASP.NET Core 8
- SignalR
- Entity Framework Core
- PostgreSQL
- JWT Bearer Authentication
- WebRTC
- Docker & Docker Compose
