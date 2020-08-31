using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BrixTest.Models
{
    public class AccountOwner
    {
        public int AcoountID { get; set; }


        [StringLength( 9 )]
        public string OwnerID { get; set; }


        //===================== DB relationship ====================================
        public Account Account { get; set; }
        public User User { get; set; }
    }
}
