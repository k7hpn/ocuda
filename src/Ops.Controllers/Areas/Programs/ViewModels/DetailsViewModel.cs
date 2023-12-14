using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.Programs.ViewModels
{
    public class DetailsViewModel
    {
        public LibraryProgram LibraryProgram { get; set; }
        public ScheduledEvent ScheduledEvent { get; set; }
        [DisplayName("Is registration required?")]
        public string RegistrationChoice { get; set; }
    }
}
