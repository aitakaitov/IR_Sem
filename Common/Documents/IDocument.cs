using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Documents
{
    /// <summary>
    /// Generic document
    /// </summary>
    public interface IDocument
    {
        /// <summary>
        /// Returns all relevant text in the document
        /// </summary>
        /// <returns></returns>
        public string GetRelevantText();
    }
}
