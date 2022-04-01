using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Preprocessing
{
    public class AnalyzerConfig
    {
        public AnalyzerConfig() { }

        public AnalyzerConfig(bool performStemming, bool removeAccents, bool lowercase)
        {
            PerformStemming = performStemming;
            RemoveAccents = removeAccents;
            Lowercase = lowercase;
        }

        public bool RemoveAccents { get; set; } = true;
        public bool Lowercase { get; set; } = true;
        public bool PerformStemming { get; set; } = true;
    }
}
