using System;
using System.Collections;
using EnvueClustering.Data;

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
        /// Returns the distance between the centres of two core micro clusters
        /// defined for EuclideanPoints.
        /// </summary>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static float EuclideanCoreMicroClusterDistance(
            CoreMicroCluster<EuclideanPoint> u,
            CoreMicroCluster<EuclideanPoint> v,
            int time)
        {
            return (float) Math.Sqrt(Math.Pow(u.Center(time).X - v.Center(time).X, 2) +
                                     Math.Pow(u.Center(time).Y - v.Center(time).Y, 2));
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
            var R = 6371e3; // Metres
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
        
        private static double DegreesToRadians(double angle)
        {
            return Math.PI * angle / 180.0;
        }
    }
}