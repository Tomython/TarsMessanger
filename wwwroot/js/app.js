// ============================================
// TarsMessenger - Основная логика приложения
// ============================================

class TarsMessenger {
    constructor() {
        this.token = localStorage.getItem('token');
        this.currentUser = JSON.parse(localStorage.getItem('currentUser') || 'null');
        this.connection = null;
        this.activeChat = null;
        this.chats = new Map(); // username -> { user, lastMessage, unreadCount, messages }
        this.users = new Map(); // username -> user data
        
        this.init();
    }

    init() {
        // Проверка авторизации
        if (this.token && this.currentUser) {
            this.showApp();
            this.connectSignalR();
        } else {
            this.showAuth();
        }

        // Обработчики авторизации
        this.setupAuthHandlers();
        
        // Обработчики навигации
        this.setupNavigationHandlers();
        
        // Обработчики чата
        this.setupChatHandlers();
        
        // Мобильная навигация
        this.setupMobileNavigation();
    }

    // ========== Авторизация ==========
    setupAuthHandlers() {
        document.getElementById('loginBtn')?.addEventListener('click', () => this.handleLogin());
        document.getElementById('registerBtn')?.addEventListener('click', () => this.handleRegister());
        document.getElementById('showRegisterBtn')?.addEventListener('click', () => this.toggleAuthForm());
        document.getElementById('showLoginBtn')?.addEventListener('click', () => this.toggleAuthForm());
        
        // Enter для отправки форм
        ['loginUsername', 'loginPassword', 'registerUsername', 'registerEmail', 'registerPassword'].forEach(id => {
            const input = document.getElementById(id);
            if (input) {
                input.addEventListener('keypress', (e) => {
                    if (e.key === 'Enter') {
                        if (id.startsWith('login')) {
                            this.handleLogin();
                        } else {
                            this.handleRegister();
                        }
                    }
                });
            }
        });
    }

