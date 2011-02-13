using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph.Format
{
    public class StringNodeValueParser : NodeValueParser<string>
    {
        public string parseToValue(string stringExportedValue)
        {
            return stringExportedValue;
        }
    }
}
