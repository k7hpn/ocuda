using System.Collections.Generic;
using Ocuda.Ops.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.Programs.ViewModels
{
    public class IndexViewModel : PaginateModel
    {
        public IndexViewModel()
        {
            LibraryPrograms = [];
            LocationNames = new Dictionary<int, string>();
        }

        public bool HasImportPermission { get; set; }
        public ICollection<LibraryProgram> LibraryPrograms { get; }
        public IDictionary<int, string> LocationNames { get; }
    }
}