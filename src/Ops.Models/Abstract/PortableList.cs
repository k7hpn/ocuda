using System.Collections.Generic;

namespace Ocuda.Ops.Models.Abstract
{
    public abstract class PortableList<T> : PortableBase
    {
        internal PortableList()
        {
            Items = new List<T>();
        }

        public IList<T> Items { get; }
    }
}