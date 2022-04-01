using Common.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Indexing
{
    public interface IIndex
    {
        public void Index(List<IDocument> documents);

        public void BooleanSearch();

        public void VectorSpaceSearch();
    }
}
