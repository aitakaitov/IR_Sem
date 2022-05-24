using System.Collections.Generic;

namespace Model.Preprocessing
{
    public interface ITokenizer
    {
        /// <summary>
        /// Tokenizes text
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public List<string> Tokenize(string text);
    }
}
