using Microsoft.Extensions.Configuration;
using Twilio;

namespace SMSer
{
    public static class InitTwilio
    {
        public static void Init(IConfiguration Configuration)
        {
            var accountSid = Configuration["TwilioAccountSID"];
            var authToken = Configuration["TwilioAuthToken"];
            TwilioClient.Init(accountSid, authToken);
        }
    }

    public class TwilioVerifySettings
    {
        public string VerificationServiceSID { get; set; }
    }
}
