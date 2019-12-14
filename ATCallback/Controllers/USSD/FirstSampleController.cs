using AT_WebTests.Resources;
using ATCallback.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace ATCallback.Controllers.USSD
{
    //Simple Web API Callback for USSD
    [RoutePrefix("api/firstsample")]
    public class FirstSampleController : ApiController
    {
        //Initiates the Class used to handle all interactions with the Africa's Talking SDK
        AfricasTalkingAPIConnect apiConnect = new AfricasTalkingAPIConnect();

        [Route("ussd")]
        // specify the actual route, your url will look like... localhost:8080/api/firstsample/ussd...
        [HttpPost, ActionName("ussd")]
        // state that the method you intend to create is a POST
        public HttpResponseMessage ussd([FromBody]USSDData ServerResponse)
        {
            // declare a complex type as input parameter
            HttpResponseMessage rs;
            string response;
            if (ServerResponse.text == null)
            {
                ServerResponse.text = "";
            }

            // loop through the server's text value to determine the next cause of action
            if (ServerResponse.text.Equals("", StringComparison.Ordinal))
            {
                // always include a 'CON' in your first statements
                response = "CON Welcome to Africa's Talking\n";
                response += "1. Show your number";
            }
            else if (ServerResponse.text.Equals("1", StringComparison.Ordinal))
            {
                response = "END Your number is " + ServerResponse.phoneNumber;

                //Make a voice call to the phone number using Africa's Talking API, uncomment the next like to try this out in your USSD flow.
                //var result = apiConnect.MakeACall("+23417006124", ServerResponse.phoneNumber);
                //response += "/n "+ result;
            }
            else
            {
                response = "END invalid option";
            }

            rs = Request.CreateResponse(HttpStatusCode.Created, response);

            // append your response to the HttpResponseMessage and set content type to text/plain, exactly what the server expects
            rs.Content = new StringContent(response, Encoding.UTF8, "text/plain");

            // finally return your response to the server
            return rs;
        }

    }

}
