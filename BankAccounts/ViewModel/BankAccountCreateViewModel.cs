using System.ComponentModel.DataAnnotations;
using BankAccounts.Migrations;

namespace BankAccounts.ViewModel;

public class BankAccountCreateViewModel
{
    [Required(ErrorMessage = "A agência é obrigatória.")]
    [StringLength(5, ErrorMessage = "A agência deve ter até 5 caracteres.")]
    public string Branch { get; set; }

    [Required(ErrorMessage = "O tipo da conta é obrigatório.")]
    [EnumDataType(typeof(AccountType), ErrorMessage = "O tipo da conta deve ser PAYMENT ou CURRENT.")]
    public string Type { get; set; }

    [Required(ErrorMessage = "O nome do titular é obrigatório.")]
    [StringLength(200, ErrorMessage = "O nome do titular não pode exceder 200 caracteres.")]
    public string HolderName { get; set; }

    [Required(ErrorMessage = "O e-mail do titular é obrigatório.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [StringLength(200, ErrorMessage = "O e-mail não pode exceder 200 caracteres.")]
    public string HolderEmail { get; set; }

    [Required(ErrorMessage = "O documento do titular é obrigatório.")]
    [StringLength(14, MinimumLength = 11, ErrorMessage = "O documento do titular deve ter entre 11 e 14 caracteres.")]
    public string HolderDocument { get; set; }

    [Required(ErrorMessage = "O tipo legal do titular é obrigatório.")]
    [EnumDataType(typeof(HolderType), ErrorMessage = "O tipo legal do titular deve ser NATURAL ou LEGAL.")]
    public string HolderType { get; set; }
}