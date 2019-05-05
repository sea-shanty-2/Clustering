using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Xml.Serialization;

namespace EnvueClustering
{
    /// <summary>
    /// Class for representing and working with matrices of floating point values.
    /// The matrix is represented internally as a multi-dimensional array. The Matrix
    /// class provides a set of methods for iterating over rows, columns, and overloads
    /// arithmetic operators in order to add, subtract and multiply matrices with other
    /// matrices or scalars. 
    /// </summary>
    public class Matrix : IEnumerable<float[]>

    {
        private float[,] _m;                   // Internal matrix representation.
        private readonly int _rows, _columns;  // Dimensions of the matrix.

        /// <summary>
        /// Returns a zero-valued matrix of shape (rows, columns). 
        /// </summary>
        public Matrix(int rows, int columns, float values = 0)
        {
            _rows = rows;
            _columns = columns;
            var zeros = new float[rows, columns];
            for (var i = 0; i < rows; i++)
            {
                for (var j = 0; j < columns; j++)
                {
                    zeros[i, j] = values;
                }
            }

            _m = zeros;
        }

        public Matrix(int[] shape)
        {
            if (shape.Length != 2)
                throw new ArgumentException($"Shape must be an array with exactly two elements.");

            _rows = shape[0];
            _columns = shape[1];
            var zeros = new float[_rows, _columns];
            for (var i = 0; i < _rows; i++)
            {
                for (var j = 0; j < _columns; j++)
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
        /// Accesses the i'th row of the matrix.
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

            set
            {
                foreach (var v in value)
                {
                    for (var j = 0; j < _columns; j++)
                    {
                        _m[i, j] = v;
                    }
                }
            }
        }
        
        /// <summary>
        /// Accesses the value of the cell at position M[i, j].
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
                    $"Cannot multiply matrices with shapes {m1.Shape} and {m2.Shape}." +
                              "Matrices must share their facing dimension, i.e. only shapes" +
                              "respecting(X, Y) * (Y, Z) can be multiplied.");

            var matrix = new Matrix(m1.Shape[0], m2.Shape[1]);
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
            return matrix;
        }

        /// <summary>
        /// Returns the Hadamard product between this and the provided matrix.
        /// Note that this is not a matrix multiplication, but an entry-wise product operation.
        /// </summary>
        public Matrix Hadamard(Matrix m)
        {
            if (!Shape.SequenceEqual(m.Shape))
            {
                throw new ArgumentException(
                    $"Cannot calculate the Hadamard product of matrices of different shapes. " +
                             $"Shapes were {Shape.Pretty()} and {m.Shape.Pretty()}");
            }
            
            var matrix = new Matrix(_rows, _columns);
            for (var i = 0; i < _rows; i++)
            {
                for (var j = 0; j < _columns; j++)
                {
                    matrix[i, j] = m[i, j] * _m[i, j];
                }
            }

            return matrix;
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
        
        public static Matrix operator *(float scalar, Matrix m1)
        {
            return m1 * scalar;
        }
        
        /// <summary>
        /// Divide all values in a matrix by a scalar value. 
        /// </summary>
        /// <param name="m1">Matrix to scale.</param>
        /// <param name="scalar">A scalar value.</param>
        /// <returns>A new matrix where all cell values have been multiplied by the provided scalar value.</returns>
        public static Matrix operator /(Matrix m1, float scalar)
        {
            var matrix = new float[m1.Shape[0], m1.Shape[1]];
            for (var i = 0; i < m1.Shape[0]; i++)
            {
                for (var j = 0; j < m1.Shape[1]; j++)
                {
                    matrix[i, j] = m1[i, j] / scalar;
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

        public static Matrix operator -(float scalar, Matrix m)
        {
            var mScalar = new Matrix(m.Shape[0], m.Shape[1], scalar);
            
            var matrix = new Matrix(m.Shape);
            var rows = m.Shape[0];
            var columns = m.Shape[1];
            for (var i = 0; i < rows; i++)
            {
                for (var j = 0; j < columns; j++)
                {
                    matrix[i, j] = mScalar[i, j] - m[i, j];
                }
            }

            return matrix;
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

        public static Matrix operator +(Matrix m1, Matrix m2)
        {
            if (!m1.Shape.SequenceEqual(m2.Shape))
                throw new ArgumentException($"Cannot add two matrices of different shapes. " +
                                            $"Shapes were: {m1.Shape.Pretty()} and {m2.Shape.Pretty()}");
            
            var matrix = new Matrix(m1.Shape);
            var rows = m1.Shape[0];
            var columns = m1.Shape[1];
            for (var i = 0; i < rows; i++)
            {
                for (var j = 0; j < columns; j++)
                {
                    matrix[i, j] = m1[i, j] + m2[i, j];
                }
            }

            return matrix;
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
                for (var j = 0; j < _columns; j++)
                {
                    var column = new float[_rows];
                    for (var i = 0; i < _rows; i++)
                    {
                        column[i] = _m[i, j];
                    }

                    yield return column;
                }
            }
        }

        /// <summary>
        /// Returns a new matrix that does not contain the provided row index.
        /// </summary>
        /// <param name="i">The index of the row to remove.</param>
        public Matrix DeleteRow(int i)
        {
            if (i >= _rows)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(i), 
                    $"Cannot delete row {i} when the matrix only contains {_rows} rows.");
            }

            var matrix = new List<float[]>();
            for (var k = 0; k < _rows; k++)
            {
                if (k == i)
                    continue;
                
                matrix.Add(this[k]);
            }
            
            return new Matrix(matrix.ToArray());
        }

        /// <summary>
        /// Returns a new matrix that does not contain the provided column.
        /// </summary>
        /// <param name="i">Index of the column to remove.</param>
        public Matrix DeleteColumn(int i)
        {
            if (i >= _columns)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(i), $"Cannot delete column {i} when the matrix only "
                                                  + $"contains {_columns} columns");
            }

            var matrix = new List<float[]>();
            foreach (var (k, column) in Columns.Enumerate())
            {
                if (k != i)
                    matrix.Add(column);
            }
            
            return new Matrix(matrix.ToArray()).T;
        }

