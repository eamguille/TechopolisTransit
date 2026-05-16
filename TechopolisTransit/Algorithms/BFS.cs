using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechopolisTransit.Models;

namespace TechopolisTransit.Algorithms
{
    public class BFS
    {
        private readonly TransporteGrafo _graph;

        public BFS(TransporteGrafo graph)
        {
            _graph = graph;
        }

        // Retorna las estaciones visitadas en orden BFS
        public List<Estacion> Traverse(Estacion origin)
        {
            var visited = new List<Estacion>();
            var queue = new Queue<Estacion>();
            var seen = new HashSet<Estacion>();

            queue.Enqueue(origin);
            seen.Add(origin);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                visited.Add(current);

                foreach (var route in _graph.GetRoutes(current))
                {
                    if (!seen.Contains(route.Destination))
                    {
                        seen.Add(route.Destination);
                        queue.Enqueue(route.Destination);
                    }
                }
            }

            return visited;
        }
    }
}
