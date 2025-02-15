using System.Text;
using BankAccounts.Context;
using BankAccounts.Migrations;
using BankAccounts.ViewModel;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Transaction = BankAccounts.Model.Transaction;


public class TransactionWorker : BackgroundService
{
    private readonly ILogger<TransactionWorker> _logger;
    private readonly IConfiguration _config;
    private readonly IServiceProvider _serviceProvider;

    public TransactionWorker(ILogger<TransactionWorker> logger, IConfiguration config, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _config = config;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory() { HostName = _config["RabbitMQ:Host"] };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: _config["RabbitMQ:QueueName"],
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            _logger.LogInformation($"Recebido: {message}");

            try
            {
                var transactionRequest = JsonConvert.DeserializeObject<TransactionRequest>(message);
                await ProcessTransaction(transactionRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao processar transação: {ex.Message}");
            }
        };

        channel.BasicConsume(queue: _config["RabbitMQ:QueueName"], autoAck: true, consumer: consumer);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task ProcessTransaction(TransactionRequest request)
    {
        using var scope = _serviceProvider.CreateScope();
        var _context = scope.ServiceProvider.GetRequiredService<DataContext>();

        var bankAccount = await _context.BankAccounts
            .Include(b => b.Balance)
            .FirstOrDefaultAsync(b => b.Id == request.AccountId);

        if (bankAccount == null)
        {
            _logger.LogWarning($"Conta bancária {request.AccountId} não encontrada.");
            return;
        }

        var transaction = new Transaction
        {
            Type = Enum.Parse<TransactionType>(request.Transaction.Type),
            Amount = request.Transaction.Amount,
            BankAccountId = request.AccountId,
            CounterpartyBankCode = request.Transaction.CounterpartyBankCode,
            CounterpartyBankName = request.Transaction.CounterpartyBankName,
            CounterpartyBranch = request.Transaction.CounterpartyBranch,
            CounterpartyAccountNumber = request.Transaction.CounterpartyAccountNumber,
            CounterpartyAccountType = Enum.Parse<AccountType>(request.Transaction.CounterpartyAccountType),
            CounterpartyAccountHolderName = request.Transaction.CounterpartyAccountHolderName,
            CounterpartyHolderType = Enum.Parse<HolderType>(request.Transaction.CounterpartyHolderType),
            CounterpartyHolderDocument = request.Transaction.CounterpartyHolderDocument,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (transaction.Type == TransactionType.DEBIT)
        {
            if (bankAccount.Balance.AvailableAmount < transaction.Amount)
            {
                _logger.LogWarning($"Saldo insuficiente para débito na conta {request.AccountId}.");
                return;
            }

            bankAccount.Balance.AvailableAmount -= transaction.Amount;
        }
        else if (transaction.Type == TransactionType.CREDIT)
        {
            bankAccount.Balance.AvailableAmount += transaction.Amount;
        }

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Transação {transaction.Id} processada com sucesso.");
    }
}

public class TransactionRequest
{
    public int AccountId { get; set; }
    public TransactionCreateViewModel Transaction { get; set; }
}
