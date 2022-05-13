using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Documents.Trec
{
    public class Topic : IDocument
    {
        public string Id { get; set; }
        public string Narrative { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string Lang { get; set; }

        public string GetRelevantText()
        {
            return Narrative + " " + Title;
        }
    }
}
