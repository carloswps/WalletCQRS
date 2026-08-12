namespace WalletCQRS.Application.Common.Results;

public record WalletOperationResult(
    bool Success,
    decimal? NewBalance = null,
    string? Error = null,
    bool NotFound = false
);