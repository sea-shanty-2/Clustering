using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EnvueClustering.ClusteringBase
{
    /// <summary>
    /// Class representing a potential core-micro-cluster (abbreviated as PCMC in the report).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PotentialCoreMicroCluster<T> : CoreMicroCluster<T> where T : ITransformable<T>
    {
        public PotentialCoreMicroCluster(
            IEnumerable<T> points,
            IEnumerable<int> timeStamps,
            Func<T, T, float> distanceFunction) : base(points, timeStamps, distanceFunction) { }

        /// <summary>
        /// Returns the CF1 measure of the cluster defined as the linear sum of time-weighted points.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public T CF1(int time)
        {
            var timeStampedPoints = Points.Zip(TimeStamps, (p, t) => (p, t));
            return timeStampedPoints.Select(tuple =>
            {
                var (p, t) = tuple;
                return p.Scale(Fading(time - t));
            }).ElementWiseSum();
        }

        /// <summary>
        /// Returns the CF2 measure of the cluster defined as the squared sum of time-weighted points.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public T CF2(int time)
        {
            var timeStampedPoints = Points.Zip(TimeStamps, (p, t) => (p, t));
            return timeStampedPoints.Select(tuple =>
            {
                var (p, t) = tuple;
                return p.Pow(2).Scale(Fading(time - t));
            }).ElementWiseSum();
        }

        /// <summary>
        /// Returns the center of the PCMC defined as CF1 / w.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public override T Center(int time)
        {
            return CF1(time).Divide(Weight(time));
        }

        /// <summary>
        /// Returns the radius of the PCMC defined as sqrt( (|CF2| / w) - (|CF1| / w)^2 ).
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public override float Radius(int time)
        {
            var w = Weight(time);
            var c1 = CF2(time).Size() / w;
            var c2 = Math.Pow(CF1(time).Size() / w, 2);
            return (float)Math.Sqrt(c1 - c2);
        }
    }
}