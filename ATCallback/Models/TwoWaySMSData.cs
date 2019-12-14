using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ATCallback.Models
{
    public class TwoWaySMSData
    {
        public string from { get; set; }
        public string to { get; set; }
        public string text { get; set; }
        public string date { get; set; }
        public string id { get; set; }
        public string linkId { get; set; }
    }
}