using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BrixTest.Models
{
    public class User
    {
        [StringLength( 9 )]
        [Key]
        public string ID { get; set; }

        [StringLength( 50 )]
        public string Name { get; set; }

    }
}
