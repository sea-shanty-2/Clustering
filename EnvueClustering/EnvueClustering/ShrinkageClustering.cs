using System;
using System.Collections;
using System.Collections.Generic;
using EnvueClustering.ClusteringBase;

namespace EnvueClustering
{
    public class ShrinkageClustering<T> : IClusterable<T> where T : ITransformable<T>
    {
        private int _k, _maxIterations;
        
        /// <summary>
        /// Initializes a shrinkage clustering algorithm. 
        /// </summary>
        /// <param name="k">The initial number of clusters. This number will be reduced to the optimal number of clusters.</param>
        /// <param name="maxIterations">The maximum number of iterations before the algorithm terminates.</param>
        public ShrinkageClustering(int k, int maxIterations)
        {
            _k = k;
            _maxIterations = maxIterations;
        }

        public T[][] Cluster(IEnumerable<T> dataStream, Func<T, T, float> similarityFunction)
        {
            return null;
        }

        private Matrix SimilarityMatrix(IEnumerable<T> data, Func<T, T, float> similarityFunction)
        {
            return null;
        }
    }
}