using BankAccounts.Context;
using BankAccounts.Migrations;
using BankAccounts.Model;
using BankAccounts.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly DataContext _context;

        public TransactionsController(DataContext context)
        {
            _context = context;
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
        public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactionsByAccount(int accountId)
        {
            var transactions = await _context.Transactions
                                             .Where(t => t.BankAccountId == accountId)
                                             .Include(t => t.BankAccount)  // Incluindo o banco associado
                                             .ToListAsync();

            if (!transactions.Any()) return NotFound();

            return transactions;
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
        public async Task<ActionResult<Transaction>> CreateTransaction(int accountId, TransactionCreateViewModel transactionViewModel)
        {
            try
            {
                var bankAccount = await _context.BankAccounts
                                                 .Include(b => b.Balance)
                                                 .FirstOrDefaultAsync(b => b.Id == accountId);

                if (bankAccount == null) return NotFound("Conta bancária não encontrada.");

                // Validação RF15: Verificando se a conta pode receber um crédito
                if (transactionViewModel.Type == "CREDIT" && !IsAccountAbleToReceiveCredit(accountId))
                {
                    return BadRequest("A conta não pode receber crédito.");
                }

                // Validação RF16: Verificando se a conta pode realizar um débito
                if (transactionViewModel.Type == "DEBIT" && !IsAccountAbleToMakeDebit(accountId, transactionViewModel.Amount))
                {
                    return BadRequest("A conta não pode realizar débito devido ao saldo insuficiente.");
                }

                // Mapeamento do ViewModel para o modelo de Transação
                var transaction = new Transaction
                {
                    Type = Enum.Parse<TransactionType>(transactionViewModel.Type),
                    Amount = transactionViewModel.Amount,
                    BankAccountId = accountId,
                    CounterpartyBankCode = transactionViewModel.CounterpartyBankCode,
                    CounterpartyBankName = transactionViewModel.CounterpartyBankName,
                    CounterpartyBranch = transactionViewModel.CounterpartyBranch,
                    CounterpartyAccountNumber = transactionViewModel.CounterpartyAccountNumber,
                    CounterpartyAccountType = Enum.Parse<AccountType>(transactionViewModel.CounterpartyAccountType),
                    CounterpartyAccountHolderName = transactionViewModel.CounterpartyAccountHolderName,
                    CounterpartyHolderType = Enum.Parse<HolderType>(transactionViewModel.CounterpartyHolderType),
                    CounterpartyHolderDocument = transactionViewModel.CounterpartyHolderDocument,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Atualizando o saldo da conta
                if (transaction.Type == TransactionType.DEBIT)
                {
                    bankAccount.Balance.AvailableAmount -= transaction.Amount;
                }
                else if (transaction.Type == TransactionType.CREDIT)
                {
                    bankAccount.Balance.AvailableAmount += transaction.Amount;
                }

                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();

                // Retorna a transação criada
                return CreatedAtAction(nameof(GetTransactionById), new { id = transaction.Id }, transaction);
            }
            catch (Exception)
            {
                return BadRequest("Erro ao criar transação.");
            }
        }
    }
}
