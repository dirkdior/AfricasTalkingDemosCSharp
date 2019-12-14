using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ATCallback.Models
{
    public class CardPaymentData
    {
        public string TransactionId { get; set; }
        public string ProductName { get; set; }
        public string CurrencyCode { get; set; }
        public int Amount { get; set; }
        public string Narration { get; set; }
        public int CardCvv { get; set; }
        public string CardNum { get; set; }
        public string CountryCode { get; set; }
        public string CardPin { get; set; }
        public int ValidTillMonth { get; set; }
        public int ValidTillYear { get; set; }
        public string Status { get; set; }
    }
}