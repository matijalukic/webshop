using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using WebShop.Models;
using System.Data.Entity;

namespace WebShop.Hubs
{
    [HubName("AuctionHub")]
    public class AuctionHub : Hub
    {

        [HubMethodName("SendBid")]
        public void SendBid(int Amount, int AuctionId, int UserId)
        {
            Bid Bid = null;
            User User = null;

            using (Prodavnica db = new Prodavnica())
            {
                var AuctionOnBid = db.Auctions.Find(AuctionId);
                var AuctionsBids = db.Bids.Where(a => a.AuctionId == AuctionId);
                var LoggedUser = db.Users.Find(UserId);
                User = LoggedUser;

                if (LoggedUser == null)
                {
                    Clients.Caller.ReceiveError("User must be logged in!");
                    return;
                }


                // ako je veci od najveceg ponudjenog
                int? MaxBidAmount = AuctionsBids.Any() ? AuctionsBids.Max(a => a.Amount) : AuctionOnBid.StartPrice;
                if (Amount > MaxBidAmount.GetValueOrDefault())
                {

                    // Maximum old bid
                    var MyBids = db.Bids.Where(b => b.AuctionId == AuctionId && b.UserId == LoggedUser.Id);
                    int MaxOldBid = MyBids.Any() ? MyBids.Max(a => a.Amount) : 0;

                    int AmountToPay = Amount - MaxOldBid;

                    // Decreas users number of tokens
                    if (LoggedUser.Tokens < AmountToPay)
                    {
                        Clients.Caller.ReceiveError("Not enough tokens!");
                        return;
                    }

                    LoggedUser.Tokens -= Amount - MaxOldBid;
                    db.Entry(LoggedUser).State = EntityState.Modified;

                    Bid newBid = new Bid
                    {
                        AuctionId = AuctionId,
                        UserId = LoggedUser.Id,
                        Amount = Amount,
                        CreatedAt = DateTime.Now
                    };

                    db.Bids.Add(newBid);
                    db.SaveChanges();
                    // return to caller new balance
                    Clients.Caller.UpdateBalance(LoggedUser.Tokens);
                    // Return to all clients
                    Clients.All.ReceiveBid(Amount, User, newBid.CreatedAt.ToString());
                    Clients.All.NewPrice(Amount);
                    Clients.All.UpdatePrice(Amount, AuctionId);
                }
                else
                {
                    Clients.Caller.ReceiveError("New bid must be the highest!");
                }
            }



        }
    }
}