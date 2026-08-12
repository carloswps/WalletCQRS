namespace WalletCQRS.Domain.Entities;

public class Wallet
{
    public Wallet(string owner, decimal initialBalance = 0)
    {
        Id = Guid.NewGuid();
        Owner = owner;
        if (initialBalance < 0)
            throw new ArgumentException("Initial balance cannot be negative", nameof(initialBalance));
        Balance = initialBalance;
    }

    public Guid Id { get; set; }
    public string Owner { get; private set; }
    public decimal Balance { get; private set; }

    public void Deposit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));
        Balance += amount;
    }
}