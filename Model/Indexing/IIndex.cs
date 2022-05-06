﻿using Common.Documents;
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
        /// <summary>
        /// Index a list of documents
        /// </summary>
        /// <param name="documents"></param>
        public void Index(List<IDocument> documents);

        /// <summary>
        /// Perform boolean search - WIP
        /// </summary>
        public (List<IDocument>, int) BooleanSearch(BasicQuery query);

        /// <summary>
        /// Perform vector-space search on a query
        /// </summary>
        /// <param name="query"></param>
        /// <returns>(top K IDocs, total count of relevant)</returns>
        public (List<IDocument>, int) VectorSpaceSearch(BasicQuery query);
    }
}
