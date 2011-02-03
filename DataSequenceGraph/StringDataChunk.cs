using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace DataSequenceGraph.DataChunk
{
    public class StringDataChunk : DataChunk<string>
    {
        private IEnumerable<string> _sourceData;

        public StringDataChunk(IEnumerable<string> sourceData)
        {
            this._sourceData = sourceData;
        }

        public IEnumerator<string> GetEnumerator()
        {
            return _sourceData.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _sourceData.GetEnumerator();
        }
    }
}
