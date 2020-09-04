using System.Threading.Tasks;

namespace Phone
{
    public interface IPhoneService
    {
        Task<string> ExtractPhoneNumber(string counrtryCode, string phoneNumber);
        Task<PhoneNumber> IsPhoneVerifiedAsync(string phoneNumber, string verificationCode);
        Task<PhoneNumber> GetPhoneNumberDetails(string PhoneNumber, string PhoneNumberCountryCode);
        Task<PhoneNumber> PhoneVerificationStatus(string PhoneNumber);
    }
}