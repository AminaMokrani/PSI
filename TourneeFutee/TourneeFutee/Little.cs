using System;
using System.Collections.Generic;
using System.Linq;

namespace TourneeFutee
{
    public class Little
    {
        private readonly Graph _graph;
        private readonly int _n;
        private readonly List<string> _vertexNames;
        private float _bestCost;
        private List<(string source, string destination)> _bestSegments;

        public Little(Graph graph)
        {
            _graph = graph;
            _n = graph.Order;
            _vertexNames = graph.VertexNames;
            _bestCost = float.PositiveInfinity;
            _bestSegments = new List<(string, string)>();
        }

        public Tour ComputeOptimalTour()
        {
            _bestCost = float.PositiveInfinity;
            _bestSegments = new List<(string, string)>();

            Matrix m = BuildCostMatrix();
            float initialBound = ReduceMatrix(m);
            if (float.IsPositiveInfinity(initialBound)) return new Tour();

            Solve(m,
                  new List<string>(_vertexNames),
                  new List<string>(_vertexNames),
                  initialBound,
                  new List<(string, string)>());

            return new Tour(_bestSegments, _bestCost);
        }

        public static float ReduceMatrix(Matrix m)
        {
            float total = 0;

            for (int i = 0; i < m.NbRows; i++)
            {
                float minVal = float.PositiveInfinity;
                for (int j = 0; j < m.NbColumns; j++)
                    if (m.GetValue(i, j) < minVal) minVal = m.GetValue(i, j);

                if (float.IsPositiveInfinity(minVal)) return float.PositiveInfinity;

                if (minVal > 0)
                {
                    for (int j = 0; j < m.NbColumns; j++)
                        if (!float.IsPositiveInfinity(m.GetValue(i, j)))
                            m.SetValue(i, j, m.GetValue(i, j) - minVal);
                    total += minVal;
                }
            }

            for (int j = 0; j < m.NbColumns; j++)
            {
                float minVal = float.PositiveInfinity;
                for (int i = 0; i < m.NbRows; i++)
                    if (m.GetValue(i, j) < minVal) minVal = m.GetValue(i, j);

                if (float.IsPositiveInfinity(minVal)) return float.PositiveInfinity;

                if (minVal > 0)
                {
                    for (int i = 0; i < m.NbRows; i++)
                        if (!float.IsPositiveInfinity(m.GetValue(i, j)))
                            m.SetValue(i, j, m.GetValue(i, j) - minVal);
                    total += minVal;
                }
            }

            return total;
        }

        public static (int i, int j, float value) GetMaxRegret(Matrix m)
        {
            int bestI = -1, bestJ = -1;
            float maxRegret = float.NegativeInfinity;

            for (int i = 0; i < m.NbRows; i++)
            {
                for (int j = 0; j < m.NbColumns; j++)
                {
                    if (m.GetValue(i, j) != 0f) continue;

                    float rowMin = float.PositiveInfinity;
                    for (int k = 0; k < m.NbColumns; k++)
                        if (k != j && m.GetValue(i, k) < rowMin)
                            rowMin = m.GetValue(i, k);

                    float colMin = float.PositiveInfinity;
                    for (int k = 0; k < m.NbRows; k++)
                        if (k != i && m.GetValue(k, j) < colMin)
                            colMin = m.GetValue(k, j);

                    float regret = rowMin + colMin;
                    if (regret > maxRegret)
                    {
                        maxRegret = regret;
                        bestI = i;
                        bestJ = j;
                    }
                }
            }

            return (bestI, bestJ, maxRegret);
        }

        public static bool IsForbiddenSegment(
            (string source, string destination) segment,
            List<(string source, string destination)> includedSegments,
            int nbCities)
        {
            string current = segment.destination;
            int chainLength = 0;
            bool progressed = true;

            while (progressed)
            {
                progressed = false;
                foreach (var s in includedSegments)
                {
                    if (s.source == current)
                    {
                        if (s.destination == segment.source)
                        {
                            int cycleLength = chainLength + 2;
                            return cycleLength < nbCities;
                        }
                        current = s.destination;
                        chainLength++;
                        progressed = true;
                        break;
                    }
                }
            }

            return false;
        }

        private void Solve(
            Matrix matrix,
            List<string> rowVertices,
            List<string> colVertices,
            float bound,
            List<(string source, string destination)> included)
        {
            if (bound >= _bestCost) return;

            if (rowVertices.Count == 1)
            {
                if (bound < _bestCost)
                {
                    _bestCost = bound;
                    _bestSegments = new List<(string, string)>(included)
                    {
                        (rowVertices[0], colVertices[0])
                    };
                }
                return;
            }

            var (row, col, _) = GetMaxRegret(matrix);
            if (row == -1) return;

            string fromV = rowVertices[row];
            string toV = colVertices[col];

            {
                Matrix mInc = CopyMatrix(matrix);
                var inclCopy = new List<(string, string)>(included) { (fromV, toV) };

                int toVRow = rowVertices.IndexOf(toV);
                int fromVCol = colVertices.IndexOf(fromV);
                if (toVRow >= 0 && fromVCol >= 0)
                    mInc.SetValue(toVRow, fromVCol, float.PositiveInfinity);

                for (int i = 0; i < mInc.NbRows; i++)
                    for (int j = 0; j < mInc.NbColumns; j++)
                        if (!float.IsPositiveInfinity(mInc.GetValue(i, j)))
                            if (IsForbiddenSegment((rowVertices[i], colVertices[j]), inclCopy, _n))
                                mInc.SetValue(i, j, float.PositiveInfinity);

                mInc.RemoveRow(row);
                mInc.RemoveColumn(col);

                var rV = new List<string>(rowVertices);
                rV.RemoveAt(row);
                var cV = new List<string>(colVertices);
                cV.RemoveAt(col);

                float newBound = bound + ReduceMatrix(mInc);
                Solve(mInc, rV, cV, newBound, inclCopy);
            }

            {
                Matrix mExc = CopyMatrix(matrix);
                mExc.SetValue(row, col, float.PositiveInfinity);

                float newBound = bound + ReduceMatrix(mExc);
                Solve(mExc,
                      new List<string>(rowVertices),
                      new List<string>(colVertices),
                      newBound,
                      new List<(string, string)>(included));
            }
        }

        private Matrix BuildCostMatrix()
        {
            var m = new Matrix(_n, _n, float.PositiveInfinity);
            for (int i = 0; i < _n; i++)
                for (int j = 0; j < _n; j++)
                {
                    if (i == j) continue;
                    try { m.SetValue(i, j, _graph.GetEdgeWeight(_vertexNames[i], _vertexNames[j])); }
                    catch { }
                }
            return m;
        }

        private static Matrix CopyMatrix(Matrix m)
        {
            var copy = new Matrix(m.NbRows, m.NbColumns, m.DefaultValue);
            for (int i = 0; i < m.NbRows; i++)
                for (int j = 0; j < m.NbColumns; j++)
                    copy.SetValue(i, j, m.GetValue(i, j));
            return copy;
        }
    }
}