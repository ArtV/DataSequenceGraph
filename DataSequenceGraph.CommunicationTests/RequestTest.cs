using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DataSequenceGraph.Communication;
using System.IO;
using DataSequenceGraph;
using DataSequenceGraph.Format;

namespace DataSequenceGraph.CommunicationTests
{
    [TestFixture]
    public class RequestTest
    {
        private DeltaDirectory deltaDir;

        [SetUp]
        public void SetUp()
        {
            string[] testProducedFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "000*.*");
            foreach (string testFile in testProducedFiles)
            {
                File.Delete(testFile);
            }
            deltaDir = new DeltaDirectory(Directory.GetCurrentDirectory());
        }

        [Test]
        public void testCreateRequest()
        {
            MemoryStream resStream = new MemoryStream();
            byte[] resBytes = new byte[0];
            using (TextWriter textWriter = new StreamWriter(resStream))
            {
                new DeltaRequest(deltaDir).writeDefaultRequest(textWriter);
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
            StringReader rdr = new StringReader("0030-delta-2011-03-13T05-38-00Z");
            DeltaRequestResultKind result = DeltaRequestHandler.handleDeltaRequest(deltaDir, new MasterNodeList<int>(), 
                new MasterNodeList<int>(), new ToStringNodeValueExporter<int>(),rdr, new MemoryStream());
            Assert.AreEqual(DeltaRequestResultKind.Empty, result);
        }

        [Test]
        public void testMismatchRequest()
        {
            StringReader rdr = new StringReader("0025-delta-2011-03-13T05-33-00Z");
            MemoryStream resStream = new MemoryStream();
            DeltaRequestResultKind result = DeltaRequestHandler.handleDeltaRequest(deltaDir, new MasterNodeList<int>(),
                new MasterNodeList<int>(), new ToStringNodeValueExporter<int>(), rdr, resStream);
            StreamReader resRdr = new StreamReader(new MemoryStream(resStream.ToArray()));
            Assert.AreNotEqual(0, resStream.Length);
            string[] deltArr = DeltaList.readList(resRdr);
            Assert.AreEqual(DeltaRequestResultKind.Mismatch, result);
            Assert.AreEqual(2, deltArr.Length);
        }

        [Test]
        public void testHaveOldDeltas()
        {
            StringReader rdr = new StringReader("0023-delta-2011-03-13T05-33-55Z");
            MemoryStream resStream = new MemoryStream();
            DeltaRequestResultKind result = DeltaRequestHandler.handleDeltaRequest(deltaDir, new MasterNodeList<int>(), 
                new MasterNodeList<int>(), new ToStringNodeValueExporter<int>(), rdr, resStream);
            Assert.AreEqual(DeltaRequestResultKind.Deltas, result);            
            using (FileStream fileStream = new FileStream("testOld.tar.gz", FileMode.Create))
            {
                new MemoryStream(resStream.ToArray()).CopyTo(fileStream);
            }  
        }
        
        [Test]
        public void produceAndSendNewDelta()
        {
            MasterNodeList<string> localNodeList = new MasterNodeList<string>();
            DataChunkRouteBlazerTest.threeThreeChunks(localNodeList, new Dictionary<Node, List<Route>>());

            StringReader rdr = new StringReader(DeltaDirectoryTest.CURBASE);
            MemoryStream resStream = new MemoryStream();
            DeltaRequestResultKind result = DeltaRequestHandler.handleDeltaRequest(deltaDir, new MasterNodeList<string>(),
                localNodeList, new ToStringNodeValueExporter<string>(), rdr, resStream);
            Assert.AreEqual(DeltaRequestResultKind.Deltas, result);
            using (FileStream fileStream = new FileStream("testNew.tar.gz", FileMode.Create))
            {
                new MemoryStream(resStream.ToArray()).CopyTo(fileStream);
            } 
        }  
    }
}
