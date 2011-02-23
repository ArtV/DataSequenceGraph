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
        public int[] RequisiteNumbers { get; set; }

        public EdgeRouteSpec()
        {
            RequisiteNumbers = new int[0];
        }
    }
}
