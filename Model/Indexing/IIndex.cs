using Common.Documents;
using Model.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Indexing
{
    /// <summary>
    /// Generic index interface
    /// </summary>
    public interface IIndex
    {
        public List<ADocument> GetDocumentsByIds(List<int> documentIds);

        /// <summary>
        /// Index a list of documents
        /// </summary>
        /// <param name="documents"></param>
        public void Index(List<ADocument> documents);

        /// <summary>
        /// Perform boolean search - WIP
        /// </summary>
        public (List<ADocument>, int) BooleanSearch(BasicQuery query);

        /// <summary>
        /// Perform vector-space search on a query
        /// </summary>
        /// <param name="query"></param>
        /// <returns>(top K IDocs, total count of relevant)</returns>
        public (List<ADocument>, int, List<float>) VectorSpaceSearch(BasicQuery query);
    }
}
