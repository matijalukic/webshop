using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebShop.Models;

namespace UpdateAuctionsJob
{
    class Program
    {
        // Ograniciti broj aukcija po obradi
        private static int MAX_AUCTIONS_PER = 100; // proizvoljan broj

        static void Main(string[] args)
        {
            using (Prodavnica DB = new Prodavnica())
            {
                using(var ContextTransaction = DB.Database.BeginTransaction())
                {
                    try
                    {
                        var UniversalNow = DateTime.UtcNow;
                        var OpenFinishedAuctions = DB.Auctions.Where(a => a.ClosedAt != null && a.ClosedAt <= UniversalNow && a.State == AuctionState.Opened).Take(MAX_AUCTIONS_PER);
                        
                        // Za svaku aukciju
                        foreach (Auction Auction in OpenFinishedAuctions)
                        {

                            // provera da li je aukcija za zatvaranje
                            if (UniversalNow > Auction.ClosedAt && Auction.State == AuctionState.Opened)
                            {
                                Console.WriteLine(Auction.Name + " is closing beacause it is opened and time has expired!");
                                // if there is bids
                                if (Auction.Bids.Any())
                                {
                                    // select the highest bid
                                    var HighestBid = Auction.Bids.OrderByDescending(b => b.Amount).First();

                                    // set the winner
                                    Auction.WinnerId = HighestBid.UserId;

                                    // select the other bids
                                    var OtherBidersId = Auction.Bids.Where(b => b.UserId != HighestBid.UserId).Select(b => b.UserId).Distinct();

                                    foreach (var OtherBiderId in OtherBidersId)
                                    {
                                        // find the highes bid of the bider
                                        var HighestBidOfBider = Auction.Bids.Where(b => b.UserId == OtherBiderId).OrderByDescending(b => b.Amount).First();
                                        // return tokens
                                        HighestBidOfBider.User.Tokens += HighestBidOfBider.Amount;

                                    }

                                }

                                // mark the auction as closed
                                Auction.State = AuctionState.Closed;
                            }
                        }

                        DB.SaveChanges();
                        ContextTransaction.Commit();
                        
                    }
                    catch (Exception)
                    {
                        ContextTransaction.Rollback();
                    }
                }
                
                
            }

             
        }
    }
}
