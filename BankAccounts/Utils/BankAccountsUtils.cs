namespace BankAccounts.Utils;

public class BankAccountsUtils
{
    public static string GenerateAccountNumber()
    {
        // Gerar um número de conta aleatório com 6 dígitos
        var rand = new Random();
        var accountNumber = rand.Next(100000, 999999);

        return accountNumber.ToString();
    }
}