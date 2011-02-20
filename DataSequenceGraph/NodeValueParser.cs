using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph.Format
{
    public interface NodeValueParser<out NodeValType>
    {
        NodeValType parseToValue(string stringExportedValue);
    }
}
