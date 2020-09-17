﻿using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CaseManagementSpace
{
    public interface ICaseRepository
    {
        Task<ObjectId> AddCase(Case clientCase);
        Task<List<Case>> GetAll();
        Task<Case> GetCaseById(string caseId);
        Task<Case> UpdateConsultant(Case caseToUpdate);

    }
}