using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace WebShop.Models
{
    public partial class Bid
    {
        [Key]
        public int Id { get; set; }
        public int Amount { get; set; }
        public DateTime CreatedAt { get; set; }

        public int AuctionId { get; set; }
        public virtual Auction Auction { get; set; }
        
        public int UserId { get; set; }
        public virtual User User { get; set; }
    }
}