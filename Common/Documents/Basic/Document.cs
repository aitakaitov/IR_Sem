using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Documents.Basic
{
    public class Document : IDocument
    {
        public string Text { get; set; }

        public string GetRelevantText() { return Text; }
    }
}
