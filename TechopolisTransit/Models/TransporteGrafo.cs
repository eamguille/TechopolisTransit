using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace TechopolisTransit.Models
{
    public class TransporteGrafo
    {
        private readonly Dictionary<Estacion, List<Ruta>> _adjacencyList;
        private readonly List<Estacion> _stations;

        public TransporteGrafo()
        {
            _adjacencyList = new Dictionary<Estacion, List<Ruta>>();
            _stations = new List<Estacion>();
        }

        public IReadOnlyList<Estacion> Stations => _stations.AsReadOnly();

        // Agrega una estación al grafo
        public void AddStation(Estacion station)
        {
            if (!_adjacencyList.ContainsKey(station))
            {
                _adjacencyList[station] = new List<Ruta>();
                _stations.Add(station);
            }
        }

        // Agrega o actualiza una ruta entre dos estaciones
        public void AddRoute(Estacion origin, Estacion destination, int travelTime, bool bidirectional = true)
        {
            if (!_adjacencyList.ContainsKey(origin) || !_adjacencyList.ContainsKey(destination))
                throw new ArgumentException("Una o ambas estaciones no existen en el grafo.");

            UpdateOrAddRoute(origin, destination, travelTime);

            if (bidirectional)
                UpdateOrAddRoute(destination, origin, travelTime);
        }

        private void UpdateOrAddRoute(Estacion from, Estacion to, int time)
        {
            var existing = _adjacencyList[from].FirstOrDefault(r => r.Destination.Equals(to));
            if (existing != null)
                existing.TravelTime = time;
            else
                _adjacencyList[from].Add(new Ruta(from, to, time));
        }

        // Obtiene rutas salientes de una estación
        public List<Ruta> GetRoutes(Estacion station)
        {
            return _adjacencyList.ContainsKey(station)
                ? _adjacencyList[station]
                : new List<Ruta>();
        }

        // Obtiene todas las rutas (sin duplicar bidireccionales)
        public List<Ruta> GetAllRoutes()
        {
            var routes = new List<Ruta>();
            var seen = new HashSet<string>();

            foreach (var kvp in _adjacencyList)
            {
                foreach (var route in kvp.Value)
                {
                    string key1 = $"{route.Origin.Id}-{route.Destination.Id}";
                    string key2 = $"{route.Destination.Id}-{route.Origin.Id}";

                    if (!seen.Contains(key1) && !seen.Contains(key2))
                    {
                        routes.Add(route);
                        seen.Add(key1);
                    }
                }
            }
            return routes;
        }

        public Estacion? GetStationByName(string name) =>
            _stations.FirstOrDefault(s => s.Name == name);

        public Estacion? GetStationById(int id) =>
            _stations.FirstOrDefault(s => s.Id == id);
    }
}
