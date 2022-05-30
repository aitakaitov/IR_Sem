using Common.Documents;
using Model.Queries;
using System.Collections.Generic;

namespace Model.Indexing
{
    /// <summary>
    /// Generic index interface
    /// </summary>
    public abstract class AIndex
    {
        /// <summary>
        /// Returns a list of documents given a list of IDs
        /// Keeps the order
        /// </summary>
        /// <param name="documentIds"></param>
        /// <returns></returns>
        public abstract List<ADocument> GetDocumentsByIds(List<int> documentIds);

        /// <summary>
        /// Index a list of documents
        /// </summary>
        /// <param name="documents"></param>
        public abstract void Index(List<ADocument> documents);

        /// <summary>
        /// Perform boolean search - WIP
        /// </summary>
        public abstract (List<ADocument>, int) BooleanSearch(BasicQuery query);

        /// <summary>
        /// Perform vector-space search on a query
        /// </summary>
        /// <param name="query"></param>
        /// <returns>(top K IDocs, total count of relevant, document cosine similarities)</returns>
        public abstract (List<ADocument>, int, List<float>) VectorSpaceSearch(BasicQuery query);

        public abstract override string ToString();
    }
}
