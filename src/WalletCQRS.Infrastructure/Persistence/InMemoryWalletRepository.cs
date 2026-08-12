using System.Collections.Concurrent;
using WalletCQRS.Application.Common.Interfaces;
using WalletCQRS.Domain.Entities;

namespace WalletCQRS.Infrastructure.Persistence;

public class InMemoryWalletRepository : IWalletRepository
{
    private readonly ConcurrentDictionary<Guid, Wallet> _wallets = new();

    public Task<Wallet?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        _wallets.TryGetValue(id, out var wallet);
        return Task.FromResult(wallet);
    }

    public Task CreateAsync(Wallet wallet, CancellationToken cancellationToken)
    {
        _wallets[wallet.Id] = wallet;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Wallet wallet, CancellationToken cancellationToken)
    {
        _wallets[wallet.Id] = wallet;
        return Task.CompletedTask;
    }
}