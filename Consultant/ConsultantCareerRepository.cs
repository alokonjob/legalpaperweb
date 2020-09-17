using Fundamentals.DbContext;
using Fundamentals.Unit;
using MongoDB.Driver;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System;

namespace Consultant
{
    public class ConsultantCareerRepository : IConsultantCareerRepository
    {
        IMongoClient client = null;
        private readonly IMongoCollection<ConsultantCareer> _consultantCareerCollection;
        private readonly IMongoCollection<ConsultantVerificationDetails> _consultantDocumentCollection;
        private readonly IMongoCollection<ConsultantTaxDetails> _consultantTaxCollection;
        public ConsultantCareerRepository(IMongoDbContext mongoContext)
        {
            client = mongoContext.GetMongoClient();
            _consultantCareerCollection = mongoContext.GetCollection<ConsultantCareer>(client, "ConsultantCareer");
            _consultantDocumentCollection = mongoContext.GetCollection<ConsultantVerificationDetails>(client, "ConsultantDocuments");
            _consultantTaxCollection = mongoContext.GetCollection<ConsultantTaxDetails>(client, "ConsultantTaxDetails");
        }
        public async Task<ConsultantCareer> AddConsultantCareer(ConsultantCareer consultantCareer)
        {
            await _consultantCareerCollection.InsertOneAsync(consultantCareer);
            return consultantCareer;
        }

        public async Task<ConsultantVerificationDetails> AddVerificationDetails(ConsultantVerificationDetails docs)
        {
            await _consultantDocumentCollection.InsertOneAsync(docs);
            return docs;
        }

        public async Task<ConsultantTaxDetails> AddTaxDetails(ConsultantTaxDetails docs)
        {
            await _consultantTaxCollection.InsertOneAsync(docs);
            return docs;
        }

        public async Task<List<ConsultantCareer>>   GetConsultantsForService(string enabledServiceId)
        {
            var cc = await _consultantCareerCollection.Find(x => x.ServicesOffered.Any(x => x.EnabledServiceId == enabledServiceId)).ToListAsync();
            var newcc = cc.Where(x=> x.ServicesOffered.Select(y => y.IsCurrent = (y.EnabledServiceId == enabledServiceId)).FirstOrDefault()).Select(z=> z.RatingsValue = Math.Round(z.Ratings.Sum()/z.Ratings.Count(),1)).ToList();

            return cc;
                // ---- OR -----------
                /*var filter = Builders<ConsultantCareer>.Filter.ElemMatch(x => x.ServicesOffered, y => y.EnabledServiceId == enabledServiceId);
                 * var res =  _consultantCareerCollection.Find(filter).ToListAsync();*/

            
        }
    }
}
