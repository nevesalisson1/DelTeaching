﻿// <auto-generated />
using System;
using BankAccounts.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BankAccounts.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20250215130504_InitialMigration")]
    partial class InitialMigration
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("BankAccounts.Model.Balance", b =>
                {
                    b.Property<int>("BankAccountId")
                        .HasColumnType("integer");

                    b.Property<decimal>("AvailableAmount")
                        .HasColumnType("numeric");

                    b.Property<decimal>("BlockedAmount")
                        .HasColumnType("numeric");

                    b.HasKey("BankAccountId");

                    b.ToTable("Balances");
                });

            modelBuilder.Entity("BankAccounts.Model.BankAccount", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Branch")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("HolderDocument")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("HolderEmail")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("HolderName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("HolderType")
                        .HasColumnType("integer");

                    b.Property<string>("Number")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("BankAccounts");
                });

            modelBuilder.Entity("BankAccounts.Model.Balance", b =>
                {
                    b.HasOne("BankAccounts.Model.BankAccount", "BankAccount")
                        .WithOne("Balance")
                        .HasForeignKey("BankAccounts.Model.Balance", "BankAccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("BankAccount");
                });

            modelBuilder.Entity("BankAccounts.Model.BankAccount", b =>
                {
                    b.Navigation("Balance")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
