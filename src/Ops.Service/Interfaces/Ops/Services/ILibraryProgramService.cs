using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface ILibraryProgramService
    {
        public Task<LibraryProgram> GetAsync(int id);

        public Task<Ocuda.Ops.Models.Portable.ImportResult>
                    ImportAsync(int userId, string filename, bool performImport);

        public Task<CollectionWithCount<LibraryProgram>> PaginateAsync(BaseFilter filter);
    }
}