using System;
using EnvueClustering.Data;
using EnvueClustering.TimelessDenStream;

namespace EnvueClustering.ClusteringBase
{
    public static class Similarity
    {
        /// <summary>
        /// Returns the euclidean distance between two euclidean points.
        /// </summary>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static float EuclideanDistance(IEuclidean u, IEuclidean v)
        {
            return (float)Math.Sqrt(Math.Pow(u.X - v.X, 2) + Math.Pow(u.Y - v.Y, 2));
        }
        
        /// <summary>
        /// Returns the euclidean distance between two euclidean points.
        /// </summary>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static float EuclideanDistance<T>(UntimedMicroCluster<T> u, UntimedMicroCluster<T> v) where T : IEuclidean, ITransformable<T>
        {
            return (float)Math.Sqrt(Math.Pow(u.Center.X - v.Center.X, 2) + Math.Pow(u.Center.Y - v.Center.Y, 2));
        }

        /// <summary>
        /// Returns the distance between the centres of two core micro clusters
        /// defined for data objects implementing both IEuclidean and ITransformable.
        /// </summary>
        /// <param name="u">First micro cluster.</param>
        /// <param name="v">Second micro cluster.</param>
        /// <param name="time">Time at which to evaluate the centres.</param>
        /// <typeparam name="T">The type of the objects.</typeparam>
        /// <returns></returns>
        public static float EuclideanDistance<T>(
            CoreMicroCluster<T> u, CoreMicroCluster<T> v, int time) where T : IEuclidean, ITransformable<T>
        {
            return EuclideanDistance(u.Center(time), v.Center(time));
        }

        /// <summary>
        /// Returns the distance (in metres) between two geospatial points defined
        /// by their longitude and latitude.
        /// </summary>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static float HaversineDistance(IGeospatial u, IGeospatial v)
        {
            var R = 6371e3; // Earth radius in metres
            var uLat = DegreesToRadians(u.Latitude);
            var vLat = DegreesToRadians(v.Latitude);
            var deltaLat = DegreesToRadians(v.Latitude - u.Latitude);
            var deltaLon = DegreesToRadians(v.Longitude - v.Latitude);

            var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                    Math.Cos(uLat) * Math.Cos(vLat) *
                    Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return (float) (R * c);
        }

        /// <summary>
        /// Returns the distance (in metres) between the centres of two micro clusters
        /// defined for data objects that implement both IGeospatial and ITransformable.
        /// </summary>
        /// <param name="u">First micro cluster.</param>
        /// <param name="v">Second micro cluster.</param>
        /// <param name="time">The time at which to evaluate the centres of the micro clusters.</param>
        /// <typeparam name="T">The type of the data objects.</typeparam>
        /// <returns></returns>
        public static float HaversineDistance<T>(
            CoreMicroCluster<T> u, CoreMicroCluster<T> v, int time) where T : IGeospatial, ITransformable<T>
        {
            return HaversineDistance(u.Center(time), v.Center(time));
        }

        private static double DegreesToRadians(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        /// <summary>
        /// Returns the cosine similarity (i.e. angle of separation) between two vectors.
        /// </summary>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static float Cosine(float[] u, float[] v)
        {
            return u.Dot(v) / 
                   u.EuclideanLength() * v.EuclideanLength();
        }

        public static float Cosine<T>(T a, T b) where T : IVectorRepresentable<float[]>
        {
            var u = a.AsVector();
            var v = b.AsVector();
            return Cosine(u, v);
        }
    }
}