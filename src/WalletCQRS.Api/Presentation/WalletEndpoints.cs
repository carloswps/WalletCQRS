using MediatR;
using WalletCQRS.Application.Features.Deposits;
using WalletCQRS.Application.Features.Queries;
using WalletCQRS.Application.Features.Wallets;

namespace WalletCQRS.Api.Presentation;

public static class WalletEndpoints
{
    public static void MapWalletEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/wallets").WithTags("Wallets");

        group.MapPost("/", async (CreateWalletRequest request, ISender mediator) =>
        {
            var command = new CreateWalletCommand(request.Owner, request.InitialBalance);
            var id = await mediator.Send(command);
            return Results.Created($"/wallets/{id}",
                new { id, owner = request.Owner, balance = request.InitialBalance });
        }).WithName("CreateWallet").WithSummary("Creates a new wallet");

        group.MapPost("/{id:guid}/deposit", async (Guid id, DepositRequest request, ISender mediator) =>
        {
            var result = await mediator.Send(new DepositCommand(id, request.Amount));
            return result switch
            {
                { NotFound: true } => Results.NotFound(new { message = "Wallet not found" }),
                { Success: false } => Results.BadRequest(new { message = result.Error }),
                _ => Results.Ok(new { message = "Deposit successful", newBalance = result.NewBalance })
            };
        });

        group.MapGet("/{id:guid}/balance", async (Guid id, ISender mediator) =>
        {
            var balance = await mediator.Send(new GetBalanceQuery(id));
            return balance is null
                ? Results.NotFound(new { message = "Wallet not found" })
                : Results.Ok(balance);
        });
    }
}

public record CreateWalletRequest(string Owner, decimal InitialBalance = 0);

public record DepositRequest(decimal Amount);