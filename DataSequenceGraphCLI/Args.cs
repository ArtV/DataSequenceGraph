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

        [Option("v", "verbose", HelpText = "Display all nodes and edges sent to output files.")]
        public bool Verbose = false;

        [Option("x", "loadxml", HelpText = "XML graph file to load as primary.")]
        public string InXMLFile = null;

        [Option("e", "loadedges", HelpText = "Binary file of graph edges to load as primary (pair with -t).")]
        public string InDatFile = null;

        [Option("t", "loadvalues", HelpText = "Text file of graph data values to load as primary (pair with -e).")]
        public string InTxtFile = null;

        [Option("s", "splittext", HelpText = "Text file to be split into sentence chunks made of word values (clashes with -c, -y, -f, -u).")]
        public string InSrcFile = null;

        [Option("X", "outxml", HelpText = "XML graph file for output.")]
        public string OutXMLFile = null;

        [Option("E", "outedges", HelpText = "Binary file of graph edges for output (pair with -T).")]
        public string OutDatFile = null;

        [Option("T", "outvalues", HelpText = "Text file of graph data values for output (pair with -E).")]
        public string OutTxtFile = null;

        [Option("c", "chunk", HelpText = "Process only the nth stored chunk of the primary graph (clashes with -m, -s).")]
        public int Chunk = -1;

        [Option("l", "leastchunk", HelpText = "Like -c but include all chunks starting with the nth (clashes with -c).")]
        public int LeastChunk = -1;

        [Option("C", "chunktext", HelpText = "Output file for the full chunk text.")]
        public string OutChunkFile = null;

        [Option("y", "load2xml", HelpText = "XML graph file to load as secondary/smaller.")]
        public string InXMLFile2 = null;

        [Option("f", "load2edges", HelpText = "Binary file of graph edges to load as secondary/smaller (pair with -u).")]
        public string InDatFile2 = null;

        [Option("u", "load2values", HelpText = "Text file of graph data values to load as secondary/smaller (pair with -f).")]
        public string InTxtFile2 = null;

        [Option("m", "missing", HelpText = "Instead of adding and sending the result to output, find the difference or missing parts. (conflicts with -c).")]
        public bool Missing = false;

        [Option("z", "basexml", HelpText = "XML graph file to load as a common base of primary and secondary graphs.")]
        public string BaseXMLFile = null;

        [Option("g", "baseedges", HelpText = "Binary file of graph edges to load as a common base of primary and secondary graphs.")]
        public string BaseDatFile = null;

        [Option("w", "basevalues", HelpText = "Text file of graph data values to load as a common base of primary and secondary graphs.")]
        public string BaseTxtFile = null;

        [Option("r", "reapply", HelpText = "Take the chunks in the secondary graph that were applied to the base graph and re-apply to the primary graph.")]
        public bool ReapplyLater = false;

        [Option("h", "hc", HelpText = "Use the numbered hand/hard-coded graph as the primary.")]
        public int HandCodedList = -1;

        [Option("d", "deltadir", HelpText = "Directory that stores incoming/outgoing graph deltas.")]
        public string DeltaDirectory = null;

        [Option("n", "newdeltadir", HelpText = "Initialize the delta directory (must already exist) with the primary graph (requires -d).")]
        public bool InitDeltaDirectory = false;

        [Option("R", "reqdeltas", HelpText = "Generate a new request for deltas (requires -d).")]
        public bool MakeDeltaRequest = false;

        [Option("i", "incomingreq", HelpText = "Respond to an incoming request for deltas (requires -d).")]
        public string DeltaRequest = null;

        [Option("b", "baseresp", HelpText = "Received .base file response to a request for deltas (requires -d).")]
        public string BaseResponseToRequest = null;

        [Option("a", "archresp", HelpText = "Received .tar.gz archive response to a request for deltas (requires -d).")]
        public string ArchiveResponseToRequest = null;

        [HelpOption(HelpText = "Display this help screen.")]
        public string GetUsage()
        {            
            HelpText txt = new HelpText("DataSequenceGraph CLI");
            txt.AddPreOptionsLine(
"  * Supported graph formats are either 1) XML file, 2) binary edge file plus delimited text file of node value strings.");
            txt.AddPreOptionsLine("  * Omitting output file parameters implies verbose display output instead.");
            txt.AddPreOptionsLine("  * -s adds the new chunks from the source file to the primary graph if specified, else a new graph.");
            txt.AddPreOptionsLine("  * -s and -m sends to output the nodes/edges that need to be added for the chunks from the source file.");
            txt.AddPreOptionsLine("  * Without -c or -m, the secondary graph will be merged into the primary graph.");
            txt.AddPreOptionsLine("  * With -c or -m, output is the nodes/edges from the primary graph absent from the secondary/destination graph.");
            txt.AddPreOptionsLine("  * -C with -c or -m sends the full chunk text to the filename. (This file can be reloaded later with -s.) ");
            txt.AddPreOptionsLine("  * Passing primary, secondary, and base graphs produces the result of first adding the secondary to the base and then " +
                "re-applying the chunks in primary that were formerly added to the base.");
            txt.AddPreOptionsLine("  * -r with primary, secondary, and base graphs produces the result of applying, to the primary graph, the " +
                "chunks that the secondary graph formerly added on top of the base graph.");
            txt.AddPreOptionsLine("  * -d and the parameters that require it may result in changes to the passed directory of graph deltas " +
                "or in new output files written to the current directory.");
            txt.AddOptions(this);
            return txt;
        }

    }
}
