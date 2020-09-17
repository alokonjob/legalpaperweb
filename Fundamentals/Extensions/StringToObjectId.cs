using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fundamentals.Extensions
{
    public static class StringToObjectId
    {
        public static ObjectId ToObjectId(this string Id)
        {
            return ObjectId.Parse(Id);
        }
    }
}
