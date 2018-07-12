namespace WebShop.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using System.Linq;
    using System.Web.Mvc;

    public static class AuctionState
    {
        public static string Ready { get { return "Ready"; } }
        public static string Opened { get { return "Opened"; } }
        public static string Closed { get { return "Closed"; } }
    }

    [Table("Auction")]
    public partial class Auction
    {

        public static IEnumerable<SelectListItem> States
        {
            get
            {
                List<SelectListItem> values = new List<SelectListItem>();
                values.Add(new SelectListItem() { Text = "Ready", Value = "Ready" });
                values.Add(new SelectListItem() { Text = "Opened", Value = "Opened" });
                values.Add(new SelectListItem() { Text = "Closed", Value = "Closed" });

                return values;
            }
        }
        // condition if Auction is opened
        public bool IsOpen()
        {
            return State == AuctionState.Opened && DateTime.Now <= ClosedAt; 
        }

        public bool IsClosed()
        {
            return State == AuctionState.Closed && DateTime.Now > ClosedAt;
        }

        // marks the auction as closed and returns tokens to all users
        public void ProclaimWinner()
        {
            if (DateTime.Now > ClosedAt && State == AuctionState.Opened)
            {
                // if there is bids
                if (Bids.Any())
                {
                    // select the highest bid
                    var HighestBid = Bids.OrderByDescending(b => b.Amount).First();

                    // select the other bids
                    var OtherBids = Bids.Where(b => b.UserId != HighestBid.UserId).OrderByDescending(b => b.Amount).GroupBy(b => b.UserId).ToList();

                    // return tokens to each user
                    foreach (Bid Bid in OtherBids)
                    {
                        Bid.User.Tokens += Bid.Amount;
                    }

                }

                // mark the auction as closed
                State = AuctionState.Closed;
            }
        }


        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Column(TypeName = "text")]
        public string Picture { get; set; }

        [Required]
        public int Duration { get; set; }

        [Required]
        [Display(Name="Starting price")]
        public int StartPrice { get; set; }

        [Required]
        public int Price { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name="Created At")]
        public DateTime CreatedAt { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name="Closed At")]
        public DateTime? ClosedAt { get; set; }

        [StringLength(10)]
        public string State { get; set; }
        
        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User User { get; set; }

        [ForeignKey("Winner")]
        public int? WinnerId { get; set; }
        public virtual User Winner { get; set; }

        public virtual ICollection<Bid> Bids { get; set; }

        public int CurrentPrice()
        {
            int MaxBidAmount = Bids.Any() ? Bids.Max(b => b.Amount) : this.StartPrice;
            return MaxBidAmount;
        }

    }

}
