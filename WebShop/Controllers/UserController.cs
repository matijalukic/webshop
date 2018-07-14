using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebShop.Models;
using WebShop.Models.Validation;
using System.Security.Cryptography;
using System.Text;
using System.Data.SqlClient;
using System.Web.Security;
using System.Data.Entity;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;

namespace WebShop.Controllers
{
    public class UserController : Controller, IDisposable
    {
        

        // Database
        private Prodavnica DB = new Prodavnica();


        // checking if there is mail
        public bool EmailExists(string email)
        {
            var UserWithEmail = DB.Users.Where(u => u.Email == email).First();
            return UserWithEmail != null;
        }

        // Hasing algorithm
        static string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // GET: Login form
        [AllowAnonymous]
        public ActionResult Login()
        {
            //if the user is logged in, send the to the home page
            if (HttpContext.User.Identity.IsAuthenticated){
                return RedirectToAction("Index", "Home");
            }

            return View();
        }
        
        // POST Login Request
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(Login LoginInfo)
        {
            if (!ModelState.IsValid)
                return View(LoginInfo);
            // if data is validated 
            if (ModelState.IsValid)
            {
                var UserToLogin = DB.Users.Where(a => a.Email == LoginInfo.Email).First();

                var HashedInputPassword = ComputeSha256Hash(LoginInfo.Password);

                // password matched
                if (UserToLogin != null && UserToLogin.Password.ToLower() == HashedInputPassword.ToLower())
                {
                // mark the user as logged in
                    FormsAuthentication.SetAuthCookie(UserToLogin.Email, false);
                    // Save info in session
                    Session["LoggedUser"] = UserToLogin;
                    Session["UsersName"] = UserToLogin.Name;
                    Session["UserId"] = UserToLogin.Id;
                    // Return to home
                    return RedirectToAction("Index", "Home");
                }

                Session["error"] = "Your email and password does not match!"; 
                return View(LoginInfo);


            }
            return Redirect(Request.UrlReferrer.ToString());
        }

        


        // Get Logout
        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            Session.Remove("LoggedUser");
            Session.Remove("UsersName");
            Session.Remove("UserId");

            return RedirectToAction("Login");
        }

        // GET: Register form
        [AllowAnonymous]
        public ActionResult Register()
        {

            return View();
        }

        // POST persist user
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult Register(
            [Bind(Include ="Name,Surname,Email,Password,ConfirmPassword")]
                Registration RegistrationRequest)
        {
            // Validacija
            if (!ModelState.IsValid) return View(RegistrationRequest);
            // validacija emaila
            if (DB.Users.Any(u => u.Email == RegistrationRequest.Email))
            {
                Session["error"] = "Email has been taken! Please login!";
                return View(RegistrationRequest);
            }


            if (ModelState.IsValid)
            {
              
                // Add to database
                DB.Users.Add(new Models.User
                {
                    Tokens = 0,
                    Name = RegistrationRequest.Name,
                    Surname = RegistrationRequest.Surname,
                    Email = RegistrationRequest.Email,
                    Password = ComputeSha256Hash(RegistrationRequest.Password)
                });

                // Sa try da catchuje exception baze
                try
                {
                    DB.SaveChanges();
                }
                catch(SqlException e)
                {
                    Session["error"] = e.Message;
                    return View();
                }
                Session["success"] = "You have registered successfully! Please log in.";
                return RedirectToAction("Login");
            }

            return View();
        }

