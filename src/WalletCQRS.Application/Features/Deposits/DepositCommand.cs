using MediatR;
using WalletCQRS.Application.Common.Interfaces;
using WalletCQRS.Application.Common.Results;

namespace WalletCQRS.Application.Features.Deposits;

public record DepositCommand(Guid WalletId, decimal Amount) : IRequest<WalletOperationResult>;

public class DepositCommandHandler(IWalletRepository walletRepository)
    : IRequestHandler<DepositCommand, WalletOperationResult>
{
    private readonly IWalletRepository _walletRepository = walletRepository;

    public async Task<WalletOperationResult> Handle(DepositCommand request, CancellationToken cancellationToken)
    {
        var wallet = await _walletRepository.GetByIdAsync(request.WalletId, cancellationToken);
        if (wallet is null)
            return new WalletOperationResult(false, NotFound: true);

        try
        {
            wallet.Deposit(request.Amount);
            await _walletRepository.UpdateAsync(wallet, cancellationToken);
            return new WalletOperationResult(true, wallet.Balance);
        }
        catch (ArgumentException e)
        {
            return new WalletOperationResult(false, Error: e.Message);
        }
    }
}