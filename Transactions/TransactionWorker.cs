using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Transactions;
public class TransactionWorker : BackgroundService
{
    private readonly ILogger<TransactionWorker> _logger;
    private readonly IConfiguration _config;

    public TransactionWorker(ILogger<TransactionWorker> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory() { HostName = _config["RabbitMQ:Host"] };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: _config["RabbitMQ:QueueName"], durable: false, exclusive: false, autoDelete: false, arguments: null);
        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            _logger.LogInformation($"Recebido: {message}");
        };

        channel.BasicConsume(queue: _config["RabbitMQ:QueueName"], autoAck: true, consumer: consumer);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}
