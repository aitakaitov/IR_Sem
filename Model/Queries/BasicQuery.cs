using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Queries
{
    public class BasicQuery
    {
        public string QueryText { get; set; } = string.Empty;
        public int TopCount { get; set; } = 10;
    }
}
