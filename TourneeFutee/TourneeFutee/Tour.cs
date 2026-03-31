using System;
using System.Collections.Generic;
using System.Linq;

namespace TourneeFutee
{
    public class Tour
    {
        private readonly List<(string source, string destination)> _segments;
        private readonly float _cost;

        public Tour()
        {
            _segments = new List<(string source, string destination)>();
            _cost = float.PositiveInfinity;
        }

        public Tour(List<(string source, string destination)> segments, float cost)
        {
            _segments = new List<(string source, string destination)>(segments);
            _cost = cost;
        }

        public float Cost => _cost;

        public int NbSegments => _segments.Count;

        public bool ContainsSegment((string source, string destination) segment)
        {
            return _segments.Any(s => s.source == segment.source && s.destination == segment.destination);
        }

        public void Print()
        {
            if (_segments.Count == 0)
            {
                Console.WriteLine("Tournée vide (aucune solution trouvée)");
                return;
            }

            Console.WriteLine($"Coût total : {_cost}");

            string start = _segments[0].source;
            string current = start;
            Console.Write(current);

            for (int step = 0; step < _segments.Count; step++)
            {
                var seg = _segments.FirstOrDefault(s => s.source == current);
                if (seg == default) break;
                Console.Write($"-{seg.destination}");
                current = seg.destination;
                if (current == start) break;
            }

            Console.WriteLine();
        }
    }
}