using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class EdgeRoute<T> : Route<T>
    {
        private Edge<T> edge;

        public override Node<T> startNode 
        {
            get
            {
                return connectedNodes.ElementAt(0);
            }
        }
        public override IEnumerable<Node<T>> connectedNodes
        {
            get
            {
                return new List<Node<T>>() 
                {
                    edge.from, edge.to
                };
            }
        }
        public override IEnumerable<Edge<T>> requisiteEdges
        {
            get
            {
                return new List<Edge<T>>()
                {
                    new Edge<T>() { from = edge.requisiteEdgeFrom, to = edge.requisiteEdgeTo }
                };
            }
        }

        public EdgeRoute(Edge<T> baseNodes)
        {
            this.edge = baseNodes;
        }


    }
}
