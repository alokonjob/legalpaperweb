using System.Threading.Tasks;

namespace Phone
{
    public interface IPhoneService
    {
        Task<string> ExtractPhoneNumber(string counrtryCode, string phoneNumber);
    }
}