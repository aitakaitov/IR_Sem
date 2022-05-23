using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Controller.Database.Entities
{
    public class DocumentInfo : BaseEntity
    {
        /// <summary>
        /// ID of the document
        /// </summary>
        public int DocumentId { get; set; }
        
        /// <summary>
        /// Position of the document in the query results
        /// </summary>
        public int OrderInQuery { get; set; }
    }
}
