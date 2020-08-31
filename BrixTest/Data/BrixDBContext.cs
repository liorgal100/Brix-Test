using System;
using System.Collections.Generic;
using System.Text;
using BrixTest.Models;
using Microsoft.EntityFrameworkCore;

namespace BrixTest.Data
{
    public class BrixDBContext: DbContext
    {
        public BrixDBContext ( DbContextOptions<BrixDBContext> options ) : base( options ) { }

        public virtual DbSet<Bank> Banks { get; set; }
        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<AccountOwner> AccountOwners { get; set; }
        public virtual DbSet<BankTransaction> BankTransactions { get; set; }

        protected override void OnModelCreating ( ModelBuilder modelBuilder )
        {
            base.OnModelCreating( modelBuilder );

            modelBuilder.Entity<Account>().HasOne( "BrixTest.Models.Bank" , "Bank" ).WithMany().HasForeignKey( "BankID" );

            modelBuilder.Entity<AccountOwner>().HasKey( A => new {A.AcoountID, A.OwnerID } );
            modelBuilder.Entity<AccountOwner>().HasOne( "BrixTest.Models.Account" , "Account" ).WithMany().HasForeignKey( "AccountID" );
            modelBuilder.Entity<AccountOwner>().HasOne( "BrixTest.Models.User" , "User" ).WithMany().HasForeignKey( "OwnerID" );

            modelBuilder.Entity<BankTransaction>().HasOne( "BrixTest.Models.AccountOwner" , "AccountOwner" ).WithMany().HasForeignKey("AccountID" , "ExcutedByOwnerID");
            modelBuilder.Entity<BankTransaction>().Property( T => T.TimeExecuted ).HasDefaultValueSql( "getdate()" );
        }

     }

}
