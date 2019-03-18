using System;
using System.Collections.Generic;
using System.Text;

namespace EnvueClustering
{
    public class Matrix
                            
    {
        private float[,] _m;

        private readonly int _rows, _columns;

        public Matrix(int rows, int columns)
        {
            _rows = rows;
            _columns = columns;
            var zeros = new float[rows, columns];
            for (var i = 0; i < rows; i++)
            {
                for (var j = 0; j < columns; j++)
                {
                    zeros[i, j] = 0;
                }
            }

            _m = zeros;
        }

        public Matrix(float[,] matrix)
        {
            if (matrix.Rank != 2)
                throw new ArgumentOutOfRangeException($"Expected a 2-d array, but received a {matrix.Rank}-d instead.");
            
            _rows = matrix.GetUpperBound(0) + 1;
            _columns = matrix.GetUpperBound(1) + 1;

            
            _m = matrix;
        }
        
        public Matrix(float[][] vectorSeries)
        {
            _rows = vectorSeries.Length;
            _columns = vectorSeries[0].Length;

            var matrix = new float[_rows, _columns];
            for (var i = 0; i < _rows; i++)
            {
                for (var j = 0; j < _columns; j++)
                {
                    matrix[i, j] = vectorSeries[i][j];
                }
            }

            _m = matrix;
        }

        
        public float[] this[int i]
        {
            get
            {
                var row = new float[_columns];
                for (var k = 0; k < _columns; k++)
                {
                    row[k] = _m[i, k];
                }

                return row;
            }
        }
        
        public float this[int i, int j]
        {
            get => _m[i, j];
            set => _m[i, j] = value;
        }

        public int[] Shape => new int[] {_rows, _columns};

        public static Matrix operator *(Matrix m1, Matrix m2)
        {
            if (m1.Shape[1] != m2.Shape[0])
                throw new FormatException($"Cannot multiply matrices with shapes {m1.Shape} and {m2.Shape}.");

            var matrix = new float[m1.Shape[0], m2.Shape[1]];
            for (var i = 0; i < m1.Shape[0]; i++)
            {
                for (var j = 0; j < m2.Shape[1]; j++)
                {
                    for (var m = 0; m < m1.Shape[1]; m++)
                    {
                        matrix[i, j] += m1[i, m] * m2[m, j];
                    }
                }
            }
            return new Matrix(matrix);
        }
        
        public static Matrix operator *(Matrix m1, float scalar)
        {
            var matrix = new float[m1.Shape[0], m1.Shape[1]];
            for (var i = 0; i < m1.Shape[0]; i++)
            {
                for (var j = 0; j < m1.Shape[1]; j++)
                {
                    matrix[i, j] = m1[i, j] * scalar;
                }
            }
            
            return new Matrix(matrix);
        }
        
        public static Matrix operator -(Matrix m1, float scalar)
        {
            var matrix = new float[m1.Shape[0], m1.Shape[1]];
            for (var i = 0; i < m1.Shape[0]; i++)
            {
                for (var j = 0; j < m1.Shape[1]; j++)
                {
                    matrix[i, j] = m1[i, j] - scalar;
                }
            }
            
            return new Matrix(matrix);
        }
        
        public static Matrix operator +(Matrix m1, float scalar)
        {
            var matrix = new float[m1.Shape[0], m1.Shape[1]];
            for (var i = 0; i < m1.Shape[0]; i++)
            {
                for (var j = 0; j < m1.Shape[1]; j++)
                {
                    matrix[i, j] = m1[i, j] - scalar;
                }
            }
            
            return new Matrix(matrix);
        }

        public Matrix T
        {
            get 
            {
                var matrix = new float[_columns, _rows];
                for (var i = 0; i < _rows; i++)
                {
                    for (var j = 0; j < _columns; j++)
                    {
                        matrix[j, i] = _m[i, j];
                    }
                }
            
                return new Matrix(matrix);}
        }

        public IEnumerable<float[]> Columns
        {
            get
            {
                for (int j = 0; j < _columns; j++)
                {
                    float[] column = new float[_rows];
                    for (int i = 0; i < _rows; i++)
                    {
                        column[i] = _m[i, j];
                    }

                    yield return column;
                }
            }
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < _rows; i++)
                stringBuilder.AppendLine(this[i].Pretty());
            return stringBuilder.ToString();
        }
    }
}