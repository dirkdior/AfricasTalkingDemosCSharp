using AT_WebTests.Resources;
using ATCallback.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;

namespace ATCallback.Controllers.SMS
{
    //Web API Callback for Incoming SMS
    [RoutePrefix("api/smsreciever")]
    public class SMSRecieverController : ApiController
    {
        AfricasTalkingAPIConnect connection = new AfricasTalkingAPIConnect();

        [Route("incomingsms")]
        [HttpPost, ActionName("incomingsms")]
        public void IncomingSMS([FromBody]TwoWaySMSData ServerResponse)
        {
            if(ServerResponse.text != "")
            {
                connection.SingleSMS(ServerResponse.from, "Message From: " + ServerResponse.from + " To: " + ServerResponse.to);

                connection.BuyAirtime(ServerResponse.from, 500);
            }
        }
    }
}
