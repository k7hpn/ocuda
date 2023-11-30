using System;

namespace Ocuda.Ops.Models.Abstract
{
    public abstract class PortableBase
    {
        public DateTime ExportedAt { get; set; }
        public string ExportedBy { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Version { get; set; }
    }
}