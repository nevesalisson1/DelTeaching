using BankAccounts.Model;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Context
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<Balance> Balances { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Balance>()
                .HasOne(b => b.BankAccount)
                .WithOne(a => a.Balance)
                .HasForeignKey<Balance>(b => b.BankAccountId);

            // Relacionamento 1:N entre BankAccount e Transactions
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.BankAccount)
                .WithMany(b => b.Transactions)
                .HasForeignKey(t => t.BankAccountId)
                .OnDelete(DeleteBehavior.Cascade); // Configuração para deletar transações quando a conta for excluída
        }
    }
}