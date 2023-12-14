using System.Collections.Generic;
using System.Diagnostics;

namespace Ocuda.Ops.Models.Portable
{
    public class ImportResult
    {
        public ImportResult()
        {
            Stopwatch = Stopwatch.StartNew();
            Issues = new List<string>();
        }
        public Stopwatch Stopwatch { get; }

        public int HeaderId { get; set; }
        public IList<string> Issues { get; }
        public int RecordsWithIssues { get; set; }
        public int TotalRecords { get; set; }
    }
}