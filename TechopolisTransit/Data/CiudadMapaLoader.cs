using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechopolisTransit.Models;

namespace TechopolisTransit.Data
{
    public static class CiudadMapaLoader
    {
        public static TransporteGrafo LoadTechopolisMap()
        {
            var graph = new TransporteGrafo();

            // --- Definición de estaciones ---
            var estaciones = new Estacion[]
            {
                new(1,  "Estación Central"),
                new(2,  "Terminal Norte"),
                new(3,  "Terminal Sur"),
                new(4,  "Mercado del Este"),
                new(5,  "Parque Oeste"),
                new(6,  "Universidad"),
                new(7,  "Hospital General"),
                new(8,  "Aeropuerto"),
                new(9,  "Centro Comercial"),
                new(10, "Complejo Deportivo"),
                new(11, "Museo Nacional"),
                new(12, "Zona Industrial"),
                new(13, "Residencial Norte"),
                new(14, "Residencial Sur"),
            };

            foreach (var e in estaciones)
                graph.AddStation(e);

            // --- Definición de rutas (bidireccionales, tiempo en minutos) ---
            graph.AddRoute(estaciones[0], estaciones[1], 8);   // Central ↔ Terminal Norte
            graph.AddRoute(estaciones[0], estaciones[2], 10);  // Central ↔ Terminal Sur
            graph.AddRoute(estaciones[0], estaciones[3], 6);   // Central ↔ Mercado del Este
            graph.AddRoute(estaciones[0], estaciones[4], 7);   // Central ↔ Parque Oeste
            graph.AddRoute(estaciones[1], estaciones[5], 5);   // Terminal Norte ↔ Universidad
            graph.AddRoute(estaciones[1], estaciones[12], 12);  // Terminal Norte ↔ Residencial Norte
            graph.AddRoute(estaciones[2], estaciones[6], 9);   // Terminal Sur ↔ Hospital General
            graph.AddRoute(estaciones[2], estaciones[13], 11);  // Terminal Sur ↔ Residencial Sur
            graph.AddRoute(estaciones[3], estaciones[8], 8);   // Mercado del Este ↔ Centro Comercial
            graph.AddRoute(estaciones[3], estaciones[7], 15);  // Mercado del Este ↔ Aeropuerto
            graph.AddRoute(estaciones[4], estaciones[10], 6);   // Parque Oeste ↔ Museo Nacional
            graph.AddRoute(estaciones[4], estaciones[9], 10);  // Parque Oeste ↔ Complejo Deportivo
            graph.AddRoute(estaciones[5], estaciones[6], 7);   // Universidad ↔ Hospital General
            graph.AddRoute(estaciones[5], estaciones[10], 4);   // Universidad ↔ Museo Nacional
            graph.AddRoute(estaciones[6], estaciones[7], 20);  // Hospital General ↔ Aeropuerto
            graph.AddRoute(estaciones[7], estaciones[11], 12);  // Aeropuerto ↔ Zona Industrial
            graph.AddRoute(estaciones[8], estaciones[9], 5);   // Centro Comercial ↔ Complejo Deportivo
            graph.AddRoute(estaciones[9], estaciones[13], 7);   // Complejo Deportivo ↔ Residencial Sur
            graph.AddRoute(estaciones[10], estaciones[12], 9);   // Museo Nacional ↔ Residencial Norte
            graph.AddRoute(estaciones[11], estaciones[13], 8);   // Zona Industrial ↔ Residencial Sur

            return graph;
        }
    }
}
