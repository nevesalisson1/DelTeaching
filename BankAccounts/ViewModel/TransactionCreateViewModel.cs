namespace BankAccounts.ViewModel;

public class TransactionCreateViewModel
{
    public string Type { get; set; } = string.Empty; // "DEBIT" ou "CREDIT"
    public decimal Amount { get; set; }
    public string CounterpartyBankCode { get; set; } = string.Empty;
    public string CounterpartyBankName { get; set; } = string.Empty;
    public string CounterpartyBranch { get; set; } = string.Empty;
    public string CounterpartyAccountNumber { get; set; } = string.Empty;
    public string CounterpartyAccountType { get; set; } = string.Empty; // "SAVINGS" ou "CHECKING"
    public string CounterpartyAccountHolderName { get; set; } = string.Empty;
    public string CounterpartyHolderType { get; set; } = string.Empty; // "INDIVIDUAL" ou "CORPORATE"
    public string CounterpartyHolderDocument { get; set; } = string.Empty;
}