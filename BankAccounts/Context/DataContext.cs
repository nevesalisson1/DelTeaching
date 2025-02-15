using BankAccounts.Model;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Context;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }

    public DbSet<BankAccount> BankAccounts { get; set; }
    public DbSet<Balance> Balances { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Balance>()
            .HasOne(b => b.BankAccount)
            .WithOne(a => a.Balance)
            .HasForeignKey<Balance>(b => b.BankAccountId);
    }
}