using Common.Documents;
using Model.Queries;
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

        public List<IDocument> VectorSpaceSearch(VectorQuery query);
    }
}
