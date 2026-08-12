using MediatR;
using WalletCQRS.Application.Common.Interfaces;

namespace WalletCQRS.Application.Features.Queries;

public record BalanceDto(Guid WalletId, string Owner, decimal Balance);

public record GetBalanceQuery(Guid WalletId) : IRequest<BalanceDto?>;

public class GetBalanceQueryHandler(IWalletRepository walletRepository) : IRequestHandler<GetBalanceQuery, BalanceDto?>
{
    private readonly IWalletRepository _walletRepository = walletRepository;

    public async Task<BalanceDto?> Handle(GetBalanceQuery request, CancellationToken cancellationToken)
    {
        var wallet = await _walletRepository.GetByIdAsync(request.WalletId, cancellationToken);
        if (wallet is null)
            return null;

        return new BalanceDto(wallet.Id, wallet.Owner, wallet.Balance);
    }
}