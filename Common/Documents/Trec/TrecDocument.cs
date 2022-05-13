using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Documents.Trec
{
    public class TrecDocument : IDocument
    {
        public string Title { get; set; }
        public string Text { get; set; }
        public string Id { get; set; }
        public string Date { get; set; }

        public string GetRelevantText()
        {
            return Title + " " + Text;
        }
    }
}
