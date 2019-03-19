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
            IEnumerable<int> timeStamps, 
            Func<T, T, float> distanceFunction) : base(points, timeStamps, distanceFunction)
        {
        }

        /// <summary>
        /// The creation time of the OCMC defined as the earliest time stamp in
        /// the set of points. NOTE: Maybe the creation time should be static. Try it out
        /// and see how it works.
        /// </summary>
        public int CreationTime => TimeStamps.ToImmutableSortedSet().First();
    }
}