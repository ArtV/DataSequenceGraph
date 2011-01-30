using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class EdgeRoute<T> : BaseRoute<T>,Route<T>
    {
        private Edge<T> edge;
        private Edge<T> requisiteEdge;

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
                    requisiteEdge
                };
            }
        }

        public EdgeRoute(Edge<T> baseNodes,Edge<T> requisiteEdge)
        {
            this.edge = baseNodes;
            this.requisiteEdge = requisiteEdge;
        }


    }
}
