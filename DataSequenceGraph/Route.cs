using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class Route
    {
        public Node startNode 
        {
            get
            {
                return connectedNodes.ElementAt(0);
            }
        }
        public IEnumerable<Node> connectedNodes
        {
            get;
            private set;
        }

        private Route(Node from, Node to)
        {
            connectedNodes = new List<Node>()
            {
                from, to
            };            
        }

        private Route(IEnumerable<Node> connectedNodes)
        {
            this.connectedNodes = connectedNodes;
        }

        private Route addRoute(Route other)
        {
            return new Route(this.connectedNodes.Concat(other.connectedNodes));
        }

        public static Route newRouteBetween(Node from, Node to)
        {
            return new Route(from, to);
        }

        public static Route connectRoutes(Route first, Route second)
        {            
            return first.addRoute(second);
        }
    }
}
