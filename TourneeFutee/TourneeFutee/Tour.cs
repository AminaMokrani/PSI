
using System;
using System.Collections.Generic;
using System.Linq;

namespace TourneeFutee
{
    public class Tour
    {
        private readonly List<string> _vertices;
        private readonly float _cost;

        public Tour()
        {
            _vertices = new List<string>();
            _cost = float.PositiveInfinity;
        }

        // Constructeur depuis une séqu ordonnée de sommets
        public Tour(List<string> vertices, float cost)
        {
            _vertices = new List<string>(vertices);
            _cost = cost;
        }

        // Constructeur depuis des segments non-ordonnés 
        public Tour(List<(string source, string destination)> segments, float cost)
        {
            _cost = cost;
            _vertices = new List<string>();
            if (segments.Count == 0) return;

            string start = segments[0].source;
            string current = start;
            for (int step = 0; step < segments.Count; step++)
            {
                _vertices.Add(current);
                var seg = segments.FirstOrDefault(s => s.source == current);
                if (seg == default) break;
                current = seg.destination;
                if (current == start) break;
            }
            _vertices.Add(start);
        }

        // Cout total de la tournée
        public float Cost => _cost;

        // utilisé par PersistanceTests
        public float TotalCost => _cost;

        // Séqu ordonnée des sommets, retour au départ inclus
        public IList<string> Vertices => _vertices.AsReadOnly();

        // Nb  de trajets dans le cycle
        public int NbSegments => _vertices.Count > 1 ? _vertices.Count - 1 : 0;

        public bool ContainsSegment((string source, string destination) segment)
        {
            for (int i = 0; i < _vertices.Count - 1; i++)
                if (_vertices[i] == segment.source && _vertices[i + 1] == segment.destination)
                    return true;
            return false;
        }

        // Retourne les segments dans l'ordre de parcours 
        public List<(string source, string destination)> GetOrderedSegments()
        {
            var result = new List<(string, string)>();
            for (int i = 0; i < _vertices.Count - 1; i++)
                result.Add((_vertices[i], _vertices[i + 1]));
            return result;
        }

        public void Print()
        {
            if (_vertices.Count == 0)
            {
                Console.WriteLine("Tournée vide (aucune solution trouvée)");
                return;
            }
            Console.WriteLine($"Coût total : {_cost}");
            Console.WriteLine(string.Join("-", _vertices));
        }
    }
}