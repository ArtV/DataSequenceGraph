﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using DataSequenceGraph.Communication;
using DataSequenceGraph.Format;

namespace DataSequenceGraph.CommunicationTests
{
    [TestFixture]
    public class ResponseTest
    {
        private DeltaDirectory deltaDir;

        [SetUp]
        public void SetUp()
        {
            DeltaDirectoryTest.resetDeltaDir();
            deltaDir = new DeltaDirectory(Directory.GetCurrentDirectory());
        }

        [Test]
        public void testMismatchResponse()
        {
            StringReader rdr = new StringReader("0025-delta-2011-03-13T05-33-00Z");
            MemoryStream resStream = new MemoryStream();
            DeltaResponseResultKind result = DeltaResponseHandler.handleDeltaBaseResponse(deltaDir,
                rdr, new StreamWriter(resStream));
            StreamReader resRdr = new StreamReader(new MemoryStream(resStream.ToArray()));
            Assert.AreNotEqual(0, resStream.Length);
            string[] deltArr = DeltaList.readList(resRdr);
            Assert.AreEqual(DeltaResponseResultKind.UpdateRequest, result);
            Assert.AreEqual(2, deltArr.Length);
        }
        
        [Test]
        public void testNewDeltaResponse()
        {
            MasterNodeList<string> localNodeList = new MasterNodeList<string>();
            DataChunkRouteBlazerTest.threeThreeChunks(localNodeList, new Dictionary<Node, List<Route>>());
            MasterNodeList<string> baseNodeList = new MasterNodeList<string>();
            MasterNodeList<string> newLocalNodeList;

            using (FileStream fileStream = new FileStream("testNewDelt.tar.gz", FileMode.Open))
            {
                DeltaResponseResultKind result = DeltaResponseHandler.handleDeltaArchiveResponse(localNodeList, deltaDir, baseNodeList,
                    new StringNodeValueParser(), new ToStringNodeValueExporter<string>(), fileStream,out newLocalNodeList);
                Assert.AreEqual(DeltaResponseResultKind.Acceptance, result);
                Assert.Greater(baseNodeList.DataChunkCount, 0);
            }
        }

        [Test]
        public void testRewriteResponse()
        {
            MasterNodeList<string> localNodeList = new MasterNodeList<string>();
            Dictionary<Node, List<Route>> dict = new Dictionary<Node,List<Route>>();
            localNodeList.nodeRoutesDictionary = dict;
            DataChunkRouteBlazerTest.threeThreeChunks(localNodeList,dict);
            MasterNodeList<string> baseNodeList = new MasterNodeList<string>();
            MasterNodeList<string> newLocalNodeList;

            // testRewrite contains a delta archive for the threeThreeChunks graph
            using (FileStream fileStream = new FileStream("testRewrite.tar.gz", FileMode.Open))
            {
                DeltaResponseResultKind result = DeltaResponseHandler.handleDeltaArchiveResponse(localNodeList, deltaDir, baseNodeList,
                    new StringNodeValueParser(), new ToStringNodeValueExporter<string>(), fileStream, out newLocalNodeList);
                Assert.AreEqual(DeltaResponseResultKind.Rewrite, result);
                Assert.Greater(newLocalNodeList.DataChunkCount, localNodeList.DataChunkCount);
            }
        }
    }
}
