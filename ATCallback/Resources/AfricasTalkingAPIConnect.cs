using AfricasTalkingCS;
using ATCallback.Models;
using ATCallback.Config;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AT_WebTests.Resources
{
    public class AfricasTalkingAPIConnect
    {
        string messageFromAPI;

        ATCallbackConfig config = new ATCallbackConfig();

        public string SingleSMS(string recipient, string messageToSend)
        {
            AfricasTalkingGatewayFile gatewayFile = new AfricasTalkingGatewayFile(config.sandboxUsername, config.sandboxApiKey, config.environment);
            try
            {
                dynamic results = gatewayFile.sendMessage(recipient, messageToSend, config.senderID);

                
                messageFromAPI = "Message Successfully Sent to " + recipient + "from " + config.senderID;
                return messageFromAPI;
            }
            catch (AfricasTalkingGatewayException e)
            {
                messageFromAPI = "Encountered an error: " + e.Message;
                return messageFromAPI;
            }
        }


        public string BuyAirtime(string phoneNumber, int amount)
        {

            AfricasTalkingGatewayFile gatewayFile = new AfricasTalkingGatewayFile(config.sandboxUsername, config.sandboxApiKey, config.environment);
            ArrayList reciepientList = new ArrayList();

            Hashtable rec1 = new Hashtable();
            rec1["phoneNumber"] = phoneNumber;
            rec1["amount"] = "NGN " + amount.ToString();

            reciepientList.Add(rec1);
            try
            {
                dynamic response = gatewayFile.sendAirtime(reciepientList);

                messageFromAPI = "Airtime Sent";
                return messageFromAPI;
            }
            catch(AfricasTalkingGatewayException ex)
            {
                messageFromAPI = "Airtime failed because: " + ex.Message;

                return messageFromAPI;
            }

        }

        public string BuyAirtimeLive(string phoneNumber, int amount)
        {

            AfricasTalkingGatewayFile gatewayFile = new AfricasTalkingGatewayFile(config.liveUsername, config.liveApiKey, "production");
            ArrayList reciepientList = new ArrayList();

            Hashtable rec1 = new Hashtable();
            rec1["phoneNumber"] = phoneNumber;
            rec1["amount"] = "NGN " + amount.ToString();

            reciepientList.Add(rec1);
            try
            {
                dynamic response = gatewayFile.sendAirtime(reciepientList);

                messageFromAPI = "Airtime Sent";
                return messageFromAPI;
            }
            catch (AfricasTalkingGatewayException ex)
            {
                messageFromAPI = "Airtime failed because: " + ex.Message;

                return messageFromAPI;
            }

        }

        public dynamic DebitUserCard(CardPaymentData data, string state)
        {

            if(state == "live")
            {
                AfricasTalkingGateway gateway = new AfricasTalkingGateway(config.liveUsername, config.liveApiKey, "production");
                string ProductName = config.livePaymentProduct;
                data.CurrencyCode = "NGN";
                data.CountryCode = "NG";
                data.Narration = "Testing Out LSW USSD";

                var cardDetails = new PaymentCard(data.CardPin, data.CountryCode, (short)data.CardCvv, data.ValidTillMonth, data.ValidTillYear, data.CardNum);

                var checkout = gateway.CardCheckout(
                    ProductName,
                    cardDetails,
                    data.CurrencyCode,
                    data.Amount,
                    data.Narration);

                return checkout;
            }
            else
            {
                AfricasTalkingGateway gateway = new AfricasTalkingGateway(config.sandboxUsername, config.sandboxApiKey, config.environment);
                string ProductName = config.sandboxPaymentProduct;
                data.CurrencyCode = "NGN";
                data.CountryCode = "NG";
                data.Narration = "Testing Out YellowCard USSD";

                var cardDetails = new PaymentCard(data.CardPin, data.CountryCode, (short)data.CardCvv, data.ValidTillMonth, data.ValidTillYear, data.CardNum);

                var checkout = gateway.CardCheckout(
                    ProductName,
                    cardDetails,
                    data.CurrencyCode,
                    data.Amount,
                    data.Narration);

                return checkout;
            }

        }

        public dynamic ValidateOTP(string transactionID, string OTP, string state)
        {
            if(state == "live")
            {
                AfricasTalkingGateway gateway = new AfricasTalkingGateway(config.liveUsername, config.liveApiKey, "production");

                var validate = gateway.ValidateCardOtp(transactionID, OTP);

                return validate;
            }
            else
            {
                AfricasTalkingGateway gateway = new AfricasTalkingGateway(config.sandboxUsername, config.sandboxApiKey, config.environment);

                var validate = gateway.ValidateCardOtp(transactionID, OTP);

                return validate;
            }

            
        }

        public dynamic MakeACall(string from, string to)
        {
            var gateway = new AfricasTalkingGateway(config.sandboxUsername, config.sandboxApiKey, config.environment);
            try
            {
                var results = gateway.Call(from, to);
                var res = JsonConvert.DeserializeObject(results);
                var status = res["entries"];
                return status;
            }
            catch (AfricasTalkingGatewayException ex)
            {
                return "";
            }
        }

    }
}