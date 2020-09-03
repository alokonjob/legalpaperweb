using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Twilio.Rest.Lookups.V1;

namespace Phone
{
    public class PhoneService : IPhoneService
    {
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

    }
}
