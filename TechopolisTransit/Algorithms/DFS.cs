using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechopolisTransit.Models;

namespace TechopolisTransit.Algorithms
{
    public class DFS
    {
        private readonly TransporteGrafo _graph;

        public DFS(TransporteGrafo graph)
        {
            _graph = graph;
        }

        // Retorna las estaciones visitadas en orden DFS
        public List<Estacion> Traverse(Estacion origin)
        {
            var visited = new List<Estacion>();
            var seen = new HashSet<Estacion>();
            TraverseRecursive(origin, visited, seen);
            return visited;
        }

        private void TraverseRecursive(Estacion current, List<Estacion> visited, HashSet<Estacion> seen)
        {
            seen.Add(current);
            visited.Add(current);

            foreach (var route in _graph.GetRoutes(current))
            {
                if (!seen.Contains(route.Destination))
                    TraverseRecursive(route.Destination, visited, seen);
            }
        }
    }
}
