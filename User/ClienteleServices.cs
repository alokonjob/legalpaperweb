using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Emailer;
using FundamentalAddress;
using Fundamentals;
using Fundamentals.DbContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using MongoDB.Bson;
using MongoDB.Driver;
using Phone;
using Users;

namespace User
{
    /// <summary>
    /// Most of the user identity related services are provided by UserManager class of Identity
    /// However we have added our own information to the model.
    /// Lets use below interface to deal with those fiels
    /// </summary>
    public interface IClienteleServices
    {
        Task<Result> CreateLogin(string fullName, string Email, string PhoneNumberCountryCode, string PhoneNumber, UserAddress Address, string password);
        string GetEmailConfirmationCode(Clientele user);
        void SendAccountConfirmEmailOnLoginCreation(string Email, string callBackUrl);
        void SendNewPassword(string Email, string password);
        Task SaveAddress(Clientele endUser, UserAddress address);
        Task<List<Clientele>> GetUserByIds(List<ObjectId> UserIds);
        Task<Clientele> GetByEmail(string Email);
    }
    public class ClienteleServices : IClienteleServices
    {
        private readonly IClienteleRepository userRepository;
        private readonly UserManager<Clientele> userManager;
        private readonly SignInManager<Clientele> signInManager;
        private readonly IEmailer emailSender;
        private readonly IPhoneService phoneService;

        public ClienteleServices(IClienteleRepository userRepository, UserManager<Clientele> userManager,
            SignInManager<Clientele> signInManager, IEmailer emailSender,IPhoneService phoneService)
        {
            this.userRepository = userRepository;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.emailSender = emailSender;
            this.phoneService = phoneService;
        }

        public async Task<Result> CreateLogin(string fullName, string Email, string PhoneNumberCountryCode, string PhoneNumber, UserAddress Address,string password)
        {
            var phoneNumber = phoneService.ExtractPhoneNumber(PhoneNumberCountryCode, PhoneNumber).Result;
            var user = new Clientele { FullName = fullName, Email = Email,UserName = Email, IsActive = true, PhoneNumber = phoneNumber,Addresses = new List<UserAddress>() { Address} };

            var existingUserByPhone = userManager.Users.Where(item => item.PhoneNumber == user.PhoneNumber).FirstOrDefault();
            if (existingUserByPhone != null)
            {
                throw new Exception($"Phone Number {user.PhoneNumber} Is Already Taken");
            }

            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                return new Result(ResultValue.Success, ErrorCode.None, $"Successfully Created") { SomeGuy = user };
            }
            else
            {
                //Add all the errors in ModelState Error
                return new Result(ResultValue.ErrorAndFatal, ErrorCode.UserCreationFailed, result.Errors.Select(x=> x.Description).ToList());
            }

        }

        public string GetEmailConfirmationCode(Clientele user)
        { 
            return userManager.GenerateEmailConfirmationTokenAsync(user).Result;
        }
        public void SendAccountConfirmEmailOnLoginCreation(string Email , string callBackUrl)
        {
            var callbackUrl = callBackUrl;
            
            emailSender.SendEmailAsync(Email, "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
        }

        public void SendNewPassword(string Email, string password)
        {
            emailSender.SendEmailAsync(Email, "Your password",
                $"Please Use the below password to login.<br/> {password} <br/> This is your confidential information, please dont share with others or our staff.");
        }
        public async Task SaveAddress(Clientele endUser , UserAddress address)
        {
            if (endUser == null || address == null || endUser.ValidateBasicClienteleInfo() == false)
            {
                throw new Exception("Invalid User or Address");
            }

            await userRepository.AddAddress(endUser, address);

        }

        public async Task<List<Clientele>> GetUserByIds(List<ObjectId> UserIds)
        {
            return await userRepository.GetUserByIds(UserIds);
        }

        public async Task<Clientele> GetByEmail(string Email)
        {
            return await userManager.FindByEmailAsync(Email);
        }
    }

    public interface IClienteleRepository
    {
        Task<Clientele> GetUserByEmail(string Email);
        Task AddAddress(Clientele user, UserAddress address);
        Task<List<Clientele>> GetUserByIds(List<ObjectId> UserIds);
    }
    public class ClienteleRepository : IClienteleRepository
    {
        IMongoClient client = null;
        private readonly IMongoCollection<Clientele> _usersCollection;
        private readonly IMongoDbContext mongoContext;

        public ClienteleRepository(IMongoDbContext mongoContext)
        {
            this.mongoContext = mongoContext;
            client = mongoContext.GetMongoClient();
            _usersCollection = mongoContext.GetCollection<Clientele>(client, "Users");
        }
        public async Task AddAddress(Clientele user,UserAddress address)
        {
            try
            {
                var findUser = _usersCollection.Find<Clientele>(x=>x.Email == user.Email).FirstOrDefault();
                var filter = Builders<Clientele>.Filter.Eq(t => t.Email, user.Email);
                var userWithSavedAddress = await _usersCollection.FindOneAndUpdateAsync<Clientele>(
                    filter,
                    Builders<Clientele>.Update.Set(
                        t => t.Addresses,
                        new List<UserAddress>() { address })
                    );
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
            }
        }

        public async Task<Clientele> GetUserByEmail(string Email)
        {
            try
            {
                var filter = Builders<Clientele>.Filter.Eq(t => t.Email, Email);
                var findUser = await _usersCollection.FindAsync<Clientele>(filter);
                return findUser.FirstOrDefaultAsync<Clientele>().Result;
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
                return null;
            }
        }

        public async Task<List<Clientele>> GetUserByIds(List<ObjectId> UserIds)
        {
            try
            {
                var filter = Builders<Clientele>.Filter.In(t => t.Id, UserIds);
                var findUser =  _usersCollection.Find(filter).ToListAsync();
                return  await findUser;
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
                return null;
            }
        }
    }
}
