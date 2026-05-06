using System;

namespace TourneeFutee
{
    public class Matrix
    {
        private float[,] _data;
        private float _defaultValue;

        // Initialise une matrice de taille nbRowsxnbColumns remplie de defaultValue
        public Matrix(int nbRows = 0, int nbColumns = 0, float defaultValue = 0)
        {
            if (nbRows < 0 || nbColumns < 0)
                throw new ArgumentOutOfRangeException();

            _defaultValue = defaultValue;
            _data = new float[nbRows, nbColumns];

            for (int i = 0; i < nbRows; i++)
                for (int j = 0; j < nbColumns; j++)
                    _data[i, j] = _defaultValue;
        }

        public float DefaultValue => _defaultValue;
        public int NbRows => _data.GetLength(0);
        public int NbColumns => _data.GetLength(1);

        // Insere une nouvelle ligne à la position i, remplie de default value
        public void AddRow(int i)
        {
            if (i < 0 || i > NbRows) throw new ArgumentOutOfRangeException();

            float[,] newData = new float[NbRows + 1, NbColumns];
            for (int r = 0; r < newData.GetLength(0); r++)
                for (int c = 0; c < NbColumns; c++)
                {
                    if (r < i) newData[r, c] = _data[r, c];
                    else if (r == i) newData[r, c] = _defaultValue;
                    else newData[r, c] = _data[r - 1, c];
                }
            _data = newData;
        }

        // Insère une nouvelle colonne à la position j, remplie de default value
        public void AddColumn(int j)
        {
            if (j < 0 || j > NbColumns) throw new ArgumentOutOfRangeException();

            float[,] newData = new float[NbRows, NbColumns + 1];
            for (int r = 0; r < NbRows; r++)
                for (int c = 0; c < newData.GetLength(1); c++)
                {
                    if (c < j) newData[r, c] = _data[r, c];
                    else if (c == j) newData[r, c] = _defaultValue;
                    else newData[r, c] = _data[r, c - 1];
                }
            _data = newData;
        }

        // Supprime la ligne à la position i
        public void RemoveRow(int i)
        {
            if (i < 0 || i >= NbRows) throw new ArgumentOutOfRangeException();

            float[,] newData = new float[NbRows - 1, NbColumns];
            for (int r = 0; r < NbRows; r++)
            {
                if (r == i) continue;
                for (int c = 0; c < NbColumns; c++)
                    newData[r < i ? r : r - 1, c] = _data[r, c];
            }
            _data = newData;
        }

        // Supprime la colonne à la position j
        public void RemoveColumn(int j)
        {
            if (j < 0 || j >= NbColumns) throw new ArgumentOutOfRangeException();

            float[,] newData = new float[NbRows, NbColumns - 1];
            for (int r = 0; r < NbRows; r++)
                for (int c = 0; c < NbColumns; c++)
                {
                    if (c == j) continue;
                    newData[r, c < j ? c : c - 1] = _data[r, c];
                }
            _data = newData;
        }

        // Retourne la valeur à la position (i,j)
        public float GetValue(int i, int j)
        {
            if (i < 0 || i >= NbRows || j < 0 || j >= NbColumns)
                throw new ArgumentOutOfRangeException();
            return _data[i, j];
        }

        // Modifie la valeur à la position (i,j)
        public void SetValue(int i, int j, float v)
        {
            if (i < 0 || i >= NbRows || j < 0 || j >= NbColumns)
                throw new ArgumentOutOfRangeException();
            _data[i, j] = v;
        }

        // Affiche la matrice 
        public void Print()
        {
            for (int i = 0; i < NbRows; i++)
            {
                for (int j = 0; j < NbColumns; j++)
                    Console.Write(_data[i, j] + "\t");
                Console.WriteLine();
            }
        }
    }
}