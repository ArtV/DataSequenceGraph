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
        public DeltaDirectory deltaDirectory { get; set; }
        public Stream outStream { get; set; }

        public DeltaRequest(DeltaDirectory deltaDirectory, Stream outStream)
        {
            this.deltaDirectory = deltaDirectory;
            this.outStream = outStream;
        }

        public void createAndWrite()
        {
            TarArchive requestTar = TarArchive.CreateOutputTarArchive(new GZipOutputStream(outStream));
            string curBase = deltaDirectory.CurrentBase;

            DeltaListFile baseFile = new DeltaListFile(new List<string> { curBase },
                deltaDirectory.DirectoryPath);
            string newName = baseFile.writeFile();

            requestTar.WriteEntry(TarEntry.CreateEntryFromFile(newName), false);

            requestTar.Close();

            File.Delete(newName);
        } 
    }
}
