using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.GZip;

namespace DataSequenceGraph.Communication
{
    public class DeltaRequest
    {
        private DeltaDirectory deltaDirectory { get; set; }

        public DeltaRequest(DeltaDirectory deltaDirectory)
        {
            this.deltaDirectory = deltaDirectory;
        }

        public void writeDefaultRequest(TextWriter outWriter)
        {
            writeRequestUsingDelta(deltaDirectory.CurrentBase, outWriter);
        }

        public void writeRequestUsingDelta(string oldDelta, TextWriter outWriter)
        {
            IEnumerable<string> fiveDeltas = deltaDirectory.getDeltasBeforeOrEqual(
                oldDelta, 5);
            DeltaList.writeList(fiveDeltas, outWriter);            
        }
    }
}
