using System.Collections;
using System.Collections.Generic;

namespace EnvueClustering.ClusteringBase
{
    public abstract class CoreMicroCluster<T>
    {
        private IEnumerable<T> _points;
        private IEnumerable<float> _timeStamps;
        
        public CoreMicroCluster(IEnumerable<T> points, IEnumerable<float> timeStamps)
        {
            _points = points;
            _timeStamps = timeStamps;
            DenStream.
        }
        
        
    }
}