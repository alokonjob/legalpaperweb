using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Address;
using AspNetCore.Identity.Mongo.Model;
using Fundamentals.DbContext;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
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
        Task SaveAddress(Clientele endUser, UserAddress address);
    }
    public class ClienteleServices : IClienteleServices
    {
        private readonly IClienteleRepository userRepository;

        public ClienteleServices(IClienteleRepository userRepository)
        {
            this.userRepository = userRepository;
        }
        public async Task SaveAddress(Clientele endUser , UserAddress address)
        {
            if (endUser == null || address == null || endUser.ValidateBasicClienteleInfo() == false)
            {
                throw new Exception("Invalid User or Address");
            }

            await userRepository.AddAddress(endUser, address);

        }
    }

    public interface IClienteleRepository
    {
        Task AddAddress(Clientele user, UserAddress address);
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
    }
}
