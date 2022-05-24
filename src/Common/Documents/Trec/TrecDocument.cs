using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Documents.Trec
{
#nullable disable
    /// <summary>
    /// TREC evaluation document
    /// For JSON parsing
    /// </summary>
    public class TrecDocument : ADocument
    {
        public string Title { get; set; }
        public string Text { get; set; }
        public string Id { get; set; }
        public string Date { get; set; }

        public override string GetRelevantText()
        {
            return Title + " " + Text;
        }
    }
}
