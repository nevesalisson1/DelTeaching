using BankAccounts.Migrations;

namespace BankAccounts.Model
{
    public class Transaction
    {
        public int Id { get; set; }
        public TransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public int BankAccountId { get; set; } // Chave estrangeira para BankAccount
        public string CounterpartyBankCode { get; set; } = string.Empty;
        public string CounterpartyBankName { get; set; } = string.Empty;
        public string CounterpartyBranch { get; set; } = string.Empty;
        public string CounterpartyAccountNumber { get; set; } = string.Empty;
        public AccountType CounterpartyAccountType { get; set; }
        public string CounterpartyAccountHolderName { get; set; } = string.Empty;
        public HolderType CounterpartyHolderType { get; set; }
        public string CounterpartyHolderDocument { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Relacionamento com BankAccount
        public BankAccount BankAccount { get; set; } = null!;
    }
}