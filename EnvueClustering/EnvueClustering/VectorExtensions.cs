using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EnvueClustering
{
    public static class VectorExtensions
    {
        /// <summary>
        /// Returns the element-wise sum of a vector.
        /// </summary>
        /// <param name="vector">An array of floating point values.</param>
        /// <exception cref="ArgumentException">An empty vector will yield an exception.</exception>
        public static float Sum(this float[] vector)
        {
            if (vector.Length == 0)
                throw new ArgumentException($"Cannot calculate the sum of an empty vector");

            return Enumerable.Sum(vector);
        }

        /// <summary>
        /// Returns the euclidean length of a vector, i.e. the square root of the sum of squared elements
        /// in the vector.
        /// </summary>
        /// <param name="vector">An array of floating point values.</param>
        /// <exception cref="ArgumentException">An empty array will yield an exception.</exception>
        public static float EuclideanLength(this float[] vector)
        {
            if (vector.Length == 0)
                throw new ArgumentException($"Cannot calculate the euclidean length of an empty vector");

            var squaredSum = vector.Sum(t => (float) Math.Pow(t, 2));
            return (float) Math.Sqrt(squaredSum);
        }

        /// <summary>
        /// Returns a pretty-printed representation of a vector.
        /// </summary>
        public static string Pretty<T>(this T[] vector)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("[");
            stringBuilder.AppendJoin(", ", vector);
            stringBuilder.Append("]");
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Returns a pretty-printed representation of a
        /// multi-dimensional array.
        /// </summary>
        public static string Pretty<T>(this T[][] vectorSeries)
        {
            var stringBuilder = new StringBuilder();
            foreach (var vector in vectorSeries)
                stringBuilder.AppendLine(vector.Pretty());

            return stringBuilder.ToString();
        }
        
        /// <summary>
        /// Returns a slice (or sub-array) from a vector.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="index">The index from which the slice will start.</param>
        /// <param name="length">The length of the slice.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] Slice<T>(this T[] source, int index, int length)
        {       
            var slice = new T[length];
            if (index + length > source.Length)
            {
                length = source.Length - index;
            }

            Array.Copy(source, index, slice, 0, length);
            return slice;
        }

        /// <summary>
        /// Returns an enumerable range of integers. 
        /// </summary>
        /// <param name="max">The end (exclusive) of the range.</param>
        /// <param name="start">The start (inclusive) of the range (0 by default).</param>
        /// <returns>A range [start, max - 1]</returns>
        public static IEnumerable<int> Range(this int max, int start = 0)
        {
            return Enumerable.Range(start, max - 1);
        }
    }
}