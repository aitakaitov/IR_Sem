using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Controller.Transfer
{
    public class CreateIndexRequest
    {
        public string Name { get; set; }
        public bool PerformStemming { get; set; }
        public bool Lowercase { get; set; }
        public bool RemoveAccents { get; set; }
        public string? StopwordsFilePath { get; set; }
        public string DocumentDirectoryPath { get; set; }
    }
}
