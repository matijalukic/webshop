namespace WebShop.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [StringLength(50)]
        [Required]
        public string Surname { get; set; }

        [Index(IsUnique = true)]
        [StringLength(50)]
        [Required]
        public string Email { get; set; }

        [StringLength(100)]
        public string Password { get; set; }

        public int Tokens { get; set; }

        [DefaultValue(false)]
        public bool IsAdmin { get; set; }
                
        public ICollection<TokenOrder> TokenOrders { get; set; }

    }
}
