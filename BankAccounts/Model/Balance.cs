using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankAccounts.Model;

public class Balance
{
    [Key, ForeignKey("BankAccount")]
    public int BankAccountId { get; set; }

    public decimal AvailableAmount { get; set; }
    public decimal BlockedAmount { get; set; }

    // Relacionamento 1:1 com BankAccount
    public BankAccount BankAccount { get; set; } = null!;
}