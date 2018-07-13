using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebShop.Models
{
    public partial class TokenOrder
    {
        [Key]        
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }
        
        public int Amount { get; set; }
        public decimal Price { get; set; }
        
        public string State { get; set; } 

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; }


        [DataType(DataType.DateTime)]
        public DateTime? CompletedAt { get; set; }


    }
}