using Asgard;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Lookups.V1;
using Twilio.Rest.Verify.V2.Service;

namespace Phone
{
    public class PhoneNumber
    { 
        public string Type { get; set; }
        public string DialNumber { get; set; }
        public bool IsSMSCapable { get; set; }
        public string VerificationStatusText { get; set; }
        public bool IsVerified => VerificationStatusText == "approved";

    }
    public class PhoneService : IPhoneService
    {
        private readonly IHeimdall gateKeeper;

        public PhoneService(IHeimdall gateKeeper)
        {
            this.gateKeeper = gateKeeper;
        }
        public async Task<string> ExtractPhoneNumber(string counrtryCode, string phoneNumber)
        {
            var numberDetails = await PhoneNumberResource.FetchAsync(
            pathPhoneNumber: new Twilio.Types.PhoneNumber(phoneNumber),
            countryCode: counrtryCode,
            type: new List<string> { "carrier" });
            if (numberDetails?.Carrier != null
                        && numberDetails.Carrier.TryGetValue("type", out var phoneNumberType)
                        && phoneNumberType == "landline")
            {

                throw new Exception($"The number you entered does not appear to be capable of receiving SMS ({phoneNumberType}). Please enter a different value and try again");
            }

            return numberDetails.PhoneNumber.ToString();
        }

        public async Task<PhoneNumber> IsPhoneVerifiedAsync(string phoneNumber, string verificationCode)
        {
            PhoneNumber phoneNumberDetails = new PhoneNumber() { DialNumber = phoneNumber };
            var verification = await VerificationCheckResource.CreateAsync(
                    to: phoneNumber,
                    code: verificationCode,
                    pathServiceSid: "VAb9da8bbdb3e209c82df8e40e644b1e37"//gateKeeper.GetSecretValue("TwilioVerificationServiceSID")
                );
            phoneNumberDetails.VerificationStatusText = verification.Status;
            return phoneNumberDetails;
        }


        public async Task<PhoneNumber> PhoneVerificationStatus(string PhoneNumber)
        {
            PhoneNumber phoneDetails = new PhoneNumber() { DialNumber = PhoneNumber};
            var verification = await VerificationResource.CreateAsync(
                    to: PhoneNumber,
                    channel: "sms",
                    pathServiceSid: "VAb9da8bbdb3e209c82df8e40e644b1e37"//gateKeeper.GetSecretValue("TwilioVerificationServiceSID")
                );
             phoneDetails.VerificationStatusText = verification.Status;
            return phoneDetails;
        }


        public async Task<PhoneNumber> GetPhoneNumberDetails(string PhoneNumber, string PhoneNumberCountryCode)
        {
            PhoneNumber phoneNumber = new PhoneNumber() { DialNumber = PhoneNumber };
            
            var numberDetails = await PhoneNumberResource.FetchAsync(
            pathPhoneNumber: new Twilio.Types.PhoneNumber(PhoneNumber),
            countryCode: PhoneNumberCountryCode,
            type: new List<string> { "carrier" });

            phoneNumber.DialNumber = numberDetails.PhoneNumber.ToString();
            phoneNumber.Type = GetPhoneNumberType(numberDetails);
            phoneNumber.IsSMSCapable = phoneNumber.Type == "landline" ? false : true;

            return phoneNumber;

        }


        private string GetPhoneNumberType(PhoneNumberResource numberDetails)
        {
            string phoneNumberType = "";
            if (numberDetails?.Carrier != null)
            {
                numberDetails.Carrier.TryGetValue("type", out phoneNumberType);
            }
            return phoneNumberType;
        }

    }
}
