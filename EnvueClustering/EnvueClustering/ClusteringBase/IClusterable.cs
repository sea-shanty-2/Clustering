using System;
using System.Collections.Generic;

namespace EnvueClustering.ClusteringBase
{
    public interface IClusterable<T>
    {
        float Cluster(IEnumerable<T> dataStream, Func<T, T, float> similarityFunction);
    }
}