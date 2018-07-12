using System;
using System.Collections.Generic;
using System.Linq;
using Binbin.Linq;
using System.Web;
using System.Web.Mvc;
using WebShop.Models;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;

namespace WebShop.Controllers
{
    public class HomeController : Controller
    {
        private Prodavnica DB = new Prodavnica();

        private void FinsishAuctions()
        {
            var UniversalNow = DateTime.Now;
            var OpenFinishedAuctions = DB.Auctions.Where(a => a.ClosedAt != null && a.ClosedAt <= UniversalNow && a.State == AuctionState.Opened);

            foreach(Auction Auction in OpenFinishedAuctions)
            {
                if (DateTime.Now > Auction.ClosedAt && Auction.State == AuctionState.Opened)
                {
                    // if there is bids
                    if (Auction.Bids.Any())
                    {
                        // select the highest bid
                        var HighestBid = Auction.Bids.OrderByDescending(b => b.Amount).First();

                        // set the winner
                        Auction.WinnerId = HighestBid.UserId;

                        // select the other bids
                        var OtherBidersId = Auction.Bids.Where(b => b.UserId != HighestBid.UserId).Select(b => b.UserId).Distinct();

                        foreach(var OtherBiderId in OtherBidersId)
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

            DB.SaveChangesAsync();

        }

        /**
         * GET / Home Pretraga
         */
        public ActionResult Index(string name = null, int? minprice = null, int? maxprice = null, string status = null)
        {
            
            // Run background job
            //new Task(() => { FinsishAuctions(); }) .Start();

            var AuctionList = DB.Auctions.Include("User").Select(t => t);
            
            // search by name if it contains
            if (name != null)
            {
                // predicate for names
                var Predicate = PredicateBuilder.Create<Auction>(t => t.Name.Contains(name));
                string[] keywords = name.Split(' ');

                // checking each word if name contains each word
                foreach (string keyword in keywords)
                    Predicate = Predicate.Or(auction => auction.Name.Contains(keyword));

                AuctionList = AuctionList.Where(
                    Predicate
                );
            }


            // filter by price
            if (minprice != null)
                AuctionList = AuctionList.Where(auction => auction.Price >= minprice);
            if (maxprice != null)
                AuctionList = AuctionList.Where(auction => auction.Price <= maxprice);

            // filter by status
            if (status != null && String.Empty != status)
                AuctionList = AuctionList.Where(auction => auction.State == status);

            AuctionList = AuctionList.OrderByDescending(a => a.CreatedAt);

            // Setting for number  of auctions on front page
            var ResultsCount = DB.Settings.Find("N").Value;
            AuctionList = AuctionList.Take(ResultsCount);

            // pass the current arguments
            ViewBag.Args = new
            {
                Name = name,
                Minprice = minprice,
                Maxprice = maxprice,
                Status = status
            };

            return View(AuctionList.ToList());
        }

        /**
         * Get single user
         **/ 
        public ActionResult Auction(int? id)
        {
            if (id == null)
                return View("Error"); // Return to Index page
            // Fetch auction
            Auction auction = DB.Auctions.Include("User").FirstOrDefault(a => a.Id == id);

            if(auction == null)
                return HttpNotFound();

            // Fetch bids
            var bids = DB.Bids.Where(b => b.AuctionId == id).OrderByDescending(a => a.CreatedAt).ToList();
            ViewBag.Bids = bids;

            ViewBag.Price = bids.Any() ? bids.First().Amount : auction.StartPrice;

            return View(auction);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}