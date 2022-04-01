using Common.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Preprocessing
{
    public interface IAnalyzer
    {
        public List<string> Preprocess(IDocument doc);
    }
}
