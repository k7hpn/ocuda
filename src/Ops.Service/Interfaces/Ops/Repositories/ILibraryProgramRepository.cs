using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface ILibraryProgramRepository : IOpsRepository<LibraryProgram, int>
    {
        public Task<CollectionWithCount<LibraryProgram>> PaginateAsync(BaseFilter filter);
    }
}