using BankAccounts.Migrations;

namespace BankAccounts.Model
{
    public class BankAccount
    {
        public int Id { get; set; }
        public string Branch { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public AccountType Type { get; set; }  // Enum para o tipo da conta

        public string HolderName { get; set; } = string.Empty;
        public string HolderEmail { get; set; } = string.Empty;
        public string HolderDocument { get; set; } = string.Empty;
        public HolderType HolderType { get; set; } // Enum para tipo de titular

        public AccountStatus Status { get; set; } // Enum para status da conta

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Relacionamento 1:1 com Balance
        public Balance Balance { get; set; } = null!;

        // Relacionamento 1:N com Transações
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}