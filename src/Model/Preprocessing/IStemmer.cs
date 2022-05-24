using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
