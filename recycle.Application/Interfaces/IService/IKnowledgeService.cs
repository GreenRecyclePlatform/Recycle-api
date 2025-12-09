using recycle.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.Interfaces.IService
{
   
        namespace recycle.Application.Interfaces
    {
        public interface IKnowledgeService
        {
            Task<List<KnowledgeItem>> SearchRelevantInfo(string query);
            List<KnowledgeItem> GetAllKnowledge();
        }
    }

}

