using BankAccounts.Context;
using BankAccounts.Migrations;
using BankAccounts.Model;
using BankAccounts.Utils;
using BankAccounts.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BankAccountsController : ControllerBase
    {
        private readonly DataContext _context;

        public BankAccountsController(DataContext context)
        {
            _context = context;
        }
        
        // RFBONUS - Listar todas as contas bancárias
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BankAccount>>> GetAllBankAccounts(
            [FromQuery] string? accountNumber = null,
            [FromQuery] string? branch = null,
            [FromQuery] string? holderDocument = null 
        )
        {
            try
            {
                var query = _context.BankAccounts.AsQueryable();

                // Filtro por número da conta
                if (!string.IsNullOrEmpty(accountNumber))
                {
                    query = query.Where(b => b.Number.Contains(accountNumber));
                }

                // Filtro por agência
                if (!string.IsNullOrEmpty(branch))
                {
                    query = query.Where(b => b.Branch.Contains(branch));
                }

                // Filtro por documento do titular
                if (!string.IsNullOrEmpty(holderDocument))
                {
                    query = query.Where(b => b.HolderDocument.Contains(holderDocument));
                }

                var bankAccounts = await query.Include(b => b.Balance).ToListAsync();

                if (bankAccounts == null || !bankAccounts.Any())
                {
                    return NotFound("Nenhuma conta bancária encontrada.");
                }

                return Ok(bankAccounts);
            }
            catch (Exception)
            {
                return BadRequest("Erro ao listar as contas bancárias.");
            }
        }

        // RF01 - Criar uma conta bancária
        [HttpPost]
        public async Task<ActionResult<BankAccount>> CreateBankAccount(BankAccountCreateViewModel bankAccountViewModel)
        {
            try
            {
                if (bankAccountViewModel == null) return BadRequest();

                // Mapeamento do ViewModel para o modelo real
                var bankAccount = new BankAccount
                {
                    Branch = bankAccountViewModel.Branch,
                    Number = BankAccountsUtils.GenerateAccountNumber(), // Método para gerar número de conta
                    Type = Enum.Parse<AccountType>(bankAccountViewModel.Type),
                    HolderName = bankAccountViewModel.HolderName,
                    HolderEmail = bankAccountViewModel.HolderEmail,
                    HolderDocument = bankAccountViewModel.HolderDocument,
                    HolderType = Enum.Parse<HolderType>(bankAccountViewModel.HolderType),
                    Status = AccountStatus.ACTIVE, // Status padrão (ATIVO)
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Adiciona a conta bancária ao contexto
                _context.BankAccounts.Add(bankAccount);
                await _context.SaveChangesAsync();

                // Criando um saldo inicial para a conta bancária (valor padrão)
                var bankAccountBalance = new Balance
                {
                    BankAccountId = bankAccount.Id,
                    AvailableAmount = 0, // Saldo inicial padrão, pode ser alterado conforme necessário
                    BlockedAmount = 0
                };

                // Adicionando o saldo no contexto
                _context.Balances.Add(bankAccountBalance);
                await _context.SaveChangesAsync();

                // Retorna a conta bancária criada
                return CreatedAtAction(nameof(GetBankAccountByNumber), new { number = bankAccount.Number }, bankAccount);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        // RF02 - Criar conta bancária com dados obrigatórios
        // Dados obrigatórios já são parte do CreateBankAccount

        // RF03 - Gerar ID e número de conta internamente (feito automaticamente pelo EF)

        // RF04 - Buscar uma conta bancária pelo número
        [HttpGet("{number}")]
        public async Task<ActionResult<BankAccount>> GetBankAccountByNumber(string number)
        {
            var bankAccount = await _context.BankAccounts
                                             .Include(b => b.Balance)
                                             .FirstOrDefaultAsync(b => b.Number == number);

            if (bankAccount == null) return NotFound();

            return bankAccount;
        }

        // RF05 - Buscar todas as contas bancárias de uma agência
        [HttpGet("agency/{branch}")]
        public async Task<ActionResult<IEnumerable<BankAccount>>> GetBankAccountsByBranch(string branch)
        {
            var bankAccounts = await _context.BankAccounts
                                              .Where(b => b.Branch == branch)
                                              .Include(b => b.Balance)
                                              .ToListAsync();

            return bankAccounts;
        }

        // RF06 - Buscar todas as contas bancárias de um titular
        [HttpGet("holder/{holderDocument}")]
        public async Task<ActionResult<IEnumerable<BankAccount>>> GetBankAccountsByHolder(string holderDocument)
        {
            var bankAccounts = await _context.BankAccounts
                                              .Where(b => b.HolderDocument == holderDocument)
                                              .Include(b => b.Balance)
                                              .ToListAsync();

            return bankAccounts;
        }

        // RF07 - Editar o e-mail do titular de uma conta
        [HttpPut("{id}/update-email")]
        public async Task<IActionResult> UpdateEmail(int id, string newEmail)
        {
            var bankAccount = await _context.BankAccounts.FindAsync(id);

            if (bankAccount == null) return NotFound();

            bankAccount.HolderEmail = newEmail;

            _context.Entry(bankAccount).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // RF08 - Atualizar o status de uma conta bancária
        [HttpPut("{id}/update-status")]
        public async Task<IActionResult> UpdateStatus(int id, UpdateBankAccountStatusViewModel statusViewModel)
        {
            try
            {
                var bankAccount = await _context.BankAccounts.FindAsync(id);

                if (bankAccount == null) return NotFound();

                bankAccount.Status = Enum.Parse<AccountStatus>(statusViewModel.Status);

                _context.Entry(bankAccount).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        // RF09 - Encerrar uma conta bancária
        [HttpDelete("{id}/close")]
        public async Task<IActionResult> CloseAccount(int id)
        {
            var bankAccount = await _context.BankAccounts.FindAsync(id);

            if (bankAccount == null) return NotFound();

            bankAccount.Status = AccountStatus.FINISHED; // Alterando o status para inativo

            _context.Entry(bankAccount).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // RF10 - Consultar saldo de uma conta bancária
        [HttpGet("{id}/balance")]
        public async Task<ActionResult<Balance>> GetBalance(int id)
        {
            var bankAccount = await _context.BankAccounts
                                             .Include(b => b.Balance)
                                             .FirstOrDefaultAsync(b => b.Id == id);

            if (bankAccount == null) return NotFound();

            return bankAccount.Balance;
        }

        // RF11 - Bloquear uma quantia do saldo de uma conta bancária
        [HttpPut("{id}/block-amount")]
        public async Task<IActionResult> BlockAmount(int id, decimal amount)
        {
            var bankAccount = await _context.BankAccounts
                                             .Include(b => b.Balance)
                                             .FirstOrDefaultAsync(b => b.Id == id);

            if (bankAccount == null) return NotFound();

            if (bankAccount.Balance.AvailableAmount < amount) return BadRequest("Saldo insuficiente");

            bankAccount.Balance.BlockedAmount += amount;
            bankAccount.Balance.AvailableAmount -= amount;

            _context.Entry(bankAccount).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // RF12 - Desbloquear uma quantia do saldo de uma conta bancária
        [HttpPut("{id}/unblock-amount")]
        public async Task<IActionResult> UnblockAmount(int id, decimal amount)
        {
            var bankAccount = await _context.BankAccounts
                                             .Include(b => b.Balance)
                                             .FirstOrDefaultAsync(b => b.Id == id);

            if (bankAccount == null) return NotFound();

            if (bankAccount.Balance.BlockedAmount < amount) return BadRequest("Quantia a desbloquear excede o bloqueio atual");

            bankAccount.Balance.BlockedAmount -= amount;
            bankAccount.Balance.AvailableAmount += amount;

            _context.Entry(bankAccount).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // RF13 - Recebimento de crédito
        [HttpPost("{id}/credit")]
        public async Task<IActionResult> CreditAccount(int id, decimal amount)
        {
            var bankAccount = await _context.BankAccounts
                                             .Include(b => b.Balance)
                                             .FirstOrDefaultAsync(b => b.Id == id);

            if (bankAccount == null) return NotFound();

            if (bankAccount.Status != AccountStatus.ACTIVE) return BadRequest("Conta não está ativa");

            bankAccount.Balance.AvailableAmount += amount;

            _context.Entry(bankAccount).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // RF14 - Realização de débito
        [HttpPost("{id}/debit")]
        public async Task<IActionResult> DebitAccount(int id, decimal amount)
        {
            var bankAccount = await _context.BankAccounts
                                             .Include(b => b.Balance)
                                             .FirstOrDefaultAsync(b => b.Id == id);

            if (bankAccount == null) return NotFound();

            if (bankAccount.Balance.AvailableAmount < amount) return BadRequest("Saldo insuficiente");

            bankAccount.Balance.AvailableAmount -= amount;

            _context.Entry(bankAccount).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
