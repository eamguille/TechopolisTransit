using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace TechopolisTransit.Models
{
    public class Ruta
    {
        public Estacion Origin { get; set; }
        public Estacion Destination { get; set; }
        public int TravelTime { get; set; } // en minutos

        public Ruta(Estacion origin, Estacion destination, int travelTime)
        {
            Origin = origin;
            Destination = destination;
            TravelTime = travelTime;
        }

        public override string ToString() =>
            $"{Origin.Name} → {Destination.Name} ({TravelTime} min)";
    }
}
