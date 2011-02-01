using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataSequenceGraph
{
    public class CompositeRoute<T> : Route<T>
    {
        private IEnumerable<Route<T>> componentRoutes;

        public CompositeRoute(IEnumerable<Route<T>> componentRoutes)
        {
            this.componentRoutes = componentRoutes;
        }

        public override Node<T> startNode
        {
            get
            {
                return componentRoutes.ElementAt(0).startNode;
            }
        }

        public override Route<T> startRoute
        {
            get 
            {
                return componentRoutes.FirstOrDefault<Route<T>>();
            }
        }

        public override IEnumerable<Node<T>> connectedNodes
        {
            get
            {
                foreach (Route<T> componentRoute in componentRoutes)
                {
                    foreach (Node<T> routeNode in componentRoute.connectedNodes)
                    {
                        yield return routeNode;
                    }
                }                
            }
        }

        public override IEnumerable<Edge<T>> requisiteEdges
        {
            get
            {
                foreach (Route<T> componentRoute in componentRoutes)
                {
                    foreach (Edge<T> routeEdge in componentRoute.requisiteEdges)
                    {
                        yield return routeEdge;
                    }
                }
            }
        }

    }
}
