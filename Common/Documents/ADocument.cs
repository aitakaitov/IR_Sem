using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Documents
{
    public abstract class ADocument
    {
        /// <summary>
        /// ID of the document, determined by the indexer
        /// </summary>
        public int Id { get; set; }

        public abstract string GetRelevantText();
    }
}
