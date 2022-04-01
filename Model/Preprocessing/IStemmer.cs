using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Preprocessing
{
    public interface IStemmer
    {
        public List<string> StemTokens(List<string> tokens);
    }
}
