using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EnvueClustering
{
    public static class VectorExtensions
    {
        public static float Sum(this float[] vector)
        {
            if (vector.Length == 0)
                throw new ArgumentException($"Cannot calculate the sum of an empty vector");

            return Enumerable.Sum(vector);
        }

        public static float EuclideanLength(this float[] vector)
        {
            if (vector.Length == 0)
                throw new ArgumentException($"Cannot calculate the euclidean length of an empty vector");

            var squaredSum = vector.Sum(t => (float) Math.Pow(t, 2));
            return (float) Math.Sqrt(squaredSum);
        }

        public static string Pretty<T>(this T[] vector)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("[");
            stringBuilder.AppendJoin(", ", vector);
            stringBuilder.Append("]");
            return stringBuilder.ToString();
        }

        public static string Pretty<T>(this T[][] vectorSeries)
        {
            var stringBuilder = new StringBuilder();
            foreach (var vector in vectorSeries)
                stringBuilder.AppendLine(vector.Pretty());

            return stringBuilder.ToString();
        }

        public static T[][] Window<T>(this T[] arr, int windowSize, float overlap)
        {
            var windows = new List<T[]>();
            for (int i = 0; i < arr.Length; i += (int) (windowSize * overlap))
            {
                windows.Add(arr.Skip(i).Take(windowSize).ToArray());
            }

            return windows.ToArray();
        }
        
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

        public static IEnumerable<int> Range(this int max, int start = 0)
        {
            return Enumerable.Range(start, max);
        }
    }
}