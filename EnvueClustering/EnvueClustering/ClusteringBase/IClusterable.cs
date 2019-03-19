using System;
using System.Collections.Generic;

namespace EnvueClustering.ClusteringBase
{
    public interface IClusterable
    {
        float Cluster<T>(IEnumerable<T> dataStream, Func<T, T, float> similarityFunction);
    }
}