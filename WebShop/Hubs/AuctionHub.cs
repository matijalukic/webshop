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
                
                // provera da li je aukcija istekla
                if (DateTime.UtcNow > AuctionOnBid.ClosedAt)
                {
                    Clients.Caller.ReceiveError("The auction has expired!");
                    return;
                }

               

                Bid newBid;
                using (var transactionContext = db.Database.BeginTransaction())
                {
                    // Maximum old bid
                    var MyBids = db.Bids.Where(b => b.AuctionId == AuctionId && b.UserId == LoggedUser.Id);
                    int MaxOldBid = MyBids.Any() ? MyBids.Max(a => a.Amount) : 0;

                    int AmountToPay = Amount - MaxOldBid;

                    // ako je veci od najveceg ponudjenog
                    int? MaxBidAmount = AuctionsBids.Any() ? AuctionsBids.Max(a => a.Amount) : AuctionOnBid.StartPrice;

                    // Decrease users number of tokens
                    if (db.Users.Find(LoggedUser.Id).Tokens < AmountToPay)
                    {
                        Clients.Caller.ReceiveError("Not enough tokens!");
                    }
                    // provera da li je najveca
                    else if (Amount <= MaxBidAmount.GetValueOrDefault())
                    {
                        Clients.Caller.ReceiveError("New bid must be the highest!");
                    }
                    else
                    {
                        try
                        {
                            // vrati rezervisani novac
                            var OldHighestBId = AuctionsBids.OrderByDescending(a => a.Amount).FirstOrDefault();
                            if (OldHighestBId != null)
                            {
                                OldHighestBId.User.Tokens += OldHighestBId.Amount;
                                db.Entry(OldHighestBId.User).State = EntityState.Modified;
                            }
                            // rezervisi stari novac
                            LoggedUser.Tokens -= Amount;
                            db.Entry(LoggedUser).State = EntityState.Modified;

                            newBid = new Bid
                            {
                                AuctionId = AuctionId,
                                UserId = LoggedUser.Id,
                                Amount = Amount,
                                CreatedAt = DateTime.UtcNow
                            };

                            db.Bids.Add(newBid);
                            db.SaveChanges();
                            transactionContext.Commit();

                            // return to caller new balance
                            Clients.Caller.UpdateBalance(LoggedUser.Tokens);
                            // Return to all clients
                            Clients.All.ReceiveBid(Amount, User, newBid.CreatedAt.ToString());
                            Clients.All.NewPrice(Amount);
                            Clients.All.UpdatePrice(Amount, AuctionId);
                        }
                        catch (Exception)
                        {
                            transactionContext.Rollback();
                        }
                    }
                    
                }
                
            }



        }
    }
}