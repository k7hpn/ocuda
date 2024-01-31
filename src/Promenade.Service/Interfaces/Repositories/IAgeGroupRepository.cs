using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface IAgeGroupRepository
    {
        public Task<ICollection<AgeGroup>> GetAgeGroupsAsync(IEnumerable<int> ageGroupIds);
    }
}