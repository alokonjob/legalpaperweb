using Asgard;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Verify.V2.Service;

namespace Messaging
{
    public class MessagingService : IMessaging
    {
        private readonly IHeimdall gateKeeper;
        public MessagingService(IHeimdall gateKeeper)
        {
            this.gateKeeper = gateKeeper;
        }
        public static void Init(IHeimdall gateKeeper)
        {
            TwilioClient.Init(gateKeeper.GetSecretValue("TwilioAccountSID"), gateKeeper.GetSecretValue("TwilioAuthToken"));
        }

        
    }
}
