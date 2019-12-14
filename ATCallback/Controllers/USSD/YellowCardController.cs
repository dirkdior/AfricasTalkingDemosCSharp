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
    //Source codes for YellowCard demo involving Africa's Talking USSD and Payment APIs for Payment over USSD
    [RoutePrefix("api/ussd")]
    public class YellowCardController : ApiController
    {
        AfricasTalkingAPIConnect apiConnect = new AfricasTalkingAPIConnect();

        //YellowCard Demo of USSD + PAYMENT
        [Route("yellowcard")]
        [HttpPost, ActionName("yellowcard")]
        public HttpResponseMessage Ussd([FromBody]USSDData ServerResponse)
        {
            HttpResponseMessage rs;
            string response;
            //Checks if the user has a pending unverified payment
            string userTranxID = CheckUser(ServerResponse.phoneNumber);

            if (userTranxID == null)
            {
                //Set of responses for a new User
                response = NewUserDial(ServerResponse);
            }
            else
            {
                //Set of responses for a user who has an existing payment
                response = ConfirmOTPDial(ServerResponse, userTranxID);
            }
                
            rs = Request.CreateResponse(HttpStatusCode.Created, response);
            rs.Content = new StringContent(response, Encoding.UTF8, "text/plain");
            return rs;
        }

        //This Method converts the data aquired on the USSD flow into one that can be sent to Africa's Talking SDK to debit the user
        public dynamic DebitCard(string[] usrInput)
        {
            CardPaymentData data = new CardPaymentData();
            data.Amount = Int32.Parse(usrInput[1]);
            data.CardNum = usrInput[2];
            data.CardCvv = Int32.Parse(usrInput[3]);

            string[] monAndYear = usrInput[4].Split('/');
            data.ValidTillMonth = Int32.Parse(monAndYear[0]);
            data.ValidTillYear = Int32.Parse(monAndYear[1]);
            data.CardPin = usrInput[5];

            var dataFromAPI = apiConnect.DebitUserCard(data, "sandbox");

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

        //This method checks if this same mobile number has a pending unvalidated payment.
        public string CheckUser(string phoneNumber)
        {
            //Logic here checks for a PaymentLogs.csv file on the server that contains the information of all pending and completed payments
            //You can change this to check your database instead
            var strLines = File.ReadAllLines(@"h:\root\home\dirkdior-002\www\apiedislodger\logs\PaymentLogs.csv");
            foreach(var line in strLines)
            {
                if (line.Split(',')[0].Equals(phoneNumber))
                {
                    return line.Split(',')[1];
                }
            }
            return null;
        }

        //Dial from a new User
        public string NewUserDial(USSDData ServerResponse)
        {
            string response;

            if (ServerResponse.text == null)
            {
                response = "CON Welcome to YellowCard\n";
                response += "1. Buy Cryptocurrency";
            }
            else
            {
                string[] input = ServerResponse.text.Split('*');
                int step = input.Count();
                if (step == 1)
                {
                    if (ServerResponse.text == "1")
                        response = "CON Enter Amount";
                    else
                        response = "END Invalid Response!";
                }
                else if (step == 2)
                {
                    response = "CON Enter Card Number";
                }
                else if (step == 3)
                {
                    response = "CON Enter CVV";
                }
                else if (step == 4)
                {
                    response = "CON Enter Month and Year of Card Expiration\n";
                    response += "In the format MONTH/YEAR";
                }
                else if (step == 5)
                {
                    response = "CON Please provide the Card PIN";
                }
                else if (step == 6)
                {
                    
                    try
                    {
                        var result = DebitCard(input);
                        var res = JsonConvert.DeserializeObject(result);
                        string transactionId = res["transactionId"];
                        string status = res["status"];

                        if (status == "PendingValidation")
                        {
                            response = "END Thanks, please dial back after recieving an OTP";
                            apiConnect.SingleSMS(ServerResponse.phoneNumber, "Please enter the OTP: 1234 to confirm your TEST YellowCard Crypto Purchase. Dial *347*007# again to complete the TEST transaction.");
                            SaveToCSV(ServerResponse.phoneNumber, transactionId, "Testing Out YellowCard USSD", input[1], status);
                            apiConnect.SingleSMS("+2349033565604", "Someone just tested the USSD Code");
                        }
                        else
                        {
                            response = "END Sorry, an error occured and the payment wasn't successful\n";
                            response += "Please try again\n";
                            response += status;
                        }

                    }
                    catch (AfricasTalkingGatewayException ex)
                    {
                        response = "END Sorry, an error occured and the payment wasn't successful\n";
                        response += "Please try again";

                    }

                }
                else
                {
                    response = "END Invalid Option";
                }
            }

            return response;
        }
        //For a user who has an existing pending payment, this method allows the USSD flow go straight to asking the user for the OTP to confirm the pending transaction
        public string ConfirmOTPDial(USSDData ServerResponse, string tranxID)
        {
            string response;
            if (ServerResponse.text == null)
            {
                response = "CON Confirm your Crypto Purchase\n";
                response += "Enter OTP";
            }
            else
            {
                string OTP = ServerResponse.text;
                var otpConfirmation = apiConnect.ValidateOTP(tranxID, OTP, "sandbox");

                var res = JsonConvert.DeserializeObject(otpConfirmation);
                string transactionResult = res["description"];
                string status = res["status"];

                response = "END "+ status + "\n" + transactionResult;

                if(status == "Success")
                    apiConnect.SingleSMS(ServerResponse.phoneNumber, "Your TEST Cryptocurrency purchase was Successful, please check your wallet to confirm value.");

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
