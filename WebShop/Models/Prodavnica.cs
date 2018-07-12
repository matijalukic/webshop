namespace WebShop.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class Prodavnica : DbContext
    {
        public Prodavnica()
            : base("name=Prodavnica1")
        {
            Database.SetInitializer<Prodavnica>(null);
        }

        public virtual DbSet<Auction> Auctions { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Bid> Bids { get; set; }
        public virtual DbSet<TokenOrder> TokenOrders { get; set; }
        public virtual DbSet<Setting> Settings { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // relationship auctions and bids
            modelBuilder.Entity<Auction>()
                .HasMany<Bid>(a => a.Bids)
                ;

            modelBuilder.Entity<Auction>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<Auction>()
                .Property(e => e.Picture)
                .IsUnicode(false);

            modelBuilder.Entity<Auction>()
                .Property(e => e.State)
                .IsUnicode(false);

            modelBuilder.Entity<User>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<User>()
                .Property(e => e.Surname)
                .IsUnicode(false);

            modelBuilder.Entity<User>()
                .Property(e => e.Email)
                .IsUnicode(false);

            modelBuilder.Entity<User>()
                .Property(e => e.Password)
                .IsUnicode(false);
        }
    }
}
