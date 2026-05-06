using System;
using System.Collections.Generic;

namespace TourneeFutee
{
    public class Graph
    {
        private bool _directed;
        private float _noEdgeValue;
        private Matrix _adjMatrix;
        private List<string> _vertexNames;
        private List<float> _vertexValues;

        // Initialise un graphe orienté ou non avec une matrice d'adjacence vide
        public Graph(bool directed = false, float noEdgeValue = 0, bool isOriented = false)
        {
            _directed = directed || isOriented;
            _noEdgeValue = noEdgeValue;
            _adjMatrix = new Matrix(0, 0, noEdgeValue);
            _vertexNames = new List<string>();
            _vertexValues = new List<float>();
        }

        public int Order => _vertexNames.Count;
        public int VertexCount => _vertexNames.Count;
        public bool Directed => _directed;
        public bool IsOriented => _directed;
        public List<string> VertexNames => new List<string>(_vertexNames);

        // Retourne l'index d'un sommet par son nom, exception s'il n'existe pas
        private int GetVertexIndex(string name)
        {
            int index = _vertexNames.IndexOf(name);
            if (index == -1)
                throw new ArgumentException("Vertex pas trouvé");
            return index;
        }

        // Ajoute un sommet au graphe avec un nom et une valeur optionnelle.
        public void AddVertex(string name, float value = 0)
        {
            if (_vertexNames.Contains(name))
                throw new ArgumentException("Vertex existe déjà");
            _vertexNames.Add(name);
            _vertexValues.Add(value);
            _adjMatrix.AddRow(Order - 1);
            _adjMatrix.AddColumn(Order - 1);
        }

        // Supprime un sommet et tous ses arcs associés
        public void RemoveVertex(string name)
        {
            int index = GetVertexIndex(name);
            _vertexNames.RemoveAt(index);
            _vertexValues.RemoveAt(index);
            _adjMatrix.RemoveRow(index);
            _adjMatrix.RemoveColumn(index);
        }

        // Retourne la valeur associée à un sommet
        public float GetVertexValue(string name) => _vertexValues[GetVertexIndex(name)];

        // Modifie la valeur associée à un sommet
        public void SetVertexValue(string name, float value) => _vertexValues[GetVertexIndex(name)] = value;

        // Retourne vrai si le sommet existe dans le graphe
        public bool ContainsVertex(string name) => _vertexNames.Contains(name);

        // Retourne la liste des voisins d'un sommet
        public List<string> GetNeighbors(string vertexName)
        {
            int index = GetVertexIndex(vertexName);
            List<string> neighbors = new List<string>();
            for (int j = 0; j < Order; j++)
                if (_adjMatrix.GetValue(index, j) != _noEdgeValue)
                    neighbors.Add(_vertexNames[j]);
            return neighbors;
        }

        // Ajoute un arc entre deux sommets avec un poids
        public void AddEdge(string sourceName, string destinationName, float weight = 1)
        {
            int i = GetVertexIndex(sourceName);
            int j = GetVertexIndex(destinationName);
            if (_adjMatrix.GetValue(i, j) != _noEdgeValue)
                throw new ArgumentException("Edge existe déjà");
            _adjMatrix.SetValue(i, j, weight);
            if (!_directed) _adjMatrix.SetValue(j, i, weight);
        }

        // Supprime l'arc entre deux sommets
        public void RemoveEdge(string sourceName, string destinationName)
        {
            int i = GetVertexIndex(sourceName);
            int j = GetVertexIndex(destinationName);
            if (_adjMatrix.GetValue(i, j) == _noEdgeValue)
                throw new ArgumentException("Edge n'existe pas");
            _adjMatrix.SetValue(i, j, _noEdgeValue);
            if (!_directed) _adjMatrix.SetValue(j, i, _noEdgeValue);
        }

        // Retourne le poids de l'arc entre deux sommets
        public float GetEdgeWeight(string sourceName, string destinationName)
        {
            int i = GetVertexIndex(sourceName);
            int j = GetVertexIndex(destinationName);
            float weight = _adjMatrix.GetValue(i, j);
            if (weight == _noEdgeValue)
                throw new ArgumentException("Edge does not exist");
            return weight;
        }

        // Modifie le poids de l'arc entre deux sommets
        public void SetEdgeWeight(string sourceName, string destinationName, float weight)
        {
            int i = GetVertexIndex(sourceName);
            int j = GetVertexIndex(destinationName);
            if (_adjMatrix.GetValue(i, j) == _noEdgeValue)
                throw new ArgumentException("Edge n'existe pas");
            _adjMatrix.SetValue(i, j, weight);
            if (!_directed) _adjMatrix.SetValue(j, i, weight);
        }
    }
}