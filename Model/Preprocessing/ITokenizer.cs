using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Preprocessing
{
    public interface ITokenizer
    {
        public List<string> Tokenize(string text);
    }
}
