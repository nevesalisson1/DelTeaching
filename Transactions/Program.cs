using BankAccounts.Context;
using Microsoft.EntityFrameworkCore;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        // Registre o DataContext no DI container
        services.AddDbContext<DataContext>(options =>
            options.UseNpgsql(hostContext.Configuration.GetConnectionString("DefaultConnection")));

        // Registre o TransactionWorker como um serviço hospedado
        services.AddHostedService<TransactionWorker>();
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole(); // Log no console
    })
    .Build();

await host.RunAsync();