namespace WebShop.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using WebShop.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<WebShop.Models.Prodavnica>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(Prodavnica context)
        {
            // Find with mail
            var UserToDelete = context.Users.Where(u => u.Email == "matija996@gmail.com").SingleOrDefault();

            if (UserToDelete != null)
            {
                context.Entry(UserToDelete).State = EntityState.Deleted;
                context.SaveChanges();
            }
            
            User firstUser = new User
            {
                Id = 1,
                Name = "Matija",
                Surname = "Lukic",
                Email = "matija996@gmail.com",
                Password = "3da3e5318fc9e6031e87fdd540c67eb85d099d0558cf7c17eb7b0b7aa12fcd13", // matija123
                Tokens = 100,
                IsAdmin = true,
            };
            context.Users.AddOrUpdate(firstUser);

            context.Users.AddOrUpdate(new User
            {
                Id = 2,
                Name = "Nemanja",
                Surname = "Kojic",
                Email = "nemanja.kojic@etf.rs",
                Password = "024957f93d238818ba6814ffa257e6b2a5673c4ed4c0dadd2732682cca6ceac8", // nemanja123
                Tokens = 1000,
                IsAdmin = false,
            });



            context.Auctions.AddOrUpdate(new Auction
            {
                Id = 1,
                Name = "Cola",
                Picture = "https://www.coca-cola.rs/content/dam/GO/coca-cola/sebia/One%20Brand/coca-cola-logo-260x260.png",
                Duration = 3600,
                StartPrice = 1,
                Price = 1000,
                CreatedAt = DateTime.Now,
                State = "Ready",
                UserId = firstUser.Id,
                WinnerId = null,
            });

            context.Auctions.AddOrUpdate(new Auction
            {
                Id = 2,
                Name = "Fanta",
                Picture = "https://www.coca-cola.rs/content/dam/GO/fanta-2gen/shared/logo/Logo_260x260.png",
                Duration = 3600,
                StartPrice = 150,
                Price = 900,
                CreatedAt = DateTime.Now,
                State = "Ready",
                UserId = firstUser.Id,
                WinnerId = null,
            });



            // Add settings
            context.Settings.AddOrUpdate(new Setting[] {
                new Setting { Key = "N", Value = 20,},
                new Setting { Key = "D", Value = 3600,},
                new Setting { Key = "S", Value = 30,},
                new Setting { Key = "G", Value = 50},
                new Setting { Key = "P", Value = 100},
                new Setting { Key = "C", Value = 0}, // 0 = RSD, 1 = EUR, 2 = USD
                new Setting { Key = "T", Value = 20},
            });
        }
    }
}
