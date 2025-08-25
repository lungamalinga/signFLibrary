using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SigniFlowMiddlewareLibrary.Models
{
    public class Employees
    {
        public bool success { get; set; }
        public object[] businessRules { get; set; }
        public object[] data { get; set; }
    }

    public class Root
    {
        public Employees employees { get; set; }
    }
}