    async handleLogin() {
        const username = document.getElementById('loginUsername').value.trim();
        const password = document.getElementById('loginPassword').value.trim();
        
        if (!username || !password) {
            this.showAuthError('Заполните все поля');
            return;
        }

        try {
            const response = await fetch('/api/auth/login', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ username, password })
            });

            const data = await response.json();
            
            if (!response.ok) {
                this.showAuthError(data.message || 'Ошибка входа');
                return;
            }

            this.token = data.token;
            this.currentUser = data.user;
            localStorage.setItem('token', this.token);
            localStorage.setItem('currentUser', JSON.stringify(this.currentUser));
            
            this.showApp();
            this.connectSignalR();
        } catch (error) {
            this.showAuthError('Ошибка подключения к серверу');
            console.error('Login error:', error);
        }
    }

    async handleRegister() {
        const username = document.getElementById('registerUsername').value.trim();
        const email = document.getElementById('registerEmail').value.trim();
        const password = document.getElementById('registerPassword').value.trim();
        
        if (!username || !password) {
            this.showAuthError('Заполните обязательные поля');
            return;
        }

        try {
            const response = await fetch('/api/auth/register', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ username, email, password })
            });

            const data = await response.json();
            
            if (!response.ok) {
                this.showAuthError(data.message || 'Ошибка регистрации');
                return;
            }

            this.token = data.token;
            this.currentUser = data.user;
            localStorage.setItem('token', this.token);
            localStorage.setItem('currentUser', JSON.stringify(this.currentUser));
            
            this.showApp();
            this.connectSignalR();
        } catch (error) {
            this.showAuthError('Ошибка подключения к серверу');
            console.error('Register error:', error);
        }
    }

    toggleAuthForm() {
        const loginForm = document.querySelector('.auth-form:not([style*="display: none"])');
        const registerForm = document.getElementById('registerForm');
        const isRegister = registerForm.style.display !== 'none';
        
        if (isRegister) {
            registerForm.style.display = 'none';
            loginForm.style.display = 'flex';
        } else {
            loginForm.style.display = 'none';
            registerForm.style.display = 'flex';
        }
        this.hideAuthError();
    }

    showAuthError(message) {
        const errorEl = document.getElementById('authError');
        if (errorEl) {
            errorEl.textContent = message;
            errorEl.style.display = 'block';
        }
    }

    hideAuthError() {
        const errorEl = document.getElementById('authError');
        if (errorEl) errorEl.style.display = 'none';
    }

    showAuth() {
        document.getElementById('authScreen').style.display = 'flex';
        document.getElementById('appContainer').style.display = 'none';
    }

    showApp() {
        document.getElementById('authScreen').style.display = 'none';
        document.getElementById('appContainer').style.display = 'flex';
    }

    // ========== SignalR ==========
    async connectSignalR() {
        if (!this.token) return;

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl("/chatHub", {
                accessTokenFactory: () => this.token
            })
            .withAutomaticReconnect()
            .build();

        // Обработчики событий
        this.connection.on("UserJoined", (username) => {
            console.log('User joined:', username);
            this.loadUsers();
        });

        this.connection.on("UserLeft", (username) => {
            console.log('User left:', username);
            this.loadUsers();
        });

        this.connection.on("UserListUpdate", (users) => {
            this.updateUsersList(users);
        });

        this.connection.on("ReceiveMessage", (fromUser, toUser, message, createdAt) => {
            this.handleReceiveMessage(fromUser, toUser, message, createdAt);
        });

        this.connection.on("ChatHistoryLoaded", (toUsername, history) => {
            if (this.activeChat === toUsername) {
                this.renderChatHistory(history);
            }
        });

        try {
            await this.connection.start();
            console.log('SignalR connected');
            this.loadUsers();
            this.loadChats();
        } catch (error) {
            console.error('SignalR connection error:', error);
            // Показываем ошибку пользователю только если это не первая попытка
            if (this.connectionRetryCount === undefined) {
                this.connectionRetryCount = 0;
            }
            this.connectionRetryCount++;
            
            if (this.connectionRetryCount > 3) {
                console.error('Failed to connect to SignalR after multiple attempts');
                // Можно показать уведомление пользователю
            } else {
                setTimeout(() => this.connectSignalR(), 3000);
            }
        }
    }

    // ========== Пользователи и чаты ==========
    async loadUsers() {
        try {
            const response = await fetch('/api/users', {
                headers: {
                    'Authorization': `Bearer ${this.token}`
                }
            });
            
            if (response.ok) {
                const users = await response.json();
                users.forEach(user => {
                    this.users.set(user.username, user);
                });
                this.updateChatsList();
            }
        } catch (error) {
            console.error('Error loading users:', error);
        }
    }

    updateUsersList(onlineUsers) {
        // Обновляем статусы пользователей
        this.users.forEach((user, username) => {
            const isOnline = onlineUsers.includes(username);
            user.isOnline = isOnline;
        });
        this.updateChatsList();
    }

    async loadChats() {
        // Загружаем список всех пользователей для чатов
        await this.loadUsers();
    }

    updateChatsList() {
        const chatsList = document.getElementById('chatsList');
        if (!chatsList) return;

        // Получаем всех пользователей кроме текущего
        const otherUsers = Array.from(this.users.values())
            .filter(u => u.username !== this.currentUser.username)
            .sort((a, b) => {
                const chatA = this.chats.get(a.username);
                const chatB = this.chats.get(b.username);
                
                if (!chatA && !chatB) return a.username.localeCompare(b.username);
                if (!chatA) return 1;
                if (!chatB) return -1;
                
                const timeA = chatA.lastMessage?.createdAt || 0;
                const timeB = chatB.lastMessage?.createdAt || 0;
                return timeB - timeA;
            });

        chatsList.innerHTML = '';

        if (otherUsers.length === 0) {
            chatsList.innerHTML = '<div style="padding: 2rem; text-align: center; color: var(--text-tertiary);">Нет доступных чатов</div>';
            return;
        }

        otherUsers.forEach(user => {
            const chat = this.chats.get(user.username) || { user, messages: [], unreadCount: 0 };
            const lastMessage = chat.lastMessage;
            const unreadCount = chat.unreadCount || 0;
            
            const chatItem = this.createChatItem(user, lastMessage, unreadCount);
            chatsList.appendChild(chatItem);
        });
    }

    createChatItem(user, lastMessage, unreadCount) {
        const item = document.createElement('div');
        item.className = 'chat-item';
        if (this.activeChat === user.username) {
            item.classList.add('active');
        }
        
        item.addEventListener('click', () => this.openChat(user.username));

        const initial = user.username.charAt(0).toUpperCase();
        const colors = ['#5865f2', '#ed4245', '#23a55a', '#f0b232', '#eb459e', '#5865f2'];
        const colorIndex = user.username.charCodeAt(0) % colors.length;
        const statusClass = user.isOnline ? 'online' : 'offline';
        
        let time = '';
        if (lastMessage && lastMessage.createdAt) {
            const msgDate = new Date(lastMessage.createdAt);
            const now = new Date();
            const diff = now - msgDate;
            const days = Math.floor(diff / 86400000);
            
            if (days === 0) {
                time = msgDate.toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' });
            } else if (days === 1) {
                time = 'Вчера';
            } else if (days < 7) {
                time = msgDate.toLocaleDateString('ru-RU', { weekday: 'short' });
            } else {
                time = msgDate.toLocaleDateString('ru-RU', { day: 'numeric', month: 'short' });
            }
        }
        
        const preview = lastMessage 
            ? (lastMessage.fromUsername === this.currentUser.username ? 'Вы: ' : '') + lastMessage.message
            : 'Нет сообщений';

        item.innerHTML = `
            <div class="chat-avatar" style="background: ${colors[colorIndex]}; color: white; font-weight: 600;">
                ${initial}
                <span class="status-indicator ${statusClass}"></span>
            </div>
            <div class="chat-info">
                <div class="chat-header-row">
                    <span class="chat-name">${this.escapeHtml(user.username)}</span>
                    ${time ? `<span class="chat-time">${time}</span>` : ''}
                </div>
                <div class="chat-preview">
                    <span>${this.escapeHtml(preview)}</span>
                    ${unreadCount > 0 ? `<span class="unread-badge">${unreadCount}</span>` : ''}
                </div>
            </div>
        `;

        return item;
    }

    createAvatar(username, size) {
        const initial = username.charAt(0).toUpperCase();
        const colors = ['#5865f2', '#ed4245', '#23a55a', '#f0b232', '#eb459e', '#5865f2'];
        const colorIndex = username.charCodeAt(0) % colors.length;
        return `<span style="width: ${size}px; height: ${size}px; border-radius: 50%; background: ${colors[colorIndex]}; display: inline-flex; align-items: center; justify-content: center; color: white; font-weight: 600; font-size: ${size * 0.4}px;">${initial}</span>`;
    }

    // ========== Чат ==========
    setupChatHandlers() {
        const messageInput = document.getElementById('messageInput');
        const sendBtn = document.getElementById('sendBtn');
        
        messageInput?.addEventListener('input', (e) => {
            sendBtn.disabled = !e.target.value.trim();
        });

        messageInput?.addEventListener('keypress', (e) => {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                this.sendMessage();
            }
        });

        sendBtn?.addEventListener('click', () => this.sendMessage());
    }

    async openChat(username) {
        if (this.activeChat === username) return;

        this.activeChat = username;
        
        // Обновляем UI
        document.getElementById('chatEmpty').style.display = 'none';
        document.getElementById('chatActive').style.display = 'flex';
        
        const user = this.users.get(username);
        if (user) {
            document.getElementById('chatHeaderName').textContent = user.username;
            const initial = user.username.charAt(0).toUpperCase();
            const colors = ['#5865f2', '#ed4245', '#23a55a', '#f0b232', '#eb459e', '#5865f2'];
            const colorIndex = user.username.charCodeAt(0) % colors.length;
            const avatarEl = document.getElementById('chatHeaderAvatar');
            avatarEl.style.background = colors[colorIndex];
            avatarEl.style.color = 'white';
            avatarEl.textContent = initial;
            
            const statusIndicator = document.getElementById('chatHeaderStatusIndicator');
            const statusText = document.getElementById('chatHeaderStatusText');
            
            if (user.isOnline) {
                statusIndicator.className = 'status-indicator online';
                statusText.textContent = 'В сети';
            } else {
                statusIndicator.className = 'status-indicator';
                statusText.textContent = 'Оффлайн';
            }
        }

        // Загружаем историю
        if (this.connection) {
            await this.connection.invoke("LoadChatHistory", username);
        }

        // Обновляем список чатов
        this.updateChatsList();
        
        // Мобильная навигация
        this.showChatOnMobile();
    }

    async sendMessage() {
        if (!this.activeChat || !this.connection) return;

        const input = document.getElementById('messageInput');
        const message = input.value.trim();
        
        if (!message) return;

        try {
            await this.connection.invoke("SendMessage", this.activeChat, message);
            input.value = '';
            document.getElementById('sendBtn').disabled = true;
        } catch (error) {
            console.error('Error sending message:', error);
        }
    }

    handleReceiveMessage(fromUser, toUser, message, createdAt) {
        const isToMe = toUser === this.currentUser.username;
        const isFromMe = fromUser === this.currentUser.username;
        const otherUser = isFromMe ? toUser : fromUser;

        // Добавляем сообщение в чат
        if (!this.chats.has(otherUser)) {
            this.chats.set(otherUser, {
                user: this.users.get(otherUser) || { username: otherUser },
                messages: [],
                unreadCount: 0
            });
        }

        const chat = this.chats.get(otherUser);
        const messageObj = {
            fromUsername: fromUser,
            toUsername: toUser,
            message,
            createdAt
        };

        chat.messages.push(messageObj);
        chat.lastMessage = messageObj;

        // Если чат активен, показываем сообщение
        if (this.activeChat === otherUser) {
            this.renderMessage(messageObj, isFromMe);
            chat.unreadCount = 0;
        } else if (isToMe) {
            // Увеличиваем счетчик непрочитанных
            chat.unreadCount = (chat.unreadCount || 0) + 1;
        }

        // Обновляем список чатов
        this.updateChatsList();
    }

    renderChatHistory(history) {
        const container = document.getElementById('messagesContainer');
        if (!container) return;
        
        container.innerHTML = '';

        if (!history || history.length === 0) {
            return;
        }

        let currentDate = null;
        history.forEach(msg => {
            // Обрабатываем разные форматы даты
            let msgDate;
            if (typeof msg.createdAt === 'string') {
                msgDate = new Date(msg.createdAt);
            } else if (msg.createdAt instanceof Date) {
                msgDate = msg.createdAt;
            } else {
                msgDate = new Date();
            }
            
            const dateStr = this.formatDate(msgDate);
            
            if (dateStr !== currentDate) {
                currentDate = dateStr;
                const divider = document.createElement('div');
                divider.className = 'date-divider';
                divider.innerHTML = `<span>${currentDate}</span>`;
                container.appendChild(divider);
            }

            const isFromMe = msg.fromUsername === this.currentUser.username;
            this.renderMessage(msg, isFromMe);
        });

        this.scrollToBottom();
    }

    renderMessage(msg, isFromMe) {
        const container = document.getElementById('messagesContainer');
        if (!container) return;
        
        const wrapper = document.createElement('div');
        wrapper.className = `message-wrapper ${isFromMe ? 'sent' : 'received'}`;

        const bubble = document.createElement('div');
        bubble.className = 'message-bubble';

        const text = document.createElement('div');
        text.className = 'message-text';
        text.textContent = msg.message || msg.text || '';

        const time = document.createElement('div');
        time.className = 'message-time';
        
        let msgDate;
        if (msg.createdAt) {
            if (typeof msg.createdAt === 'string') {
                msgDate = new Date(msg.createdAt);
            } else if (msg.createdAt instanceof Date) {
                msgDate = msg.createdAt;
            } else {
                msgDate = new Date();
            }
        } else {
            msgDate = new Date();
        }
        
        time.textContent = this.formatTime(msgDate);

        bubble.appendChild(text);
        bubble.appendChild(time);
        wrapper.appendChild(bubble);
        container.appendChild(wrapper);

        this.scrollToBottom();
    }

    scrollToBottom() {
        const container = document.getElementById('messagesContainer');
        if (container) {
            container.scrollTop = container.scrollHeight;
        }
    }

    // ========== Навигация ==========
    setupNavigationHandlers() {
        document.getElementById('backToChatsBtn')?.addEventListener('click', () => {
            this.activeChat = null;
            document.getElementById('chatEmpty').style.display = 'flex';
            document.getElementById('chatActive').style.display = 'none';
            this.showChatsOnMobile();
        });
    }

    setupMobileNavigation() {
        const backBtn = document.getElementById('backToChatsBtn');
        if (backBtn) {
            backBtn.addEventListener('click', () => this.showChatsOnMobile());
        }
    }

    showChatsOnMobile() {
        if (window.innerWidth <= 768) {
            document.getElementById('chatsPanel').classList.remove('hidden');
            document.getElementById('chatPanel').classList.add('hidden');
        }
    }

    showChatOnMobile() {
        if (window.innerWidth <= 768) {
            document.getElementById('chatsPanel').classList.add('hidden');
            document.getElementById('chatPanel').classList.remove('hidden');
        }
    }

    // ========== Утилиты ==========
    formatTime(date) {
        if (!date || isNaN(date.getTime())) return '';
        
        const now = new Date();
        const diff = now - date;
        const minutes = Math.floor(diff / 60000);
        
        if (minutes < 1) return 'только что';
        if (minutes < 60) return `${minutes}м назад`;
        
        const hours = Math.floor(minutes / 60);
        if (hours < 24) return `${hours}ч назад`;
        
        return date.toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' });
    }

    formatDate(date) {
        if (!date || isNaN(date.getTime())) return '';
        
        const now = new Date();
        now.setHours(0, 0, 0, 0);
        const dateOnly = new Date(date);
        dateOnly.setHours(0, 0, 0, 0);
        
        const diff = now - dateOnly;
        const days = Math.floor(diff / 86400000);
        
        if (days === 0) return 'Сегодня';
        if (days === 1) return 'Вчера';
        if (days < 7) {
            const weekday = date.toLocaleDateString('ru-RU', { weekday: 'long' });
            return weekday.charAt(0).toUpperCase() + weekday.slice(1);
        }
        
        return date.toLocaleDateString('ru-RU', { day: 'numeric', month: 'long' });
    }

    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }
}

// Инициализация приложения
let app;
document.addEventListener('DOMContentLoaded', () => {
    app = new TarsMessenger();
});
