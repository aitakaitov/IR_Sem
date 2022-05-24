using System.Collections.Generic;

namespace Model.Preprocessing
{
    public interface IStemmer
    {
        /// <summary>
        /// Perform stemming over a list of tokens
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public List<string> StemTokens(List<string> tokens);
    }
}
