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
        public string Query { get; set; }
        public List<DocumentInfo> Documents { get; set; } = new();
        public EQueryType QueryType { get; set; }
        public string IndexName { get; set; }

        public string ToString()
        {
            return Query;
        }
    }
}
