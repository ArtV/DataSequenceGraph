using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class EdgeRouteSpec
    {
        public int FromNumber { get; set; }
        public int ToNumber { get; set; }
        public int RequisiteFromNumber { get; set; }
        public int RequisiteToNumber { get; set; }

        public EdgeRouteSpec()
        {
            RequisiteFromNumber = -1;
            RequisiteToNumber = -1;
        }
    }
}