        public Matrix DeleteColumns(Predicate<float[]> predicate)
        {
            var matrix = new List<float[]>();

            foreach (var (k, column) in Columns.Enumerate())
            {
                if (!predicate(column))
                    matrix.Add(column);
            }
            
            return new Matrix(matrix.ToArray()).T;
        }

        /// <inheritdoc />
        /// <summary>
        /// Returns an iterator over the rows of the matrix.
        /// </summary>
        public IEnumerator<float[]> GetEnumerator()
        {
            for (var i = 0; i < _rows; i++)
            {
                yield return this[i];
            }
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            for (var i = 0; i < _rows; i++)
                stringBuilder.AppendLine(this[i].Pretty());
            return stringBuilder.ToString();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Returns a matrix where each row has one random cell set to 1, otherwise 0. 
        /// </summary>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        /// <returns></returns>
        public static Matrix RandomAssignmentMatrix(int rows, int columns)
        {
            var matrix = new Matrix(rows, columns);
            var r = new Random();
            for (var i = 0; i < rows; i++)
            {
                var j = r.Next(columns);
                matrix[i, j] = 1;
            }

            return matrix;
        }

        public static Matrix Ones(int rows, int columns)
        {
            var ones = new float[rows, columns];
            for (var i = 0; i < rows; i++)
            {
                for (var j = 0; j < columns; j++)
                {
                    ones[i, j] = 1;
                }
            }

            return new Matrix(ones);
        }

        public static Matrix SimilarityMatrix<T>(
            IEnumerable<T> data, 
            Func<T, T, float> similarityFunction, 
            bool normalize = false, 
            bool inverse = false)
        {
            var arr = data.ToArray();
            var matrix = new Matrix(arr.Length, arr.Length);
            for (var i = 0; i < arr.Length; i++)
            {
                for (var j = 0; j <= i; j++)
                {
                    matrix[i, j] = similarityFunction(arr[i], arr[j]);
                }
            }
            
            matrix = matrix.Square();
            if (normalize)
            {
                matrix = matrix / matrix.Max();
            }

            if (inverse)
            {
                matrix = 1 - matrix;
            }

            return matrix;
        }

        /// <summary>
        /// Mirrors a triangular matrix into a square matrix.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public Matrix Square()
        {
            var mirrored = this.T + this;
            for (var i = 0; i < Shape[0]; i++)
            {
                mirrored[i, i] = this[i, i];
            }

            return mirrored;
        }

        /// <summary>
        /// Returns the largest value in the matrix.
        /// </summary>
        /// <returns></returns>
        public float Max()
        {
            return this
                .Select(row => row.Max())
                .Max();
        }
        
        /// <summary>
        /// Returns the smallest value in the matrix.
        /// </summary>
        /// <returns></returns>
        public float Min()
        {
            return this
                .Select(row => row.Min())
                .Min();
        }
    }
}