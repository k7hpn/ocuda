using System.Collections.Generic;

namespace Ocuda.Ops.Models
{
    public class DataImportResult
    {
        public DataImportResult()
        {
            Issues = new List<string>();
        }

        public IList<string> Issues { get; }
        public int TotalItems { get; set; }
    }
}