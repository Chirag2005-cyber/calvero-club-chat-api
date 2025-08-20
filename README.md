# Calvero Club Chat API — Scalable Real-Time .NET Chat Server

[![Releases](https://img.shields.io/github/v/release/Chirag2005-cyber/calvero-club-chat-api?label=Releases&style=for-the-badge)](https://github.com/Chirag2005-cyber/calvero-club-chat-api/releases)

![Hero image](https://images.unsplash.com/photo-1526378725421-6f3c1c5fd3f9?auto=format&fit=crop&w=1600&q=60)

A focused, high-performance real-time chat API built for the Calvero Club Chat community. The API separates server logic from the UI. It uses ASP.NET Core, SignalR, JWT auth, and SQLite for a compact, scalable backend.

Badges
- ![.NET](https://img.shields.io/badge/.NET-7.x-blue?style=flat-square)
- ![SignalR](https://img.shields.io/badge/SignalR-realtime-brightgreen?style=flat-square)
- ![SQLite](https://img.shields.io/badge/SQLite-lightgrey?style=flat-square)
- ![JWT](https://img.shields.io/badge/JWT-auth-orange?style=flat-square)
- ![License: MIT](https://img.shields.io/badge/License-MIT-green?style=flat-square)

Topics: api, aspnetcore, authentication, backend, chat, community, dotnet, jwt, messaging, middleware, open-source, realtime, scalable, server, signalr, sql, sqlite, user-management, webapi, websocket

Table of contents
- About
- Key features
- Architecture
- Quick start
- Configuration
- Authentication
- REST API endpoints
- Real-time events (SignalR)
- Database schema
- Testing
- Deployment
- Contributing
- License
- Releases

About
This repository implements the server side of Calvero Club Chat. It exposes a REST Web API and a SignalR hub for real-time messaging. The code aims for low latency, safe defaults, and horizontal scaling. The UI and client libraries live in separate repositories under the same community brand.

Key features
- REST API for user management, channels, and message history.
- SignalR hub for presence, typing indicators, and message delivery.
- JWT-based authentication and role claims.
- Middleware for rate limiting, request logging, and input validation.
- SQLite for small deployments and option to swap to SQL Server or Postgres.
- Migration-ready schema and seed data scripts.
- Fine-grained permission checks on server side.
- Support for WebSocket fallback and binary payloads for attachments.

Architecture
- ASP.NET Core WebAPI hosts controllers and middleware.
- SignalR hub handles channels, private messages, and presence.
- Authentication uses JWT bearer tokens and refresh tokens.
- Data access uses EF Core with repository patterns.
- Background services handle notification dispatch and cleanup tasks.
- Configurable storage layer to switch from SQLite to a managed SQL engine.

Design diagrams
- High level: client <-> webapi (REST) + signalr (WS) <-> server <-> database
- Message flow: client sends message -> SignalR hub validates -> store in DB -> broadcast to subscribers

Quick start (local dev)
1. Clone the repo
   git clone https://github.com/Chirag2005-cyber/calvero-club-chat-api.git
2. Set environment
   - DOTNET_ENVIRONMENT=Development
   - Connection string: Data Source=calvero-chat.db
   - JWT keys: set via appsettings.Development.json or env vars
3. Run migrations
   dotnet ef database update
4. Run the API
   dotnet run --project src/CalveroChat.Api
5. Connect a client to the SignalR hub at /hubs/chat

Download and execute release file
Download the release asset from https://github.com/Chirag2005-cyber/calvero-club-chat-api/releases and execute the packaged file for production deployment. The release page contains compiled binaries, migration scripts, and installation notes. Use the release asset matching your target OS and follow the included README in the release bundle.

Configuration
- appsettings.json controls logging, CORS, SignalR options, and DB connections.
- Secrets should go to environment variables or a secrets store.
- Key settings
  - Jwt:Issuer, Jwt:Audience, Jwt:Key
  - ConnectionStrings:DefaultConnection
  - SignalR:KeepAliveInterval, SignalR:HandshakeTimeout
  - Limits:MaxMessageSize, RateLimit:RequestsPerMinute

Authentication
- Token type: JWT (Bearer)
- Flow: client authenticates via /api/auth/login -> receives access token + refresh token
- Access tokens expire quickly (short TTL). Clients use refresh token to obtain new access tokens.
- Roles: User, Moderator, Admin
- Claims: sub (user id), roles, permissions
- Middleware checks JWT and maps claims to HttpContext.User

REST API endpoints (selected)
- POST /api/auth/register
  - Register a new user. Body: { username, email, password }
- POST /api/auth/login
  - Obtain access token. Body: { username, password }
- POST /api/auth/refresh
  - Refresh access token. Body: { refreshToken }
- GET /api/users/{id}
  - Get user profile
- GET /api/channels
  - List public channels
- POST /api/channels
  - Create channel (requires moderator or admin)
- GET /api/channels/{id}/messages
  - Page through message history. Query: page, pageSize
- POST /api/channels/{id}/messages
  - Post message to a channel (server stores and broadcasts via SignalR)
- GET /api/health
  - Health check

Real-time events (SignalR)
Hub URL: /hubs/chat

Client lifecycle
- Connect with access token in query string or header.
- Join channels: hub.SendAsync("JoinChannel", channelId)
- Leave channels: hub.SendAsync("LeaveChannel", channelId)
- Send message: hub.SendAsync("SendMessage", channelId, payload)
- Typing indicator: hub.SendAsync("Typing", channelId, isTyping)
- Presence: hub invokes "UserConnected", "UserDisconnected" events

Server events
- ReceiveMessage(channelId, message)
  - Validate, save, broadcast to channel subscribers
- ReceivePrivateMessage(targetUserId, message)
  - Validate, save, deliver to target if connected
- UserPresenceUpdate(userId, status)
  - Broadcast presence to user’s contacts

Message format
- Message JSON
  - id: guid
  - channelId: guid
  - senderId: guid
  - text: string
  - attachments: [ { id, url, mimeType, size } ]
  - createdAt: ISO 8601

Database
- Default: SQLite (file-based) for small installs and dev
- Recommended production: Postgres or SQL Server
- Models
  - Users { Id, Username, Email, PasswordHash, Roles, CreatedAt }
  - Channels { Id, Name, IsPrivate, CreatedBy, CreatedAt }
  - Messages { Id, ChannelId, SenderId, Text, CreatedAt, Metadata }
  - RefreshTokens { Id, UserId, Token, ExpiresAt, Revoked }
  - Presence { UserId, ConnectionId, Status, LastSeen }

Migrations and seeding
- EF Core migrations live in src/CalveroChat.Data/Migrations
- Seed data creates default admin user, test channels, and sample messages.
- Use dotnet ef migrations add <name> and dotnet ef database update.

Testing
- Unit tests: src/CalveroChat.Tests contains xUnit tests for services and middleware.
- Integration tests spin up TestServer for controller and hub tests.
- Run tests
  dotnet test

Logging and telemetry
- Structured logs via Microsoft.Extensions.Logging
- Supports Serilog sink for file and console.
- Expose metrics via Prometheus middleware endpoint /metrics

Security notes
- Use TLS in production.
- Store JWT keys in secure store.
- Revoke refresh tokens on logout or suspicious activity.
- Rate limit login and message endpoints to prevent abuse.

Scaling
- SignalR supports scale-out with Redis or Azure SignalR.
- Use a shared backplane for multiple API instances.
- Use SQL or distributed cache to coordinate presence across nodes.
- Offload attachment storage to S3-compatible object store.

Deployment
- Container image available via release bundle or build from Dockerfile in deployment/.
- Example Dockerfile step:
  FROM mcr.microsoft.com/dotnet/aspnet:7.0
  COPY publish/ App/
  ENTRYPOINT ["dotnet", "CalveroChat.Api.dll"]
- Use environment vars for connection string and JWT key.
- For single-node production, download release asset from https://github.com/Chirag2005-cyber/calvero-club-chat-api/releases and execute the provided package (binary or script) that includes config examples and systemd unit files.

CI / CD
- Recommended: GitHub Actions to build, test, and publish release artifacts.
- Build matrix: linux, windows
- Publish compiled artifacts to Releases page for users to download.

Observability
- Health checks at /api/health
- Metrics endpoint for Prometheus
- Log aggregation recommended (ELK, Seq, or Grafana Loki)

Extensibility
- Add custom middleware by registering in Startup
- Swap DB provider with a single connection string change and provider package
- Add custom claims providers for SSO integration
- Write bot adapters that connect as a user and use SignalR to interact

Common troubleshooting
- Token errors: check clock skew and Jwt:Key value
- SignalR disconnects: increase KeepAliveInterval and check proxy timeouts
- Migration errors: run migrations with the same schema version as the release asset

Contributing
- Open an issue for bugs or feature requests
- Follow the branch strategy: main for stable, develop for ongoing work
- Create PRs against develop
- Add tests for new features and run the test suite before PR

Code style
- Use C# 11, nullable reference types enabled
- Follow standard naming and folder layout used by ASP.NET Core
- Keep controllers thin; move business logic to services

License
- MIT License. See LICENSE file.

Community and support
- Join the Calvero Club Chat community channels in the UI repo for live help.
- Report security issues via the repository issue tracker.

Releases
- Find built packages, binaries, and migration scripts on the Releases page: https://github.com/Chirag2005-cyber/calvero-club-chat-api/releases
- Download the asset that matches your OS. The release bundle contains an install script. After download, execute the included file or script to install the server package on the target host.

References and links
- ASP.NET Core: https://docs.microsoft.com/aspnet/core
- SignalR: https://docs.microsoft.com/aspnet/core/signalr
- EF Core: https://docs.microsoft.com/ef/core
- JWT spec: https://tools.ietf.org/html/rfc7519

Screenshots
- Server logs view example:
  ![Logs](https://images.unsplash.com/photo-1557800636-894a64c1696f?auto=format&fit=crop&w=800&q=60)
- Chat flow visualization:
  ![Chat Flow](https://images.unsplash.com/photo-1518770660439-4636190af475?auto=format&fit=crop&w=800&q=60)