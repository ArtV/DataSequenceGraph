using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph.Format
{
    public class ToStringNodeValueExporter<NodeValType> : NodeValueExporter<NodeValType>
    {
        public string ToNodeValueString(NodeValType originalValue)
        {
            return originalValue.ToString();
        }
    }
}
