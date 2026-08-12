# WalletCQRS

> 🎓 **A learning project** built to explore and practice **CQRS** (Command Query Responsibility Segregation) with **Clean Architecture** principles in ASP.NET Core.

This repository is intentionally simple on purpose. It is meant to be a **study reference** for understanding how to structure a .NET solution into multiple projects, separate commands from queries, and keep the application layer decoupled from the web framework.

---

## 📋 Table of Contents

- [About this project](#-about-this-project)
- [Technologies](#-technologies)
- [Architecture](#-architecture)
- [Project structure](#-project-structure)
- [How it works](#-how-it-works)
- [Getting started](#-getting-started)
- [API endpoints](#-api-endpoints)
- [Key concepts learned](#-key-concepts-learned)
- [Roadmap / next steps](#-roadmap--next-steps)

---

## 🎯 About this project

**WalletCQRS** is a minimal wallet API that allows you to:

- Create a wallet
- Deposit money into a wallet
- Check a wallet's balance

While the business domain is tiny, the real purpose is to demonstrate a **production-ready folder/architecture layout**:

- **Multi-project solution** (Domain, Application, Infrastructure, Api)
- **CQRS** separation between **commands** (writes) and **queries** (reads)
- **Dependency inversion** — high-level layers depend on abstractions, not on low-level implementations
- **Decoupled Application layer** — handlers return plain results, not HTTP types

> ⚠️ **Important:** This is a **study project**. The data layer uses an **in-memory repository**, so all data is lost when the application stops. There is no database, authentication, or persistence.

---

## 🧰 Technologies

| Technology | Purpose |
|---|---|
| [.NET 10](https://dotnet.microsoft.com/) | Framework / runtime |
| [ASP.NET Core](https://learn.microsoft.com/aspnet/core) | Web API (Minimal APIs) |
| [MediatR](https://github.com/jbogard/MediatR) | Implements the mediator pattern for CQRS |
| [Scalar](https://scalar.com/) | API reference / documentation UI |
| [Microsoft.AspNetCore.OpenApi](https://learn.microsoft.com/aspnet/core/fundamentals/openapi) | OpenAPI document generation |

---

## 🏗️ Architecture

This solution follows a **Clean Architecture** style layout split into four projects. The dependency rule points **inward**: `Domain` has no dependencies, and each outer layer may only depend on the layers closer to the center.

```
                    ┌──────────────────────┐
                    │       Api (Web)       │  ← Composition root, HTTP
                    └──────────┬───────────┘
                               │
                 ┌─────────────┴─────────────┐
                 │       Application         │  ← Use cases, MediatR handlers
                 └─────────────┬─────────────┘
                               │
                 ┌─────────────┴─────────────┐
                 │     Infrastructure        │  ← Implementations (repositories)
                 └─────────────┬─────────────┘
                               │
                    ┌──────────┴───────────┐
                    │        Domain        │  ← Entities, business rules
                    └──────────────────────┘
```

### Dependency flow

```
Domain  ←──  Application  ←──  Infrastructure
   ↑              ↑                    ↑
   └──────────────┴──────────  Api (composition root)
```

- **Domain**: zero dependencies.
- **Application**: depends only on `Domain` and `MediatR`.
- **Infrastructure**: depends on `Application` (implements its interfaces) and `Domain`.
- **Api**: depends on `Application` and `Infrastructure` to wire everything together.

Because `Application` does **not** reference any web packages, the handlers can be unit-tested and reused outside of HTTP contexts.

---

## 📂 Project structure

```
WalletCQRS.sln
└── src/
    ├── WalletCQRS.Domain/                          # Business entities & rules
    │   └── Entities/
    │       └── Wallet.cs
    │
    ├── WalletCQRS.Application/                     # Use cases (CQRS handlers)
    │   ├── Common/
    │   │   ├── Interfaces/
    │   │   │   └── IWalletRepository.cs
    │   │   └── Results/
    │   │       └── WalletOperationResult.cs
    │   └── Features/
    │       ├── Wallets/
    │       │   └── CreateWalletCommand.cs          # Command (write)
    │       ├── Deposits/
    │       │   └── DepositCommand.cs               # Command (write)
    │       └── Queries/
    │           └── GetBalanceQuery.cs              # Query (read)
    │
    ├── WalletCQRS.Infrastructure/                  # Implementations
    │   └── Persistence/
    │       └── InMemoryWalletRepository.cs
    │
    └── WalletCQRS.Api/                             # Web / HTTP entry point
        ├── Program.cs
        └── Presentation/
            └── WalletEndpoints.cs
```

---

## ⚙️ How it works

### CQRS split

The core idea of CQRS is that **reads and writes are handled differently**:

| Command (writes) | Query (reads) |
|---|---|
| `CreateWalletCommand` | `GetBalanceQuery` |
| `DepositCommand` | — |

- **Commands** change state and are handled by `IRequestHandler<...>`.
- **Queries** only read state and are also handled by `IRequestHandler<...>`, but semantically represent reads.

### The mediator pattern

Instead of calling handlers directly, the `Api` layer sends requests through `ISender` (MediatR):

```csharp
var balance = await mediator.Send(new GetBalanceQuery(id));
```

MediatR automatically resolves the correct handler for each request type. Handlers are registered once in `Program.cs`:

```csharp
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateWalletCommand).Assembly));
```

### Decoupling from HTTP

Handlers do **not** return `IResult` or any ASP.NET type. They return plain data (or throw `ArgumentException` for validation failures):

| Handler | Returns |
|---|---|
| `CreateWalletCommandHandler` | `Guid` (the new wallet id) |
| `DepositCommandHandler` | `WalletOperationResult` |
| `GetBalanceQueryHandler` | `BalanceDto?` |

The **Api** layer translates these plain results into proper HTTP responses (`200`, `201`, `400`, `404`). This keeps the `Application` project free of web dependencies.

---

## 🚀 Getting started

### Prerequisites

- [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0) (or newer)

### Run the API

```bash
# from the repository root
dotnet run --project src/WalletCQRS.Api
```

By default the app runs in **Development** mode, where the interactive API reference (Scalar) is available.

### Open the API reference

With the app running, open your browser at:

```
http://localhost:5269/scalar/v1
```

> The HTTPS profile uses port `7251`; the HTTP profile uses `5269` (see `src/WalletCQRS.Api/Properties/launchSettings.json`).

## 📚 Key concepts learned

1. **CQRS** — separating write operations (commands) from read operations (queries).
2. **MediatR** — using the mediator pattern to dispatch requests to their handlers.
3. **Clean Architecture / multi-project** — enforcing layer boundaries via separate projects and project references.
4. **Dependency Inversion** — `Application` defines the repository **interface**; `Infrastructure` provides the **implementation**; `Api` wires them via dependency injection.
5. **Decoupling from the web** — handlers return plain results; the presentation layer maps them to HTTP status codes.
6. **Domain invariants** — the `Wallet` entity encapsulates its own business rules (e.g., no negative balances, deposits must be positive).
## 📄 License

This project is for **educational purposes**. Feel free to use, study, and modify it as a reference for your own learning.
