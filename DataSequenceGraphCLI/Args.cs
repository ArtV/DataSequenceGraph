using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using CommandLine.Text;

namespace DataSequenceGraphCLI
{
    sealed class Args
    {
        [Option("q", "quiet", HelpText = "Display no command output.")]
        public bool Quiet = false;

        [Option("v", "verbose", HelpText = "Display all produced nodes and edges.")]
        public bool Verbose = false;

        [Option("x", "loadxml", HelpText = "XML graph file to load as primary.")]
        public string InXMLFile = null;

        [Option("e", "loadedges", HelpText = "Binary file of graph edges to load as primary (pair with -t).")]
        public string InDatFile = null;

        [Option("t", "loadvalues", HelpText = "Text file of graph data values to load as primary (pair with -e).")]
        public string InTxtFile = null;

        [Option("s", "splittext", HelpText = "Text file to be split into sentence chunks which are split into word values.")]
        public string InSrcFile = null;

        [Option("X", "outxml", HelpText = "XML graph file for output.")]
        public string OutXMLFile = null;

        [Option("E", "outedges", HelpText = "Binary file of graph edges for output (pair with -T).")]
        public string OutDatFile = null;

        [Option("T", "outvalues", HelpText = "Text file of graph data values for output (pair with -E).")]
        public string OutTxtFile = null;

        [Option("c", "chunk", HelpText = "Process the nth stored chunk of the primary loaded graph.")]
        public int Chunk = -1;

        [Option("y", "load2xml", HelpText = "XML graph file to load as secondary/smaller.")]
        public string InXMLFile2 = null;

        [Option("f", "load2edges", HelpText = "Binary file of graph edges to load as secondary/smaller (pair with -u).")]
        public string InDatFile2 = null;

        [Option("u", "load2values", HelpText = "Text file of graph data values to load as secondary/smaller (pair with -f).")]
        public string InTxtFile2 = null;

        [Option("m", "allmissing", HelpText = "Find all nodes and edges in the primary graph that are missing in the secondary.")]
        public bool AllMissing = false;

        [Option("h", "hc", HelpText = "Use the specified hand-coded graph as the primary.")]
        public int HandCodedList = -1;

        [HelpOption(HelpText = "Display this help screen.")]
        public string GetUsage()
        {            
            HelpText txt = new HelpText("DataSequenceGraph CLI");
            txt.AddOptions(this);
            return txt;
        }

    }
}
