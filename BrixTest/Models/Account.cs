using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BrixTest.Models
{
    public class Account
    {
        public int ID { get; set; }

        public int BankID { get; set; }

        public double Balance { get; set; } = 0;

        //===================== DB relationship ====================================
        public Bank Bank { get; set; }
    }
}
