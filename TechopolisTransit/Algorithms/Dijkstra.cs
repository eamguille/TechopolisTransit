using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TechopolisTransit.Models;
using static System.Collections.Specialized.BitVector32;

namespace TechopolisTransit.Algorithms
{

    public class ShortestPathResult
    {
        public List<Estacion> Path { get; set; } = new();
        public int TotalTime { get; set; }
        public bool PathFound { get; set; }
    }

    public class Dijkstra
    {
        private readonly TransporteGrafo _graph;

        public Dijkstra(TransporteGrafo graph)
        {
            _graph = graph;
        }

        public ShortestPathResult FindShortestPath(Estacion origin, Estacion destination)
        {
            var result = new ShortestPathResult();
            var distances = new Dictionary<Estacion, int>();
            var previous = new Dictionary<Estacion, Estacion?>();
            var unvisited = new HashSet<Estacion>();

            // Inicializar distancias en "infinito"
            foreach (var station in _graph.Stations)
            {
                distances[station] = int.MaxValue;
                previous[station] = null;
                unvisited.Add(station);
            }

            distances[origin] = 0;

            while (unvisited.Count > 0)
            {
                // Seleccionar el nodo no visitado con menor distancia
                Estacion? current = null;
                int minDist = int.MaxValue;

                foreach (var station in unvisited)
                {
                    if (distances[station] < minDist)
                    {
                        minDist = distances[station];
                        current = station;
                    }
                }

                // No hay nodo alcanzable o llegamos al destino
                if (current == null || distances[current] == int.MaxValue)
                    break;

                if (current.Equals(destination))
                    break;

                unvisited.Remove(current);

                // Relajar aristas vecinas
                foreach (var route in _graph.GetRoutes(current))
                {
                    if (!unvisited.Contains(route.Destination))
                        continue;

                    int newDist = distances[current] + route.TravelTime;
                    if (newDist < distances[route.Destination])
                    {
                        distances[route.Destination] = newDist;
                        previous[route.Destination] = current;
                    }
                }
            }

            // Sin ruta posible
            if (distances[destination] == int.MaxValue)
            {
                result.PathFound = false;
                return result;
            }

            // Reconstruir el camino
            result.PathFound = true;
            result.TotalTime = distances[destination];

            Estacion? step = destination;
            while (step != null)
            {
                result.Path.Insert(0, step);
                previous.TryGetValue(step, out step);
            }

            return result;
        }
    }
}
