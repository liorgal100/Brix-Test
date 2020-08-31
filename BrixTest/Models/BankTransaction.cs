using System;
using System.Collections.Generic;
using System.Text;

namespace BrixTest.Models
{
    public class BankTransaction
    {
        public int ID { get; set; }

        public int AccountID { get; set; }
        public string ExcutedByOwnerID { get; set; }
        public double Amount { get; set; }
        public DateTime TimeExecuted { get; set; }

        //===================== DB relationship ====================================
        public AccountOwner AccountOwner { get; set; }
    }
}
