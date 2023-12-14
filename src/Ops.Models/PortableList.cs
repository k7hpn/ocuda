using System.Collections.Generic;
using Ocuda.Ops.Models.Abstract;

namespace Ocuda.Ops.Models
{
    public class PortableList<T> : PortableBase
    {
        public IList<T> Items { get; set; }
    }
}