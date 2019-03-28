using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvueClustering.ClusteringBase;

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
        /// Multiplies all elements in a vector by a given scalar value.
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="scalar"></param>
        /// <returns></returns>
        public static IEnumerable<T> Scale<T>(this IEnumerable<T> vector, float scalar) where T : ITransformable<T>
        {
            return vector.Select(value => value.Scale(scalar));
        }
        
        /// <summary>
        /// Multiplies all elements in a vector by a given scalar value.
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="scalar"></param>
        /// <returns></returns>
        public static IEnumerable<float> Scale(this IEnumerable<float> vector, float scalar)
        {
            return vector.Select(value => value * scalar);
        }

        /// <summary>
        /// Divides all elements in a vector by a scalar value.
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="scalar"></param>
        /// <returns></returns>
        public static IEnumerable<T> Divide<T>(this IEnumerable<T> vector, float scalar) where T : ITransformable<T>
        {
            if (scalar == 0)
            {
                throw new ArgumentException("Cannot divide a vector by 0.");
            }

            return vector.Select(value => value.Divide(scalar));
        }
        
        /// <summary>
        /// Divides all elements in a vector by a scalar value.
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="scalar"></param>
        /// <returns></returns>
        public static IEnumerable<float> Divide(this IEnumerable<float> vector, float scalar)
        {
            if (scalar == 0)
            {
                throw new ArgumentException("Cannot divide a vector by 0.");
            }
            return vector.Select(value => value / scalar);
        }

        /// <summary>
        /// Calculates the L1-norm of a vector of points that implement the ITransformable interface.
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T ElementWiseSum<T>(this IEnumerable<T> source) where T : ITransformable<T>
        {
            return source.Aggregate((result, item) => result.Add(item));
        }

        /// <summary>
        /// Returns a pretty-printed representation of a vector.
        /// </summary>
        public static string Pretty<T>(this IEnumerable<T> vector)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("[");
            stringBuilder.AppendJoin(" ", vector);
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
        /// If the slice exceeds the length of the vector, returns the remaining values.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="index">The index from which the slice will start.</param>
        /// <param name="length">The length of the slice.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] Slice<T>(this T[] source, int index, int length)
        {       
            if (index + length > source.Length)
            {
                length = source.Length - index;
            }
            var slice = new T[length];

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
            return Enumerable.Range(start, max);
        }
        
        /// <summary>
        /// Returns an enumerable of integers with the same value.
        /// </summary>
        /// <param name="value">The values of the enumerable.</param>
        /// <param name="length">The length of the enumerable.</param>
        public static IEnumerable<T> Repeat<T>(this T value, int length)
        {
            return length.Range().Select(_ => value);
        }

        /// <summary>
        /// Converts a collection [v0, v1, ... vn] into a list of pairs
        /// [(0, v0), (1, v1), ... (n, vn)].
        /// </summary>
        /// <param name="source">The collection to enumerate.</param>
        /// <returns></returns>
        public static IEnumerable<(int, T)> Enumerate<T>(this IEnumerable<T> source)
        {
            var arr = source as T[] ?? source.ToArray();  // So we don't enumerate several times.
            var length = arr.Length;
            return length.Range().Zip(arr);
        }

        /// <summary>
        /// Returns the index of the smallest element in the enumerable.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static int ArgMin(this IEnumerable<float> source)
        {
            var arr = source.ToArray();
            if (arr.Length == 0)
            {
                throw new ArgumentException("Cannot find the ArgMin of an empty list.");
            }

            var sorted = arr.Enumerate().ToList();
            sorted.Sort((p1, p2) =>
            {
                var (_, v1) = p1;
                var (_, v2) = p2;
                return v1.CompareTo(v2);
            });

            var (iMin, _) = sorted.First();
            return iMin;
        }
        
        /// <summary>
        /// Returns the index of the largest element in the enumerable.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static int ArgMax(this IEnumerable<float> source)
        {
            var arr = source.ToArray();

            if (arr.Length == 0)
            {
                throw new ArgumentException("Cannot find the ArgMax of an empty list.");
            }

            var sorted = arr.Enumerate().ToList();
            sorted.Sort((p1, p2) =>
            {
                var (_, v1) = p1;
                var (_, v2) = p2;
                return v1.CompareTo(v2);
            });

            var (iMin, _) = sorted.Last();
            return iMin;
        }

        public static IEnumerable<(TFirst, TSecond)> Zip<TFirst, TSecond>(this IEnumerable<TFirst> source, IEnumerable<TSecond> other)
        {
            return source.Zip(other, (first, second) => (first, second));
        }

        /// <summary>
        /// Flattens a 2-d enumerable to a 1-d enumerable.
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> source)
        {
            var flattened = new List<T>();
            foreach (var arr in source)
            {
                flattened.AddRange(arr);
            }

            return flattened;
        }

        /// <summary>
        /// Flattens a 3-d enumerable to a 1-d enumerable.
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<IEnumerable<T>>> source)
        {
            var flattened = new List<T>();
            foreach (var arr2d in source)
            {
                flattened.AddRange(arr2d.Flatten());
            }

            return flattened;
        }
    }
}