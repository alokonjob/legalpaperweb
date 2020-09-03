using System;
using System.Collections.Generic;
using Address;
using AspNetCore.Identity.Mongo.Model;
namespace Users
{
    public class Clientele : MongoUser
    {
        public string FullName { get; set; }
        public List<UserAddress> Addresses { get; set; }
        public bool IsActive { get; set; }

    }

    public static class ClienteleExtension
    {
        /// <summary>
        /// Validates the most primary informations like
        /// *Email 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static bool ValidateBasicClienteleInfo(this Clientele user)
        {
            if (string.IsNullOrEmpty(user.Email)) return false;
            return true;
        }
    }
}
