using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ATCallback.Config
{
    public class ATCallbackConfig
    {
        //Change to "production" when not using the sandbox
        public string environment = "sandbox";

        public string sandboxUsername = "";
        public string sandboxApiKey = "";

        public string liveUsername = "";
        public string liveApiKey = "";

        //SMS
        //Set your sender ID
        public string senderID = "";

        //PAYMENT
        public string sandboxPaymentProduct = "";
        public string livePaymentProduct = "";
    }
}