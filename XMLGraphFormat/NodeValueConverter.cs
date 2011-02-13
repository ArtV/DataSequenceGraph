using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph.Format
{
    public interface NodeValueConverter<in NodeValType>
    {
        string ToNodeValueString(NodeValType originalValue);
    }
}
