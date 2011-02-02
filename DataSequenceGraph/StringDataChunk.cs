using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph.DataChunk
{
    public class StringDataChunk : DataChunk<string>
    {
        private IEnumerable<string> _sourceData;

        public StringDataChunk(IEnumerable<string> sourceData)
        {
            this._sourceData = sourceData;
        }

        public IEnumerable<string> sourceData
        {
            get { return this._sourceData; }
        }
    }
}
