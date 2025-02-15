using System.ComponentModel.DataAnnotations;

namespace BankAccounts.ViewModel;

public class BankAccountCreateViewModel
{
    [Required(ErrorMessage = "A agência é obrigatória.")]
    [StringLength(4, MinimumLength = 3, ErrorMessage = "A agência deve ter entre 3 e 4 caracteres.")]
    public string Branch { get; set; }

    [Required(ErrorMessage = "O tipo da conta é obrigatório.")]
    [StringLength(20, ErrorMessage = "O tipo da conta não pode exceder 20 caracteres.")]
    public string Type { get; set; }

    [Required(ErrorMessage = "O nome do titular é obrigatório.")]
    [StringLength(100, ErrorMessage = "O nome do titular não pode exceder 100 caracteres.")]
    public string HolderName { get; set; }

    [Required(ErrorMessage = "O e-mail do titular é obrigatório.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [StringLength(150, ErrorMessage = "O e-mail não pode exceder 150 caracteres.")]
    public string HolderEmail { get; set; }

    [Required(ErrorMessage = "O documento do titular é obrigatório.")]
    [StringLength(14, MinimumLength = 11, ErrorMessage = "O documento do titular deve ter entre 11 e 14 caracteres.")]
    public string HolderDocument { get; set; }

    [Required(ErrorMessage = "O tipo legal do titular é obrigatório.")]
    [StringLength(20, ErrorMessage = "O tipo legal do titular não pode exceder 20 caracteres.")]
    public string HolderType { get; set; }
}