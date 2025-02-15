using System.Text;
using BankAccounts.Context;
using BankAccounts.Migrations;
using BankAccounts.Model;
using BankAccounts.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace BankAccounts.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConfiguration _config;

        public TransactionsController(DataContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // RF15 - Validação antes de aceitar um crédito
        private bool IsAccountAbleToReceiveCredit(int id)
        {
            var bankAccount = _context.BankAccounts.FirstOrDefault(b => b.Id == id);
            return bankAccount != null && bankAccount.Status == AccountStatus.ACTIVE;
        }

        // RF16 - Validação antes de realizar um débito
        private bool IsAccountAbleToMakeDebit(int id, decimal amount)
        {
            var bankAccount = _context.BankAccounts
                .Include(b => b.Balance)
                .FirstOrDefault(b => b.Id == id);
            return bankAccount != null && bankAccount.Status == AccountStatus.ACTIVE &&
                   bankAccount.Balance.AvailableAmount >= amount;
        }

        // RF17 - Buscar uma transação pelo ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Transaction>> GetTransactionById(int id)
        {
            var transaction = await _context.Transactions
                                            .Include(t => t.BankAccount)  // Incluindo o banco associado
                                            .FirstOrDefaultAsync(t => t.Id == id);

            if (transaction == null) return NotFound();

            return transaction;
        }

        // RF18 - Buscar todas as transações de uma conta bancária
        [HttpGet("account/{accountId}")]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactionsByAccount(
            int accountId,
            [FromQuery] int? id = null,                  // Filtro opcional por ID da transação
            [FromQuery] DateTime? startDate = null,      // Filtro opcional por data de início
            [FromQuery] DateTime? endDate = null,        // Filtro opcional por data de término
            [FromQuery] string? counterpartyDocument = null, // Filtro opcional por documento da contraparte
            [FromQuery] string? transactionType = null     // Filtro opcional por tipo de transação
        )
        {
            try
            {
                // Inicia a consulta
                var query = _context.Transactions
                    .Where(t => t.BankAccountId == accountId);  // Filtra pela conta bancária

                if (id.HasValue)
                {
                    query = query.Where(t => t.Id == id);
                }

                // Filtro por data de início, se informado
                if (startDate.HasValue)
                {
                    query = query.Where(t => t.CreatedAt >= startDate.Value);
                }

                // Filtro por data de término, se informado
                if (endDate.HasValue)
                {
                    query = query.Where(t => t.CreatedAt <= endDate.Value);
                }

                // Filtro por documento da contraparte, se informado
                if (!string.IsNullOrEmpty(counterpartyDocument))
                {
                    query = query.Where(t => t.CounterpartyHolderDocument.Contains(counterpartyDocument));
                }

                // Filtro por tipo de transação, se informado
                if (!string.IsNullOrEmpty(transactionType))
                {
                    if (Enum.TryParse<TransactionType>(transactionType, true, out var parsedType))
                    {
                        query = query.Where(t => t.Type == parsedType);
                    }
                    else
                    {
                        return BadRequest("Tipo de transação inválido.");
                    }
                }

                // Executa a consulta sem incluir o BankAccount
                var transactions = await query.ToListAsync();

                if (!transactions.Any())
                {
                    return NotFound("Nenhuma transação encontrada para a conta bancária.");
                }

                return Ok(transactions);
            }
            catch (Exception)
            {
                return BadRequest("Erro ao listar as transações.");
            }
        }

        // RF19 - Buscar transações pelo documento do titular
        [HttpGet("holder/{holderDocument}")]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactionsByHolderDocument(string holderDocument)
        {
            var bankAccounts = await _context.BankAccounts
                                              .Where(b => b.HolderDocument == holderDocument)
                                              .ToListAsync();

            if (!bankAccounts.Any()) return NotFound("Nenhuma conta encontrada para o titular.");

            var transactions = await _context.Transactions
                                             .Where(t => bankAccounts.Select(b => b.Id).Contains(t.BankAccountId))
                                             .Include(t => t.BankAccount)  // Incluindo o banco associado
                                             .ToListAsync();

            if (!transactions.Any()) return NotFound("Nenhuma transação encontrada para o titular.");

            return transactions;
        }

        // RF20 - Criar uma nova transação
        [HttpPost("account/{accountId}")]
        public ActionResult CreateTransaction(int accountId, TransactionCreateViewModel transactionViewModel)
        {
            try
            {
                var rabbitMqHost = _config["RabbitMQ:Host"] ?? "localhost";  // Provide default
                var factory = new ConnectionFactory() { HostName = rabbitMqHost };
                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                channel.QueueDeclare(queue: _config["RabbitMQ:QueueName" ?? "transactions_queue"],
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                var transactionRequest = new
                {
                    AccountId = accountId,
                    Transaction = transactionViewModel
                };

                var message = JsonConvert.SerializeObject(transactionRequest);
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "",
                    routingKey: _config["RabbitMQ:QueueName"],
                    basicProperties: null,
                    body: body);

                return Ok();
            }
            catch (Exception)
            {
                return BadRequest("Erro ao enviar transação para processamento.");
            }
        }
    }
}
