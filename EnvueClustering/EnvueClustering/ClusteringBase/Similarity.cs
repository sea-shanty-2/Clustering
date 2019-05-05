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
            return EuclideanDistance(u.Center, v.Center);
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
        public static float Haversine(IGeospatial u, IGeospatial v)
        {
            var R = 6371e3; // Earth radius in metres
            var lat = DegreesToRadians(v.Latitude - u.Latitude);
            var lng = DegreesToRadians(v.Longitude - u.Longitude);
            var h1 = Math.Sin(lat / 2) * Math.Sin(lat / 2) +
                     Math.Cos(DegreesToRadians(u.Latitude)) * Math.Cos(DegreesToRadians(v.Latitude)) *
                     Math.Sin(lng / 2) * Math.Sin(lng / 2);
            var h2 = 2 * Math.Asin(Math.Min(1, Math.Sqrt(h1)));
            return (float) (R * h2);
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
        public static float Haversine<T>(UntimedMicroCluster<T> u, UntimedMicroCluster<T> v) where T : IGeospatial, ITransformable<T>
        {
            return Haversine(u.Center, v.Center);
        }

        public static float ToRadians(this float val)
        {
            return (float) (Math.PI / 180.0 * val);
        }

        private static double DegreesToRadians(double angle)
        {
            return (Math.PI / 180.0) * angle;
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
                   (u.EuclideanLength() * v.EuclideanLength());
        }

        public static float Cosine<T>(T a, T b) where T : IVectorRepresentable<float[]>
        {
            var u = a.AsVector();
            var v = b.AsVector();
            return Cosine(u, v);
        }
    }
}