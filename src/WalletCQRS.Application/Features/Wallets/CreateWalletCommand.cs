using MediatR;
using WalletCQRS.Application.Common.Interfaces;
using WalletCQRS.Domain.Entities;

namespace WalletCQRS.Application.Features.Wallets;

public record CreateWalletCommand(string Owner, decimal InitialBalance = 0) : IRequest<Guid>;

public class CreateWalletCommandHandler(IWalletRepository walletRepository) : IRequestHandler<CreateWalletCommand, Guid>
{
    public async Task<Guid> Handle(CreateWalletCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Owner))
            throw new ArgumentException("Owner name is required");

        var wallet = new Wallet(request.Owner, request.InitialBalance);
        await walletRepository.CreateAsync(wallet, cancellationToken);
        return wallet.Id;
    }
}