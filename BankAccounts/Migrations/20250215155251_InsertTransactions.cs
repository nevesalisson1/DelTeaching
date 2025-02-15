using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BankAccounts.Migrations
{
    /// <inheritdoc />
    public partial class InsertTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    BankAccountId = table.Column<int>(type: "integer", nullable: false),
                    CounterpartyBankCode = table.Column<string>(type: "text", nullable: false),
                    CounterpartyBankName = table.Column<string>(type: "text", nullable: false),
                    CounterpartyBranch = table.Column<string>(type: "text", nullable: false),
                    CounterpartyAccountNumber = table.Column<string>(type: "text", nullable: false),
                    CounterpartyAccountType = table.Column<int>(type: "integer", nullable: false),
                    CounterpartyAccountHolderName = table.Column<string>(type: "text", nullable: false),
                    CounterpartyHolderType = table.Column<int>(type: "integer", nullable: false),
                    CounterpartyHolderDocument = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_BankAccounts_BankAccountId",
                        column: x => x.BankAccountId,
                        principalTable: "BankAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_BankAccountId",
                table: "Transactions",
                column: "BankAccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transactions");
        }
    }
}
