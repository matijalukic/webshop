using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebShop.Models;

namespace WebShop.Controllers
{
    public class AuctionsController : Controller
    {
        private Prodavnica db = new Prodavnica();



        // GET: Auctions
        [Authorize]
        public ActionResult Index()
        {
            var LoggedUser = Session["LoggedUser"] as User;
            if (LoggedUser == null)
                return RedirectToAction("Index", "Home");

            var MyAuctuions = db.Auctions.Where(a => a.UserId == LoggedUser.Id).ToList();
            return View(MyAuctuions);
        }

        [Authorize]
        public ActionResult Pending()
        {
            var LoggedUser = Session["LoggedUser"] as User;
            if (db.Users.Find(LoggedUser.Id).IsAdmin)
            {
                var ReadyAuctions = db.Auctions.Where(a => a.State == "Ready").ToList();

                return View(ReadyAuctions);
            }
            return RedirectToAction("Index");
        }

        [Authorize]
        public ActionResult WonAuctions()
        {
            var LoggedUser = Session["LoggedUser"] as User;
            if (LoggedUser == null)
                return RedirectToAction("Index", "Home");

            var MyAuctuions = db.Auctions.Where(a => a.WinnerId == LoggedUser.Id).ToList();
            return View(MyAuctuions);
        }

        [Authorize]
        public ActionResult Open(int id)
        {
            var LoggedUser = Session["LoggedUser"] as User;
            if (LoggedUser.IsAdmin)
            {
                var AuctionToOpen = db.Auctions.Find(id);
                AuctionToOpen.State = AuctionState.Opened;
                AuctionToOpen.ClosedAt = DateTime.Now.AddSeconds(AuctionToOpen.Duration);


                Session["success"] = "The auction " + AuctionToOpen.Name + " is open!";
                db.SaveChanges();
            }

            return RedirectToAction("Pending");
        }

        // GET: Auctions/Details/5
        [Authorize]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Auction auction = db.Auctions.Find(id);
            if (auction == null)
            {
                return HttpNotFound();
            }
            return View(auction);
        }

        // GET: Auctions/Create
        [Authorize]
        public ActionResult Create()
        {
            ViewBag.DefaultDuration = db.Settings.Find("D").Value;
            return View();
        }

        // POST: Auctions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Create([Bind(Include = "Name,Image,Duration,StartPrice,Price")] Auction auction, HttpPostedFileBase Image)
        {
            string imagePath, pathForDb;
            if (Image != null && Image.ContentLength > 0)
                try
                {
                    if(!Directory.Exists(Server.MapPath("~/Images"))) Directory.CreateDirectory(Server.MapPath("~/Images"));
                    imagePath = Path.Combine(Server.MapPath("~/Images"), Path.GetFileName(Image.FileName));
                    pathForDb = "Images/" + Path.GetFileName(Image.FileName);
                    Image.SaveAs(imagePath);
                    ViewBag.Message = "File uploaded successfully";
                }
                catch (Exception ex)
                {
                    Session["error"] = "ERROR:" + ex.Message.ToString();
                    return View(auction);

                }
            else
            {
                Session["error"] = "You have not specified a file.";
                return View(auction);

            }
            if (!ModelState.IsValid)
                return View(auction);

            var UserId = (Session["LoggedUser"] as User).Id;

            auction.State = "Ready";
            auction.CreatedAt = DateTime.Now;
            auction.UserId = UserId;
            auction.Picture = pathForDb;
            
            db.Auctions.Add(auction);
            db.SaveChanges();


            return RedirectToAction("Index");

        }

        // GET: Auctions/Edit/5
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Auction auction = db.Auctions.Find(id);
            if (auction == null)
            {
                return HttpNotFound();
            }
            return View(auction);
        }

        // POST: Auctions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Picture,Duration,StartPrice,Price")] Auction auction)
        {

            if (ModelState.IsValid)
            {
                var ChangingAuction = db.Auctions.Find(auction.Id);

                ChangingAuction.Name = auction.Name;
                ChangingAuction.Picture = auction.Picture;
                ChangingAuction.Duration = auction.Duration;
                ChangingAuction.StartPrice = auction.StartPrice;
                ChangingAuction.Price = auction.Price;
                db.Entry(ChangingAuction).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            return View(auction);
        }

        // GET: Auctions/Delete/5
        [Authorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Auction auction = db.Auctions.Find(id);
            if (auction == null)
            {
                return HttpNotFound();
            }
            return View(auction);
        }

        // POST: Auctions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult DeleteConfirmed(int id)
        {
            
            Auction auction = db.Auctions.Find(id);
            if(auction.State == "Ready")
            {
                db.Auctions.Remove(auction);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
        
        // GET: Auction/Bid/5
        [Authorize]
        public ActionResult Bid(int AuctionId, int Amount)
        {
            var AuctionOnBid = db.Auctions.Find(AuctionId);
            var AuctionsBids = db.Bids.Where(a => a.AuctionId == AuctionId);
            var LoggedUser = Session["LoggedUser"] as User;

            if (LoggedUser == null)
                return RedirectToAction("Index", "Home");


            // ako je veci od najveceg ponudjenog
            int? MaxBidAmount = AuctionsBids.Any() ? AuctionsBids.Max(a => a.Amount) : AuctionOnBid.StartPrice;
            if(Amount > MaxBidAmount.GetValueOrDefault())
            {

                // Maximum old bid
                var MyBids = db.Bids.Where(b => b.AuctionId == AuctionId && b.UserId == LoggedUser.Id);
                int MaxOldBid = MyBids.Any() ? MyBids.Max(a => a.Amount) : 0;

                int AmountToPay = Amount - MaxOldBid;

                // Decreas users number of tokens
                if(LoggedUser.Tokens < AmountToPay)
                {
                    Session["error"] = "Not enough funds";
                    Redirect(Request.UrlReferrer.ToString());
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
                Session["success"] = "Your bid has been placed";
            }
            else
            {
                Session["error"] = "Your Bid must be the highest and higher than start price!";
            }
                       

            return Redirect(Request.UrlReferrer.ToString());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
