using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebShop.Models.Validation
{
    // Login form
    public class Login
    {
        [Required]
        [Display(Name ="Email")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

    }
    // Registration form
    public class Registration
    {
        [Required]
        [Display(Name = "Name")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [Required]
        public string Surname { get; set; }

        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        [Remote("EmailExists", "User", ErrorMessage = "Email exists!")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "Password's doesnt match!")]
        public string ConfirmPassword { get; set; }
    }

    // Change password 
    public class NewPassword
    {
        [Required]
        [DataType(DataType.Password)]

        public string OldPassword { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "Password's doesnt match!")]
        public string ConfirmPassword { get; set; }

        public bool IsMatched()
        {
            return Password == ConfirmPassword;
        }
    }

}