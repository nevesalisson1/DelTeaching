using System.ComponentModel.DataAnnotations;

namespace BankAccounts.ViewModel;

public class UpdateBankAccountStatusViewModel
{
    [Required(ErrorMessage = "É necessário informar o status.")]
    public string Status { get; set; }
}