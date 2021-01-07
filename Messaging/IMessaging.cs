using Asgard;

namespace Messaging
{
    public interface IMessaging
    {
        void SendSMS(string phoneNumber, string textSms);
    }
}