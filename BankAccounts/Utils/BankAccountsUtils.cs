namespace BankAccounts.Utils;

public class BankAccountsUtils
{
    public static string GenerateAccountNumber()
    {
        var dateTimePart = DateTime.UtcNow.ToString("mmssfff");
        var randomPart = new Random().Next(1000, 9999).ToString();

        var accountNumber = dateTimePart + randomPart;

        return accountNumber;
    }
}