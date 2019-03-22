using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace EnvueClustering.ClusteringBase
{
    public class OutlierCoreMicroCluster<T> : CoreMicroCluster<T> where T : ITransformable<T>
    {        
        public OutlierCoreMicroCluster(
            IEnumerable<T> points, 
            Func<T, T, float> distanceFunction) : base(points, distanceFunction)
        {
        }
        
        /// <summary>
        /// Returns the CF1 measure of the cluster defined as the linear sum of time-weighted points.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public T CF1(int time)
        {
            return Points
                .Select(p => p.Scale(Fading(time - p.TimeStamp)))
                .ElementWiseSum();
        }

        /// <summary>
        /// Returns the CF2 measure of the cluster defined as the squared sum of time-weighted points.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public T CF2(int time)
        {
            return Points
                .Select(p => p.Pow(2).Scale(Fading(time - p.TimeStamp)))
                .ElementWiseSum();
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
            var radius = (float)Math.Sqrt(Math.Abs(c1 - c2));

            return radius;
        }

        /// <summary>
        /// The creation time of the OCMC defined as the earliest time stamp in
        /// the set of points. NOTE: Maybe the creation time should be static. Try it out
        /// and see how it works.
        /// </summary>
        public int CreationTime => Points
            .Select(p => p.TimeStamp)
            .ToImmutableSortedSet().First();        
    }
}