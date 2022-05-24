using Controller.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Controller.Database.Entities
{
    public class QueryResult : BaseEntity
    {
        public DateTime DateQueried { get; set; }

        /// <summary>
        /// Query string
        /// </summary>
        public string Query { get; set; } = "";

        /// <summary>
        /// List of documents the query returned
        /// </summary>
        public List<DocumentInfo> Documents { get; set; } = new();

        /// <summary>
        /// either boolean or vector
        /// </summary>
        public EQueryType QueryType { get; set; }

        /// <summary>
        /// Name of the index
        /// Indexes should have unique names in order for this to work properly
        /// </summary>
        public string IndexName { get; set; } = "";

        public override string ToString()
        {
            return Query;
        }
    }
}
