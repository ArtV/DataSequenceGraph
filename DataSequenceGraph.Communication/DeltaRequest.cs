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
        public static void createAndWrite(DeltaDirectory deltaDirectory, TextWriter outWriter)
        { 
            IEnumerable<string> fiveDeltas = deltaDirectory.getDeltasBeforeOrEqual(
                deltaDirectory.CurrentBase, 5);
            DeltaList.writeList(fiveDeltas, outWriter);            
        } 
    }
}
