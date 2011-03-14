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
        private DeltaDirectory deltaDir;

        [SetUp]
        public void SetUp()
        {
            deltaDir = new DeltaDirectory(Directory.GetCurrentDirectory());
        }

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

//            string newFile = deltaDir.DirectoryPath + @"\" + deltaDir.CurrentBase + ".base";
            MemoryStream resStream = new MemoryStream();
            byte[] resBytes = new byte[0];
            using (TextWriter textWriter = new StreamWriter(resStream))
            {
                DeltaRequest.createAndWrite(deltaDir, textWriter);
                resBytes = resStream.ToArray();
            }
            Assert.AreNotEqual(0, resBytes.Length);
            StreamReader resRdr = new StreamReader(new MemoryStream(resBytes));
            string[] deltArr = DeltaList.readList(resRdr);
            Assert.AreEqual(4, deltArr.Length);            
        }

        [Test]
        public void testFutureBaseRequest()
        {
            StringReader rdr = new StringReader("30-2011-03-13T05-38-00Z");
            ResultKind result = DeltaRequestHandler.handleDeltaRequest(deltaDir, rdr, new MemoryStream());
            Assert.AreEqual(ResultKind.Empty, result);
        }

        [Test]
        public void testMismatchRequest()
        {
            StringReader rdr = new StringReader("25-2011-03-13T05-33-00Z");
            MemoryStream resStream = new MemoryStream();
            ResultKind result = DeltaRequestHandler.handleDeltaRequest(deltaDir, rdr, resStream);
            StreamReader resRdr = new StreamReader(new MemoryStream(resStream.ToArray()));
            Assert.AreNotEqual(0, resStream.Length);
            string[] deltArr = DeltaList.readList(resRdr);
            Assert.AreEqual(ResultKind.Mismatch, result);
            Assert.AreEqual(2, deltArr.Length);
        }

        [Test]
        public void testHaveOldDeltas()
        {
            StringReader rdr = new StringReader("23-2011-03-13T05-33-55Z");
            ResultKind result = DeltaRequestHandler.handleDeltaRequest(deltaDir, rdr, new MemoryStream());
            Assert.AreEqual(ResultKind.Deltas, result);
        }
    }
}
