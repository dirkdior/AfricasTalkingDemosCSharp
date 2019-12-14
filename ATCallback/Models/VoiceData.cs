using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ATCallback.Models
{
    public class VoiceData
    {
        public string isActive { get; set; }

        public string sessionId { get; set; }

        public string direction { get; set; }

        public string callerNumber { get; set; }

        public string destinationNumber { get; set; }

        public string dtmfDigits { get; set; }

        public string recordingUrl { get; set; }

        public string durationInSeconds { get; set; }

        public string currencyCode { get; set; }

        public string amount { get; set; }
    }
}