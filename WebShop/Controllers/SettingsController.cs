using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebShop.Models;

namespace WebShop.Controllers
{
    public class SettingsController : Controller
    {
        private Prodavnica db = new Prodavnica();

        // GET: Settings
        [Authorize]
        public ActionResult Index()
        {
            var LoggedUser = Session["LoggedUser"] as User;
            if (LoggedUser == null || !db.Users.Find(LoggedUser.Id).IsAdmin) return HttpNotFound();

            ViewBag.Currency = db.Settings.Find("C").Value;

            return View(db.Settings.ToList());
        }

        // GET: Settings/Details/5
        [Authorize]
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Setting setting = db.Settings.Find(id);
            if (setting == null)
            {
                return HttpNotFound();
            }
            return View(setting);
        }

        //// GET: Settings/Create
        //public ActionResult Create()
        //{
        //    return View();
        //}

        //// POST: Settings/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create([Bind(Include = "Key,Value")] Setting setting)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Settings.Add(setting);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }

        //    return View(setting);
        //}

        // GET: Settings/Edit/5
        public ActionResult Edit(string id)
        {
            var LoggedUser = Session["LoggedUser"] as User;
            if (LoggedUser == null || !db.Users.Find(LoggedUser.Id).IsAdmin) return HttpNotFound();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Setting setting = db.Settings.Find(id);
            if (setting == null)
            {
                return HttpNotFound();
            }
            return View(setting);
        }

        // POST: Settings/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Edit([Bind(Include = "Key,Value")] Setting setting)
        {

            var LoggedUser = Session["LoggedUser"] as User;
            if (LoggedUser == null || !db.Users.Find(LoggedUser.Id).IsAdmin) return HttpNotFound();

            if (ModelState.IsValid)
            {
                db.Entry(setting).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(setting);
        }

        // GET: Settings/Delete/5
        //public ActionResult Delete(string id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Setting setting = db.Settings.Find(id);
        //    if (setting == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(setting);
        //}

        // POST: Settings/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(string id)
        //{
        //    Setting setting = db.Settings.Find(id);
        //    db.Settings.Remove(setting);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

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
