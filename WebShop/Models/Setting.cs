using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebShop.Models
{
    public static class Currencies
    {

        public static string RSD{ get { return "RSD"; } }
        public static string EUR { get { return "EUR"; } }
        public static string USD { get { return "USD"; } }

        public static string Currency(int value)
        {
            string[] MapCurrency = new string[] { RSD, EUR, RSD };
            return MapCurrency[value];
        }

        public static decimal ConvertToRsd(decimal value, int currency)
        {
            decimal[] Rates = new decimal[] { 1, 120, 100 };
            if (currency < 0 || currency >= Rates.Length) currency = 0;

            return value * Rates[currency];
        }

}
    [Table("Setting")]
    public class Setting
    {
        
        [Key]
        public string Key { get; set; }
        
        public int Value { get; set; }

    }
}