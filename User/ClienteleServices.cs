using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
        Task<Clientele> GetUserAsync(ClaimsPrincipal User);
        bool IsSignedIn(ClaimsPrincipal User);
        Task<Result> CreateLogin(string fullName, string Email, string PhoneNumberCountryCode, string PhoneNumber, UserAddress Address, string password,string Role = null,Claim claimToAdd = null);
        Task<Result> CreateNewUserWithPassword(Clientele user,string password="");
        string GetEmailConfirmationCode(Clientele user);
        Task SendAccountConfirmEmailOnLoginCreation(string Email, string callBackUrl);
        Task SendStaffAccountConfirmEmailOnLoginCreation(string Email, string callBackUrl);
        Task SendNewPassword(string Email, string password);
        Task SaveAddress(Clientele endUser, UserAddress address);
        Task<List<Clientele>> GetUserByIds(List<ObjectId> UserIds);
        Task<Clientele> GetByEmail(string Email);
        Task<IList<string>> GetRoles(Clientele user);
        List<IdentityUserClaim<string>> GetClaims(Clientele user);
    }

    public interface IClienteleStaffServices
    {
        Task<IList<Clientele>> GetUserByRoles(string Role);
        Task<List<Clientele>> GetUserByClaims(string claim);
        Clientele FindAvailableCaseManager();

    }


    public class ClienteleServices : IClienteleServices, IClienteleStaffServices
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

        public async Task<Result> CreateLogin(string fullName, string Email, string PhoneNumberCountryCode, string PhoneNumber, UserAddress Address, string password, string Role = null, Claim claimToAdd = null)
        {
            var phoneNumber = phoneService.ExtractPhoneNumber(PhoneNumberCountryCode, PhoneNumber).Result;
            var user = new Clientele { FullName = fullName, Email = Email,UserName = Email, IsActive = true, PhoneNumber = phoneNumber,Addresses = new List<UserAddress>() { Address} };
            //user.Roles = new List<string>() { Role };
            user.Claims = new List<IdentityUserClaim<string>>() { new IdentityUserClaim<string>() { ClaimType = claimToAdd.Type, ClaimValue = claimToAdd.Value } };
            var existingUserByPhone = userManager.Users.Where(item => item.PhoneNumber == user.PhoneNumber).FirstOrDefault();
            if (existingUserByPhone != null)
            {
                throw new Exception($"Phone Number {user.PhoneNumber} Is Already Taken");
            }

            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {

                var addRolesResult = await userManager.AddToRoleAsync(user, Role);
                
                if (addRolesResult.Succeeded)
                {
                    return new Result(ResultValue.Success, ErrorCode.None, $"Successfully Created with Roles") { SomeGuy = user };
                }
                return new Result(ResultValue.Success, ErrorCode.None, $"Successfully Created without Roles") { SomeGuy = user };
            }
            else
            {
                //Add all the errors in ModelState Error
                return new Result(ResultValue.ErrorAndFatal, ErrorCode.UserCreationFailed, result.Errors.Select(x=> x.Description).ToList());
            }

        }

        public async Task<Result> CreateNewUserWithPassword(Clientele user,string password = "")
        {
            password = string.IsNullOrEmpty(password)?  PasswordGenerator.GenerateRandomPassword() : password;
            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {

                if (result.Succeeded)
                {
                    return new Result(ResultValue.Success, ErrorCode.None, $"Successfully Created with Password") { SomeGuy = user };
                }
                return new Result(ResultValue.Success, ErrorCode.None, $"Successfully Created without Roles") { SomeGuy = user };
            }
            else
            {
                //Add all the errors in ModelState Error
                return new Result(ResultValue.ErrorAndFatal, ErrorCode.UserCreationFailed, result.Errors.Select(x => x.Description).ToList());
            }
        }

        public string GetEmailConfirmationCode(Clientele user)
        { 
            return userManager.GenerateEmailConfirmationTokenAsync(user).Result;
        }
        public async Task SendAccountConfirmEmailOnLoginCreation(string Email , string callBackUrl)
        {
            var callbackUrl = callBackUrl;
            
            await emailSender.SendEmailAsync(Email, "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
        }

        public async Task SendStaffAccountConfirmEmailOnLoginCreation(string Email, string callBackUrl)
        {
            var callbackUrl = callBackUrl;

            await emailSender.SendEmailAsync(Email, "Your account is created as a Staff Member.Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
        }

        public async Task SendNewPassword(string Email, string password)
        {
            await emailSender.SendEmailAsync(Email, "Your password",
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

        public async Task<Clientele> GetUserAsync(ClaimsPrincipal User)
        {
            return await userManager.GetUserAsync(User);
        }

        public async Task<IList<Clientele>> GetUserByRoles(string Role)
        {
            return await userManager.GetUsersInRoleAsync(Role);// (userRepository as IClienteleStaffRepository).GetUserByRoles(Role);
        }

        public async Task<List<Clientele>> GetUserByClaims(string claim)
        {
            return await (userRepository as IClienteleStaffRepository).GetUserByClaims(claim);
        }

        public Clientele FindAvailableCaseManager()
        {
            return GetUserByRoles("CaseManager").Result.FirstOrDefault();
            //var consultants = await (userRepository as IClienteleStaffRepository).GetUserByRoles("CaseManager");
            //return consultants.Where(x=>x.Email == "aloksingh.itbhu@gmail.com").FirstOrDefault();
        }

        public bool IsSignedIn(ClaimsPrincipal User)
        {
            return signInManager.IsSignedIn(User);
        }

        public async Task<IList<string>> GetRoles(Clientele user)
        {
            return await userManager.GetRolesAsync(user);
        }

        public List<IdentityUserClaim<string>> GetClaims(Clientele user)
        {
            return user.Claims;
        }
    }

    public interface IClienteleStaffRepository
    {
        Task<List<Clientele>> GetUserByRoles(string Role);
        Task<List<Clientele>> GetUserByClaims(string claim);
    }

    public interface IClienteleRepository
    {
        Task<Clientele> GetUserByEmail(string Email);
        Task AddAddress(Clientele user, UserAddress address);
        Task<List<Clientele>> GetUserByIds(List<ObjectId> UserIds);

    }
    public class ClienteleRepository : IClienteleRepository, IClienteleStaffRepository
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

        public async Task<List<Clientele>> GetUserByRoles(string Role)
        {
            try
            {
                
                var findUser = await _usersCollection.FindAsync<Clientele>(x=>x.Roles.Any(x=>x== Role));
                return await findUser.ToListAsync();
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
                return null;
            }
        }

        public async Task<List<Clientele>> GetUserByClaims(string claim)
        {
            try
            {

                var findUser = await _usersCollection.FindAsync<Clientele>((x => x.Claims.Any(x => x.ClaimValue == claim)));
                return await findUser.ToListAsync();
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
                return null;
            }
        }
    }
}
