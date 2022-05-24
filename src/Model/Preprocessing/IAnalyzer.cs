using Common.Documents;
using System.Collections.Generic;

namespace Model.Preprocessing
{
    public interface IAnalyzer
    {
        /// <summary>
        /// Preprocess a document into a list of terms
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public List<string> Preprocess(ADocument doc);

        /// <summary>
        /// Tokenize a string useing the same settings as the Analyzer
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public List<string> Tokenize(string s);
    }
}
