# AGENTS.md

Study project: CQRS + Clean Architecture with MediatR in ASP.NET Core. Keep guidance here — don't commit unfinished/mismatched changes.

## Build & run

```bash
# run the web app (Api is the startup project)
dotnet run --project src/WalletCQRS.Api
# build the whole solution
dotnet build WalletCQRS.sln
```

No test project exists yet. There is no database — the repository is in-memory (`InMemoryWalletRepository`), so **all data is lost on restart**.

## Architecture & dependency rule

Multi-project solution under `src/`. Dependencies point inward:

- `WalletCQRS.Domain` — entities + invariants. Zero dependencies.
- `WalletCQRS.Application` — MediatR commands/queries + `IWalletRepository` interface + result DTOs. Depends only on Domain + MediatR.
- `WalletCQRS.Infrastructure` — implements `IWalletRepository` (`InMemoryWalletRepository`). Depends on Application.
- `WalletCQRS.Api` — Program.cs (composition root) + `Presentation/WalletEndpoints.cs`. Depends on Application + Infrastructure.

**Hard rule: `Application` must NOT reference ASP.NET/web types.** Handlers return plain records (`Guid`, `WalletOperationResult`, `BalanceDto?`), never `IResult`. The `Api` layer maps those to HTTP status codes in `WalletEndpoints.cs`.

## MediatR registration (easy to get wrong)

In `Program.cs`, handlers live in the **Application** project, not Api. Register from the Application assembly:

```csharp
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateWalletCommand).Assembly));
```

Do NOT use `typeof(Program).Assembly` — it points at Api and won't find handlers.

## Port mismatch gotcha

`Program.cs` hardcodes `options.HttpsPort = 7251`, but `launchSettings.json` binds HTTPS to **7231** and HTTP to **5032**. `README.md` also lists outdated ports (5269/7251). If you touch URL/redirection behavior, reconcile all three, or the redirect target won't match the actual listening port.

## Gotchas

- New CQRS handlers/requests use **primary constructors**; match the existing style.
- `GetBalanceQuery`/`DepositCommand`/`CreateWalletCommand` + `IWalletRepository` + `IWalletRepository` implementations must stay consistent when adding new members (e.g. repository methods must exist on interface, Application impl, and Infrastructure impl).
- `Wallet` entity: `Deposit` validates `amount <= 0` and constructor rejects negative `initialBalance`. Keep these invariants in the entity, not the handlers.
- When editing endpoints, the `{id:guid}` route constraint means `id` must be a valid GUID or the route 404s.

## Current repo state (git)

There is **staged, uncommitted migration work**: the old single-project tree (`WalletCQRS/` and `WalletCQRS.Api/` etc. at repo root) plus the new `src/` layout are both staged, with deletions and additions mixed. If you touch git, do not commit a half-migrated tree — reconcile to the `src/`-only layout first (old root-level project folders were removed from the solution and should not be restored).
