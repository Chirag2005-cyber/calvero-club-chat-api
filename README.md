# 🚀 Calvero Club Chat API

<img src="https://chat.calvero.club/favicon.ico" width="64" height="64" alt="Calvero Club Logo" />

> ⭐ **Star, contribute and join the community!**

---

## 📝 About

An open-source, modern real-time chat API built for the Calvero Club Chat community! Focused on high performance, security, and scalability. The user interface and API are provided in separate repositories under a unified community brand.

- **API & Server:** [`calvero-club-chat-api`](https://github.com/Xjectro/calvero-club-chat-api)
- **Web Interface:** [`calvero-club-chat-web`](https://github.com/Xjectro/calvero-club-chat-web)
- **Demo:** [chat.calvero.club](https://chat.calvero.club)

---

## ✨ Features

- ⚡ Real-time chat (SignalR)
- 🔒 JWT-based authentication
- 👤 User management & registration
- 💬 Chat rooms & messaging
- 🚦 Rate limit & validation middleware
- 🧩 Modern .NET 9 architecture
- 🧹 Clean and readable code

---

## ⚙️ Installation

### Requirements
- .NET 9 SDK
- SQL Server or SQLite

### Getting Started

```powershell
# Clone the repository

cd calvero-club-chat-api

# Install dependencies
---

# Apply database migrations


# Start the application

```

### Environment Variables
Configure your connection strings and JWT settings in `appsettings.Development.json` and `appsettings.Production.json`.

---

## 📡 API Usage

You can test API endpoints using the `Api.http` file or Postman.

### Example Endpoints
- `POST /api/auth/register` — User registration
- `POST /api/auth/login` — Login
- `GET /api/chat/rooms` — List chat rooms
- `POST /api/chat/rooms` — Create new room
- `POST /api/chat/messages` — Send message

---

## 🖥️ Web Interface

The user interface is available at [`calvero-club-chat-web`](https://github.com/Xjectro/calvero-club-chat-web) and works seamlessly with this API. For real-time messaging and community experience, check out the web repository.

---

## 🤝 Contributing

Pull requests and suggestions are always welcome! Fork, star, and send a PR! ✨

---

## 📄 License

MIT

---

> 👨‍💻 Developer: [Xjectro](https://github.com/Xjectro)
> 
> 🚀 Demo & test: [chat.calvero.club](https://chat.calvero.club)