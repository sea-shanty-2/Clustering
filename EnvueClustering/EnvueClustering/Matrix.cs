using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace EnvueClustering
{
    /// <summary>
    /// Class for representing and working with matrices of floating point values.
    /// The matrix is represented internally as a multi-dimensional array. The Matrix
    /// class provides a set of methods for iterating over rows, columns, and overloads
    /// arithmetic operators in order to add, subtract and multiply matrices with other
    /// matrices or scalars. 
    /// </summary>
    public class Matrix
                            
    {
        private float[,] _m;

        private readonly int _rows, _columns;

        /// <summary>
        /// Returns a zero-valued matrix of shape (rows, columns). 
        /// </summary>
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

        /// <summary>
        /// Returns a Matrix object representing the given multi-dimensional array.
        /// </summary>
        public Matrix(float[,] matrix)
        {
            _rows = matrix.GetUpperBound(0) + 1;
            _columns = matrix.GetUpperBound(1) + 1;

            if (_rows == 1 && _columns == 1)
            {
                throw new ArgumentException(
                    "Cannot represent an empty multi-dimensional array as a matrix.");
            }


            _m = matrix;
        }
        
        /// <summary>
        /// Returns a Matrix object representing the given array of arrays (a vector series).
        /// The matrix is internally represented as a multi-dimensional array,
        /// so all vectors in the vector series must be of equal length.
        /// </summary>
        /// <param name="vectorSeries">An array of arrays of equal length.</param>
        public Matrix(float[][] vectorSeries)
        {
            if (vectorSeries.Length == 0)
            {
                throw new ArgumentException(
                    "Cannot represent an empty array of arrays as a matrix.");
            }

            if (vectorSeries.Any(vector => vector.Length != vectorSeries[0].Length))
            {
                throw new ArgumentException(
                    "Cannot represent an array of arrays of different lengths as a matrix.");
            }

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

        /// <summary>
        /// Returns the i'th row of the matrix.
        /// </summary>
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
        
        /// <summary>
        /// Returns the value of the cell at position M[i, j].
        /// </summary>
        public float this[int i, int j]
        {
            get => _m[i, j];
            set => _m[i, j] = value;
        }

        /// <summary>
        /// Returns the shape of the matrix.
        /// </summary>
        public int[] Shape => new int[] {_rows, _columns};

        /// <summary>
        /// Performs a matrix multiplication between two matrices.
        /// Note that this is not an element-wise product operation. This
        /// operation corresponds to numpy.matmul().
        /// </summary>
        /// <param name="m1">A matrix of shape (X, Y).</param>
        /// <param name="m2">A matrix of shape (Y, Z)</param>
        /// <returns>A matrix of shape (X, Z)</returns>
        /// <exception cref="FormatException">The matrices must share their facing dimension. If m1 has shape
        /// (Y, 32), then it can only be multiplied with B if B has shape (32, X). If this requirement is not met,
        /// an exception is thrown.</exception>
        public static Matrix operator *(Matrix m1, Matrix m2)
        {
            if (m1.Shape[1] != m2.Shape[0])
                throw new ArgumentException(
                    $"Cannot multiply matrices with shapes {m1.Shape} and {m2.Shape}."
                            + "Matrices must share their facing dimension, i.e. only shapes" 
                            + "respecting(X, Y) * (Y, Z) can be multiplied.");

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
        
        /// <summary>
        /// Scales a matrix by a scalar value. 
        /// </summary>
        /// <param name="m1">Matrix to scale.</param>
        /// <param name="scalar">A scalar value.</param>
        /// <returns>A new matrix where all cell values have been multiplied by the provided scalar value.</returns>
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
        
        /// <summary>
        /// Subtracts a scalar value from all cell values in a matrix.
        /// </summary>
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
        
        /// <summary>
        /// Adds a scalar value to all cell values in a matrix.
        /// </summary>
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

        /// <summary>
        /// Returns the transpose matrix.
        /// </summary>
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

        /// <summary>
        /// Iterator over the columns of the matrix.
        /// </summary>
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