        [Authorize]
        public ActionResult Index()
        {
            if (!(System.Web.HttpContext.Current.User != null) || !System.Web.HttpContext.Current.User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            // import user in session by reading email from cookies
            var LoggedUserEmail = System.Web.HttpContext.Current.User.Identity.Name;
            if (Session["LoggedUser"] == null)
                Session["LoggedUser"] = DB.Users.Where(u => u.Email == LoggedUserEmail).FirstOrDefault();

            return View(Session["LoggedUser"]);
        }


        // change profile request
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(
            [Bind(Include ="Name,Surname,Email")]
            User LoggedUser)
        {
            User ChangingUser = DB.Users.Find((Session["LoggedUser"] as User).Id); 

            ChangingUser.Name = LoggedUser.Name;
            ChangingUser.Surname = LoggedUser.Surname;
            ChangingUser.Email = LoggedUser.Email;
            DB.Entry(ChangingUser).State = EntityState.Modified;
            DB.SaveChanges();

            // Update logged user in section
            Session["LoggedUser"] = ChangingUser;
            Session["UsersName"] = ChangingUser.Name;

            Session["success"] = "You profile has been changed!";

            return View(ChangingUser);
        }

        // Change pass request
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(
            [Bind(Include ="OldPassword,Password,ConfirmPassword")]
            NewPassword NewPasswordRequest)
        {
            if(!NewPasswordRequest.IsMatched())
            {
                Session["error"] = "New passwords aren't matched!";
                return RedirectToAction("Index");
            }


            var LoggedUser = Session["LoggedUser"] as User;
            var LoggedUserInDB = DB.Users.SingleOrDefault(u => u.Email == LoggedUser.Email);

            if (LoggedUser != null)
            {
                var oldHasedPassword = ComputeSha256Hash(NewPasswordRequest.OldPassword);
                var newHashedPassword = ComputeSha256Hash(NewPasswordRequest.Password);
                    
                // if old password is same as in db
                if(LoggedUserInDB.Password.ToLower() == oldHasedPassword.ToLower())
                {
                    LoggedUserInDB.Password = newHashedPassword;
                    DB.SaveChanges();
                    Session["success"] = "Password has been changed"; 
                }
                else
                {
                    Session["error"] = "Old passwords doesnt match!"; 
                }
            }

            return RedirectToAction("Index");
        }

        // Orders per page listed
        public static int OrdersPerPage = 20;
        [Authorize]
        public ActionResult Orders(int? start = 0)
        {
            ViewBag.Title = "Token Orders";
            int startOrder = start.GetValueOrDefault();
            ViewBag.Start = startOrder;


            var UserId = Session["UserId"] as int?;
            var OrderOfUser = DB.TokenOrders.Where(t => t.UserId == UserId).OrderByDescending(t => t.CreatedAt).Skip(() => startOrder).Take(() => OrdersPerPage);


            ViewBag.TotalOrders = DB.TokenOrders.Where(t => t.UserId == UserId).Count();

            return View(OrderOfUser);
        }

        [Authorize]
        public ActionResult BuyTokens()
        {

            ViewBag.Silver = DB.Settings.Where(s => s.Key == "S").First().Value;
            ViewBag.Gold = DB.Settings.Where(s => s.Key == "G").First().Value;
            ViewBag.Platinum = DB.Settings.Where(s => s.Key == "P").First().Value;
            ViewBag.Price = DB.Settings.Where(s => s.Key == "T").First().Value;

            return View();
        }
        

        [Authorize]
        public ActionResult CreateTokenTransactions(int amount)
        {
            var LoggedUser = Session["LoggedUser"] as User;
            if (LoggedUser == null)
                return RedirectToAction("Index");

            var Prices = new
            {
                Silver = DB.Settings.Where(s => s.Key == "S").First().Value,
                Gold = DB.Settings.Where(s => s.Key == "G").First().Value,
                Platinum = DB.Settings.Where(s => s.Key == "P").First().Value,
            };
            // prices in check
            if (amount != Prices.Silver && amount != Prices.Gold && amount != Prices.Platinum)
                return RedirectToAction("PaymentFailed");


            // calculate price
            int currency = DB.Settings.Find("C").Value;
            int pricePerToken = DB.Settings.Find("T").Value;
            decimal price = Currencies.ConvertToRsd(pricePerToken * amount, currency);


            // make submited order
            var NewTokenOrder = DB.TokenOrders.Add(new TokenOrder()
            {
                UserId = LoggedUser.Id,
                State = "SUBMITTED",
                Amount = amount,
                Price = price,
                CreatedAt = DateTime.Now,
            });

            DB.SaveChanges();
            
            return Redirect("http://stage.centili.com/payment/widget?apikey=ce797d8eeab183872300e64cd877a4e3&country=rs&reference=" + NewTokenOrder.Id + "&returnurl=http://matijaiep.azurewebsites.net/User/Confirm"); // &price=" + price);
        }

        
        // sending email
        private async Task PostMessage(TokenOrder Order, User User)
        {
            var apiKey = System.Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
            //using (SmtpClient smtpClient = new SmtpClient("smtp.sendgrid.net", 587))
            //{
            //    var basicCredential = new NetworkCredential("azure_fd3fd6f65f786f561329c9cbf9b8201d@azure.com", "Zaboravimstalno060796@");
            //    using (MailMessage message = new MailMessage())
            //    {
            //        MailAddress fromAddress = new MailAddress("noreply@matijaiep.com");

            //        smtpClient.Host = "matijaiep.azurewebsites.net";
            //        smtpClient.UseDefaultCredentials = false;
            //        smtpClient.Credentials = basicCredential;

            //        message.From = fromAddress;
            //        message.Subject = "Kupovina Tokena";
            //        // Set IsBodyHtml to true means you can send HTML email.
            //        message.IsBodyHtml = true;
            //        message.Body = "<h1>Kupili ste " + Order.Amount + " Tokena</h1>";
            //        message.Body += "<p>Po ceni od " + Order.Price + " RSD</p>";
            //        message.To.Add(User.Email);

            //        smtpClient.Send(message);
            //    }
            //}


            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("noreply@matijaiepazurewebsites.net", "Matija Lukic"),
                Subject = "Kupovina Tokena",
                PlainTextContent = "Zdravo!",
                HtmlContent = "<h1>Kupili ste " + Order.Amount + " Tokena</h1><p>Po ceni od " + Order.Price + " RSD</p>"
            };
            msg.AddTo(new EmailAddress(User.Email));
            await client.SendEmailAsync(msg);
        }

        [AllowAnonymous]
        public async Task<ActionResult> Confirm(string reference, string status)
        {
            var OrderId = Int32.Parse(reference);
            var OrderToConfirm = DB.TokenOrders.Find(OrderId);
            if (OrderToConfirm != null)
            {
                var UserWhoGetsTokens = DB.Users.Where(a => a.Id == OrderToConfirm.UserId).SingleOrDefault();

                if (UserWhoGetsTokens != null)
                {
                    // notify user and update balance
                    if (status == "success" && OrderToConfirm.CompletedAt == null)
                    {
                        try
                        {
                            await PostMessage(OrderToConfirm, UserWhoGetsTokens);
                        }
                        catch (Exception ex)
                        {
                            Session["error"] = ex.InnerException.Message;
                        }

                        UserWhoGetsTokens.Tokens += OrderToConfirm.Amount;
                        DB.Entry(UserWhoGetsTokens).State = EntityState.Modified;
                    }
                    OrderToConfirm.State = status == "success" ? "COMPLETED" : "CANCELED";
                    OrderToConfirm.CompletedAt = DateTime.Now;

                    DB.Entry(OrderToConfirm).State = EntityState.Modified;
                    DB.SaveChanges();
                }
            }


            return RedirectToAction("Orders");
        }

        public ActionResult PaymentSuccessfull()
        {

            return View();
        }

        public ActionResult PaymentFailed()
        {

            return View();
        }


        protected override void Dispose(bool disposing)
        {
            DB.Dispose();
            base.Dispose(disposing);
        }
    }
}