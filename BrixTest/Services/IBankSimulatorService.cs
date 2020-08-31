using BrixTest.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BrixTest.Services
{
    public interface IBankSimulatorService
    {
        public Task<Bank> CreateBank(string BankName);
        public Task<Account> CreateAccount ( int BankID );
        public Task<AccountOwner> AddAccountOwner (string OwnerID, string OwnerName, int AccountID );

        public BankTransaction Deposit ( int AccountID , string DepositByUserID , double Amount);
        public BankTransaction Withdrawal ( int AccountID , string DepositByUserID , double Amount );
    }

    public class BankSimulatorService: IBankSimulatorService
    {
        private readonly IDBServices _dbServices;
        public BankSimulatorService ( IDBServices dbServices)
        {
            _dbServices = dbServices;
        }

        public async Task<Bank> CreateBank ( string BankName )
        {
            Bank _newBank = await _dbServices.CreateBank( BankName );
            return _newBank;
        }
        public async Task<Account> CreateAccount (int BankID)
        {
            return await _dbServices.CreateAccount( BankID );
        }

        public async Task<AccountOwner>AddAccountOwner(string OwnerID , string OwnerName , int AccountID)
        {
            User _owner = await _dbServices.GetUser( OwnerID );
            if (_owner == null)
                _owner = await _dbServices.CreateUser( OwnerID , OwnerName );

            AccountOwner _accountOwner = await _dbServices.AddAccountOwner( OwnerID , AccountID );
            return _accountOwner;
        }

        public BankTransaction Deposit (int AccountID , string DepositByUserID , double Amount)
        {
            if (Amount <= 0)
                throw new Exception( "Amount must be a possitive number" );

            BankTransaction _depositTrans = new BankTransaction() {AccountID = AccountID, ExcutedByOwnerID = DepositByUserID, Amount = Amount };
            return _dbServices.AddTransaction( _depositTrans );
        }

        public BankTransaction Withdrawal( int AccountID , string DepositByUserID , double Amount )
        {
            if (Amount <= 0)
                throw new Exception( "Amount must be a possitive number" );

            Amount = Amount * -1;
            BankTransaction _depositTrans = new BankTransaction() { AccountID = AccountID , ExcutedByOwnerID = DepositByUserID , Amount = Amount };
            return _dbServices.AddTransaction( _depositTrans );
        }

    }
}
