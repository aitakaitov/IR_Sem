using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Controller.Database.Entities
{
    public class DocumentInfo : BaseEntity
    {
        public int DocumentId { get; set; }
        public int OrderInQuery { get; set; }
    }
}
