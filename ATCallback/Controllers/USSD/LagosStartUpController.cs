using ATCallback.Models;
using AfricasTalkingCS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using AT_WebTests.Resources;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace ATCallback.Controllers.USSD
{
    //Source codes for Lagos Startup Week demo involving Africa's Talking USSD and Payment APIs for Payment over USSD
    [RoutePrefix("api/lsw")]
    public class LagosStartUpController : ApiController
    {
        AfricasTalkingAPIConnect apiConnect = new AfricasTalkingAPIConnect();

        //StartUp Week Demo of USSD + PAYMENT
        //sandbox/ussd path is to allow tests with the Payment API using the sandbox
        [Route("sandbox/ussd")]
        [HttpPost, ActionName("sandbox/ussd")]
        public HttpResponseMessage Ussd([FromBody]USSDData ServerResponse)
        {
            HttpResponseMessage rs;
            string response;

            try
            {
                //Checks if the user has a pending unverified payment
                string userTranxID = CheckUser(ServerResponse.phoneNumber);
                
                if (userTranxID == null)
                {
                    //Set of responses for a new User
                    response = lswNewUserDial(ServerResponse, "sandbox");
                }
                else
                {
                    //Set of responses for a user who has an existing payment
                    response = lswConfirmOTPDial(ServerResponse, userTranxID, "sandbox");
                }
            }
            catch (Exception ex)
            {
                response = "END " + ex;

            }



            rs = Request.CreateResponse(HttpStatusCode.Created, response);
            rs.Content = new StringContent(response, Encoding.UTF8, "text/plain");
            return rs;
        }

        //live/ussd path is to allow tests with the Payment API using the live environment
        [Route("live/ussd")]
        [HttpPost, ActionName("live/ussd")]
        public HttpResponseMessage LiveUssd([FromBody]USSDData ServerResponse)
        {
            HttpResponseMessage rs;
            string response;

            try
            {
                string userTranxID = CheckUser(ServerResponse.phoneNumber);

                if (userTranxID == null)
                {
                    response = lswNewUserDial(ServerResponse, "live");
                }
                else
                {
                    response = lswConfirmOTPDial(ServerResponse, userTranxID, "live");
                }
            }
            catch (Exception ex)
            {
                response = "END " + ex;

            }

            rs = Request.CreateResponse(HttpStatusCode.Created, response);
            rs.Content = new StringContent(response, Encoding.UTF8, "text/plain");
            return rs;
        }

        //This method checks if this same mobile number has a pending unvalidated payment.
        public string CheckUser(string phoneNumber)
        {
            //Logic here checks for a PaymentLogs.csv file on the server that contains the information of all pending and completed payments
            //You can change this to check your database instead
            var strLines = File.ReadAllLines(@"h:\root\home\dirkdior-002\www\apiedislodger\logs\PaymentLogs.csv");
            foreach (var line in strLines)
            {
                if (line.Split(',')[0].Equals(phoneNumber))
                {
                    return line.Split(',')[1];
                }
            }
            return null;
        }

        //Dial from a new User
        public string lswNewUserDial(USSDData ServerResponse, string state)
        {
            string response;

            if (ServerResponse.text == null)
            {
                response = "CON Welcome\n";
                response += "Please pick an amount to pay";
            }
            else
            {
                
                string[] input = ServerResponse.text.Split('*');
                int step = input.Count();

                if (step == 1)
                {
                    response = "CON Enter Card Number";
                }
                else if (step == 2)
                {
                    response = "CON Enter CVV";
                }
                else if (step == 3)
                {
                    response = "CON Enter Month and Year of Card Expiration\n";
                    response += "In the format MONTH/YEAR";
                }
                else if (step == 4)
                {
                    response = "CON Please provide the Card PIN";
                }
                else if (step == 5)
                {

                    try
                    {
                        var result = DebitCard(input, state);
                        var res = JsonConvert.DeserializeObject(result);
                        string transactionId = res["transactionId"];
                        string status = res["status"];
                        string description = res["description"];

                        if (status == "PendingValidation")
                        {
                            response = "END Thanks, please dial *347*007# again after recieving an OTP";

                            if(state != "live")
                                apiConnect.SingleSMS(ServerResponse.phoneNumber, "Please enter the OTP: 1234 to confirm your payment. Dial *347*007# again to complete the payment.");

                            SaveToCSV(ServerResponse.phoneNumber, transactionId, "Lagos StartUp Week Demo", input[0], status);
                        }
                        else
                        {
                            response = "END Sorry, an error occured and the payment wasn't successful\n";
                            response += "Please try again\n";
                            response += status + "\n" + description;
                        }

                    }
                    catch (AfricasTalkingGatewayException ex)
                    {
                        response = "END Sorry, an error occured and the payment wasn't successful\n";
                        response += "Please try again"+ex;

                    }

                }
                else
                {
                    response = "END Invalid Option";
                }
            }

            return response;
        }

        //This Method converts the data aquired on the USSD flow into one that can be sent to Africa's Talking SDK to debit the user
        public dynamic DebitCard(string[] usrInput, string state)
        {
            CardPaymentData data = new CardPaymentData();
            data.Amount = Int32.Parse(usrInput[0]);
            data.CardNum = usrInput[1];
            data.CardCvv = Int32.Parse(usrInput[2]);

            string[] monAndYear = usrInput[3].Split('/');
            data.ValidTillMonth = Int32.Parse(monAndYear[0]);
            data.ValidTillYear = Int32.Parse(monAndYear[1]);
            data.CardPin = usrInput[4];

            var dataFromAPI = apiConnect.DebitUserCard(data, state);
            
            return dataFromAPI;
        }

        //This method collects the data and saves to a CSV file on the server, this can be changed to save to your database instead
        public void SaveToCSV(string phoneNumber, string trxID, string Narration, string Amount, string Status)
        {
            var csv = new StringBuilder();

            var newLine = string.Format("{0},{1},{2},{3},{4}", phoneNumber, trxID, Narration, Amount, Status);
            csv.AppendLine(newLine);

            string path = @"h:\root\home\dirkdior-002\www\apiedislodger\logs\PaymentLogs.csv";

            if (!File.Exists(path))
            {
                File.Create(path).Dispose();

                File.AppendAllText(path, csv.ToString());
            }
            else if (File.Exists(path))
            {
                File.AppendAllText(path, csv.ToString());
            }
        }

        //For a user who has an existing pending payment, this method allows the USSD flow go straight to asking the user for the OTP to confirm the pending transaction
        public string lswConfirmOTPDial(USSDData ServerResponse, string tranxID, string state)
        {
            //Whatever user dials should confirm OTP
            string response;
            if (ServerResponse.text == null)
            {
                response = "CON Confirm your Payment\n";
                response += "Enter OTP";
            }
            else
            {
                string OTP = ServerResponse.text;

                var otpConfirmation = apiConnect.ValidateOTP(tranxID, OTP, state);

                var res = JsonConvert.DeserializeObject(otpConfirmation);
                string transactionResult = res["description"];
                string status = res["status"];

                response = "END " + status + "\n" + transactionResult;

                if (status == "Success")
                {
                    response = "END Your payment was successful. \nThanks for making your payment promptly.";
                    apiConnect.SingleSMS(ServerResponse.phoneNumber, "Thanks for making your payment promptly. We will be sending you some airtime as a gift for paying back on time.");
                    apiConnect.BuyAirtimeLive(ServerResponse.phoneNumber, 50);
                }

                ChangeStatus(ServerResponse.phoneNumber, status);
            }

            return response;
        }

        //This method changes the state of the transaction in the log file.
        //This can be changed to edit the data in your database
        public void ChangeStatus(string phoneNumber, string status)
        {
            string file = @"h:\root\home\dirkdior-002\www\apiedislodger\logs\PaymentLogs.csv";

            string strLineString = File.ReadAllText(file);
            string newData = strLineString.Replace(phoneNumber, status + "" + phoneNumber);

            File.Delete(file);
            File.Create(file).Dispose();
            File.AppendAllText(file, newData);
        }
    }
}