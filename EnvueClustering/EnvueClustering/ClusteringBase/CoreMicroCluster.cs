using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EnvueClustering.ClusteringBase
{
    /// <summary>
    /// Class representing the base core micro cluster structure in DenStream.
    /// Core micro clusters contain a collection of data points of arbitrary dimensionality.
    /// Because of the need of uniform transformation of data points during the calculation of
    /// weights and centers, it is required that data points implement the ITransformable interface.
    /// </summary>
    /// <typeparam name="T">The type of the data points</typeparam>
    public abstract class CoreMicroCluster<T> where T : ITransformable<T>
    {
        public readonly List<T> Points;
        public readonly List<int> TimeStamps;
        protected readonly Func<float, float> Fading;
        
        private readonly Func<T, T, float> _distanceFunction;

        
        protected CoreMicroCluster(
            IEnumerable<T> points,               // The points contained in the cluster
            IEnumerable<int> timeStamps,       // The timestamps for all points
            Func<T, T, float> distanceFunction)  // Function to use when calculating distance (see Radius)
        {
            Points = points as List<T>;
            TimeStamps = timeStamps as List<int>;
            Fading = DenStream<T>.Fading;
            _distanceFunction = distanceFunction;
        }

        /// <summary>
        /// Returns the weight of a core-micro-cluster defined as the sum of faded time scores
        /// for all points in the cluster.
        /// </summary>
        /// <param name="time">The current timestamp.</param>
        /// <returns>The weight of the cluster.</returns>
        public virtual float Weight(int time)
        {
            return TimeStamps.Select(t => Fading(time - t)).Sum();
        }

        /// <summary>
        /// Returns the center of the micro-cluster defined as the average of weighted points
        /// normalized against the weight of the micro-cluster.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public virtual T Center(int time)
        {
            var timeStampedPoints = Points.Zip(TimeStamps, (p, t) => (p, t));
            var weightedSum = timeStampedPoints.Select(tuple =>
            {
                var (p, t) = tuple;
                return p.Scale(Fading(time - t));
            }).ElementWiseSum();

            return weightedSum.Divide(Weight(time));
        }

        /// <summary>
        /// Returns the radius of the micro-cluster defined as the time-weighted sum of the
        /// distances between points and the center normalized against the weight of the micro-cluster.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public virtual float Radius(int time)
        {
            var c = Center(time);
            var timeStampedPoints = Points.Zip(TimeStamps, (p, t) => (p, t));
            return timeStampedPoints.Select(tuple =>
            {
                var (p, t) = tuple;
                var dist = _distanceFunction(p, c);
                return Fading(time - t) * dist;
            }).Sum() / Weight(time);
        }
    }
}