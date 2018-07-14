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
                            // mark the auction as closed
                            Auction.State = AuctionState.Closed;
                            DB.Entry(Auction).State = System.Data.Entity.EntityState.Modified;
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
