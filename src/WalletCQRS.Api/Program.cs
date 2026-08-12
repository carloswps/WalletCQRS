using Scalar.AspNetCore;
using WalletCQRS.Api.Presentation;
using WalletCQRS.Application.Common.Interfaces;
using WalletCQRS.Application.Features.Wallets;
using WalletCQRS.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateWalletCommand).Assembly));
builder.Services.AddOpenApi();
builder.Services.AddSingleton<IWalletRepository, InMemoryWalletRepository>();

//Health
builder.Services.AddHealthChecks();

//Redirect
builder.Services.AddHttpsRedirection(options =>
    {
        options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
        options.HttpsPort = 7251;
    }
);


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseRouting();
app.MapHealthChecks("/health");
app.MapWalletEndpoints();
app.Run();