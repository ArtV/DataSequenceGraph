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
        public const string CURBASE = "0027-delta-2011-03-13T05-36-00Z";
        public const string MIDSTR = "0025-delta-2011-03-13T05-34-00Z";
        private DeltaDirectory dir;

        public static void resetDeltaDir()
        {
            string[] testProducedFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "000*.*");
            foreach (string testFile in testProducedFiles)
            {
                File.Delete(testFile);
            }
            string[] testRenamedFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.datx");
            string newName;
            foreach (string fname in testRenamedFiles)
            {
                newName = fname.Substring(0, fname.Length - 5) + ".dat";
                renameOrDelete(fname, newName);
            }
            testRenamedFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.txtx");
            foreach (string fname in testRenamedFiles)
            {
                newName = fname.Substring(0, fname.Length - 5) + ".txt";
                renameOrDelete(fname, newName);
            }
            File.Delete(Directory.GetCurrentDirectory() + @"\0026-delta-2011-03-13T01-35-53Z.dat");
            File.Delete(Directory.GetCurrentDirectory() + @"\0026-delta-2011-03-13T01-35-53Z.txt");
        }

        private static void renameOrDelete(string fname, string newName)
        {
            if (File.Exists(newName))
            {
                File.Delete(fname);
            }
            else
            {
                File.Move(fname, newName);
            }
        }

        [SetUp]
        public void SetUp()
        {
            resetDeltaDir();
            dir = new DeltaDirectory(Directory.GetCurrentDirectory());
        }

        [Test]
        public void testDeltasBefore()
        {
            IEnumerable<string> deltas = dir.getDeltasBeforeOrEqual(MIDSTR,2);
            Assert.AreEqual(-1, deltas.First().CompareTo(MIDSTR));
            Assert.AreEqual(2, deltas.Count());
            Assert.AreNotEqual(true, deltas.Contains(MIDSTR));
            Assert.AreEqual(2, dir.getDeltasBeforeOrEqual(CURBASE,2).Count());
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
            Assert.AreEqual(0, dir.getDeltasAfter(CURBASE).Count());
        }
    }
}
