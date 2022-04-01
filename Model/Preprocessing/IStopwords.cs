using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Preprocessing
{
    public interface IStopwords
    {
        public void SetConfig(AnalyzerConfig config);
        public List<string> RemoveStopwords(List<string> tokens);
    }
}
