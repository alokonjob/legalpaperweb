using Asgard;
using Fundamentals.Events;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Rest.Verify.V2.Service;
using Twilio.Types;

namespace Messaging
{
    public class MessagingService : IMessaging
    {
        private readonly IHeimdall gateKeeper;
        private readonly ILogger<MessagingService> logger;

        public MessagingService(IHeimdall gateKeeper, ILogger<MessagingService> logger)
        {
            this.gateKeeper = gateKeeper;
            this.logger = logger;
        }
        public static void Init(IHeimdall gateKeeper)
        {
            TwilioClient.Init(gateKeeper.GetSecretValue("TwilioAccountSID"), gateKeeper.GetSecretValue("TwilioAuthToken"));
        }

        public void SendSMS(string phoneNumber, string textSms)
        {
            try
            {
                var messageOptions = new CreateMessageOptions(
                        new PhoneNumber(phoneNumber));
                messageOptions.Body = textSms;
                messageOptions.From = new PhoneNumber("+13344906142");
                var message = MessageResource.Create(messageOptions);
                logger.LogInformation(LogEvents.SendSMSSuccess, $" to Phone xxxx-{phoneNumber.Substring(6)} with status {message.Status}");
            }
            catch (Exception error)
            {
                logger.LogCritical(LogEvents.SendSMSFailure, $" to Phone xxxx-{phoneNumber.Substring(6)} with error {error.Message}");
            }
        }


    }
}
