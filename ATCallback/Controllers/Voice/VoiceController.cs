using ATCallback.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace ATCallback.Controllers.Voice
{
    //Web API callback for handling incoming Voice Calls from Africa's Talking APIs
    [RoutePrefix("api/voice")]
    public class VoiceController : ApiController
    {
        [Route("incomingcall")]
        [HttpPost, ActionName("incomingcall")]
        public HttpResponseMessage IncomingCall([FromBody]VoiceData dataFromCall)
        {
            HttpResponseMessage rs;
            string response;
            
            if(dataFromCall.isActive == "1")
            {
                string text = "Welcome, playing an Audio to you now...";

                response = "<Response>";
                response += "<Say>" + text + "</Say>";

                //Plays an audio from an mp3 file sitting on a webserver
                response += "<Play url=\"http://api.edislodger.com/logs/audio.mp3\"/>";

                response += "</Response>";

                rs = Request.CreateResponse(HttpStatusCode.Created, response);
                rs.Content = new StringContent(response, Encoding.UTF8, "text/plain");

                return rs;
            }
            else
            {
                return null;
            }
        }
    }
}
