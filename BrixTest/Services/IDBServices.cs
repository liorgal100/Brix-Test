using BrixTest.Data;
using BrixTest.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Threading;

namespace BrixTest.Services
{
    public interface IDBServices
    {
        public Task<Bank> CreateBank(string BankName);
        public Task<Account> CreateAccount (int BankID);
        public Task<User> CreateUser (string UserID, string UserName);
        public Task<User> GetUser (string UserID);
        public Task<AccountOwner> AddAccountOwner (string OwnerID , int AccountID);

        public BankTransaction AddTransaction ( BankTransaction _transaction );
    }

    public class DBServices : IDBServices
    {
        private readonly BrixDBContext _dbContext;
        private readonly IConfigurationRoot _configuration;
        private readonly ILogServices _log;
        private readonly object Locker = new object();
        public DBServices (BrixDBContext dbContext, IConfigurationRoot configuration, ILogServices log)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _log = log;
        }

        public async Task<Bank> CreateBank ( string BankName )
        {
            Bank _newBank = new Bank() { Name = BankName };
            _dbContext.Add( _newBank );
            await _dbContext.SaveChangesAsync();
            return _newBank;
        }

        public async Task<Account> CreateAccount ( int BankID )
        {
            Account _newAccount = new Account() { BankID = BankID };
            try
            {
                _dbContext.Accounts.Add( _newAccount );
                await _dbContext.SaveChangesAsync();
                return _newAccount;
            }
            catch (Exception Ex)
            {
                if (Ex.InnerException != null && Ex.InnerException is SqlException)
                    throw new Exception( "DB Error. please check " + BankID.ToString() + " is a valid bank id." );
                else
                    throw;
            }
        }

        public async Task<User> CreateUser ( string UserID , string UserName )
        {
            User _newUser = new User() { ID = UserID , Name = UserName };
            try
            {
                _dbContext.Users.Add( _newUser );
                await _dbContext.SaveChangesAsync();
                return _newUser;
            }
            catch (Exception Ex)
            {
                if (Ex.InnerException != null && Ex.InnerException is SqlException)
                    throw new Exception( "DB Error. please check " + UserID + " is a valid and new user id." );
                else
                    throw;
            }
        }
        public async Task<User> GetUser ( string UserID )
        {
            return await _dbContext.Users.Where( U => U.ID == UserID ).FirstOrDefaultAsync();
        }

        public async Task<AccountOwner> AddAccountOwner(string OwnerID , int AccountID )
        {
           List<AccountOwner> lstOwners = _dbContext.AccountOwners.Where( A => A.AcoountID == AccountID ).ToList();
           AccountOwner _existingOwner = lstOwners.Find( A => A.User.ID == OwnerID);
           int AccountOwnerLimit = Convert.ToInt32( _configuration[ "AccountOwnerLimit" ] );

            if (_existingOwner != null) //user is already the account owner
                return _existingOwner;
            else if (lstOwners.Count == AccountOwnerLimit) //enforce X owners for an account policy
                throw new Exception( "Account " + AccountID.ToString() + " have " + lstOwners.Count.ToString() + " owners and can't except another owner." );
            else //create new
            {
                try
                {
                    AccountOwner _AccountOwner = new AccountOwner() {OwnerID = OwnerID , AcoountID = AccountID};
                    _dbContext.AccountOwners.Add( _AccountOwner );
                    await _dbContext.SaveChangesAsync();
                    return _AccountOwner;
                }
                catch (Exception Ex)
                {
                    if (Ex.InnerException != null && Ex.InnerException is SqlException)
                        throw new Exception( "DB Error. please make sure the account and owner exist in our bank. ");
                    else
                        throw;
                }
            }
         }

        public BankTransaction AddTransaction(BankTransaction _transaction)
        {
            //check that operation is done by the account owner.
            AccountOwner _owner = _dbContext.AccountOwners.Where( O => O.AcoountID == _transaction.AccountID && O.OwnerID == _transaction.ExcutedByOwnerID).FirstOrDefault();
            if (_owner == null)
                throw new Exception("The account and user provided dont match. please make sure the user provided is the account owner.");

            bool _lockWasTaken = false;
            Monitor.Enter( Locker , ref _lockWasTaken);
            try
            {
                //Thread.Sleep( 10000 ); //unmark to test multiple deposit\Withdrawal at the same time.

                //check balance is enough for Withdrawal.
                Account _Account = _dbContext.Accounts.Where( A => A.ID == _transaction.AccountID ).FirstOrDefault();
                if (_transaction.Amount < 0 && _Account.Balance + _transaction.Amount < 0)
                    throw new Exception( "insufficient funds. your balance is $" + _Account.Balance.ToString() );

                //update balance and save transaction
                _dbContext.Accounts.Attach( _Account );
                _Account.Balance += _transaction.Amount;
                _dbContext.BankTransactions.Add( _transaction );
                _dbContext.SaveChanges();

                _log.WriteLine(string.Format( "Successfully {0} ${1} to account {2}. New balance is ${3}.", _transaction.Amount > 0? "Deposit": "Withdrawal" ,
                    _transaction.Amount.ToString(),_transaction.AccountID.ToString(), _Account.Balance.ToString() ) , ConsoleColor.Green );
                
                return _transaction;
            }
            catch(Exception ex)
            {
                _log.WriteLine( "" , ConsoleColor.Red);
                _log.WriteLine( string.Format( "Error {0} account {1}. Error details: {2}" , _transaction.Amount > 0 ? "Deposit to " : "Withdrawal from ", _transaction.AccountID.ToString()
                                              , ex.Message ) , ConsoleColor.Red );
                return null;
            }
            finally
            {
                if (_lockWasTaken) Monitor.Exit( Locker );
            }
        }
    }
    
}
