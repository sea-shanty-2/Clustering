using System;
using System.Collections.Generic;
using EnvueClustering.ClusteringBase;

namespace EnvueClustering
{
    public class ShrinkageClustering<T> : IClusterable<T>
    {
        
        public T[][] Cluster(IEnumerable<T> dataStream, Func<T, T, float> similarityFunction)
        {
            throw new NotImplementedException();
        }
    }
}