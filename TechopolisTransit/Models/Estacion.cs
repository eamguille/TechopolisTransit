using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace TechopolisTransit.Models
{
    public class Estacion
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public Estacion(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public override string ToString() => Name;

        public override bool Equals(object? obj) =>
            obj is Estacion other && Id == other.Id;

        public override int GetHashCode() => Id.GetHashCode();
    }
}
