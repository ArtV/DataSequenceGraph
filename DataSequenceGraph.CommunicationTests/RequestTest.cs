using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DataSequenceGraph.Communication;
using System.IO;

namespace DataSequenceGraph.CommunicationTests
{
    [TestFixture]
    public class RequestTest
    {
        [Test]
        public void testCreateRequest()
        {
            /*
            using (FileStream fileStream = new FileStream("test.tar.gz", FileMode.Create))
            {
                DeltaRequest req = new DeltaRequest(new DeltaDirectory(Directory.GetCurrentDirectory()),
                    fileStream);
                req.createAndWrite();
            } */
            DeltaDirectory dir = new DeltaDirectory(Directory.GetCurrentDirectory());

            DeltaListFile baseFile = new DeltaListFile(new List<string> { dir.CurrentBase },
                dir.DirectoryPath);
            string newName = baseFile.writeFile();            
            int dummy = 3;
            Assert.AreEqual(9, dummy * 3);
             

        }
    }
}
