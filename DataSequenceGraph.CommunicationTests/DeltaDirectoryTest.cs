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
    public class DeltaDirectoryTest
    {
        const string CURBASE = "27-2011-03-13T05-36-00Z";
        const string MIDSTR = "25-2011-03-13T05-34-00Z";
        private DeltaDirectory dir;

        [SetUp]
        public void SetUp()
        {
            dir = new DeltaDirectory(Directory.GetCurrentDirectory());
        }

        [Test]
        public void testDeltasBefore()
        {
            IEnumerable<string> deltas = dir.getDeltasBefore(MIDSTR);
            Assert.AreEqual(-1, deltas.First().CompareTo(MIDSTR));
            Assert.AreEqual(2, deltas.Count());
            Assert.AreEqual(4, dir.getDeltasBefore(CURBASE).Count());
        }

        [Test]
        public void testGetCurrentBase()
        {
            Assert.AreEqual(CURBASE, dir.CurrentBase);
        }

        [Test]
        public void testDeltasAfter()
        {
            IEnumerable<string> deltas = dir.getDeltasAfter(MIDSTR);
            Assert.AreEqual(1, deltas.First().CompareTo(MIDSTR));
            Assert.AreEqual(2, deltas.Count());
            Assert.AreEqual(1, dir.getDeltasAfter(CURBASE).Count());
        }
    }
}
