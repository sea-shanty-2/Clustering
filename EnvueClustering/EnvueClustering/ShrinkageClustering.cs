using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EnvueClustering.ClusteringBase;

namespace EnvueClustering
{
    public class ShrinkageClustering<T> : IClusterable<T>
    {
        private int _k;
        private readonly int _maxIterations;
        private readonly Func<T, T, float> _similarityFunction;
        
        
        /// <summary>
        /// Initializes a shrinkage clustering algorithm. 
        /// </summary>
        /// <param name="k">The initial number of clusters. This number will be reduced to the optimal number of clusters.</param>
        /// <param name="maxIterations">The maximum number of iterations before the algorithm terminates.</param>
        /// <param name="similarityFunction">Similarity function for points in the data stream.
        /// Must output 0 if elements are equal, otherwise a positive value.</param>
        public ShrinkageClustering(int k, int maxIterations, Func<T, T, float> similarityFunction)
        {
            _k = k;
            _maxIterations = maxIterations;
            _similarityFunction = similarityFunction;
        }

        public T[][] Cluster(IEnumerable<T> dataStream)
        {
            var dataArr = dataStream.ToArray();
            var assignmentMatrix = ShrinkClusters(dataArr);
            var clusters = assignmentMatrix.Columns.Select(c => new List<T>()).ToList();

            foreach (var (i, val) in dataArr.Enumerate())
            {
                var clusterIndex = assignmentMatrix[i].ArgMax();
                if (clusters[clusterIndex] == null)
                    clusters[clusterIndex] = new List<T>();
                clusters[clusterIndex].Add(val);
            }

            return clusters.Select(cluster => cluster.ToArray()).ToArray();
        }

        /// <summary>
        /// Returns a cluster assignment matrix that, for every data point indexed by the
        /// matrix rows, assigns it to a cluster (indexed by the columns).
        /// </summary>
        /// <param name="dataStream">The data stream to cluster.</param>
        /// <returns></returns>
        private Matrix ShrinkClusters(IEnumerable<T> dataStream)
        {
            var dataArr = dataStream.ToArray();

            Console.WriteLine($"Received: {dataArr.Pretty()}");

            if (dataArr.Length == 1)
            {
                return new Matrix(new[,]
                {
                    { 1f }
                });
            }

            var S = Matrix.SimilarityMatrix(dataArr, _similarityFunction, normalize: true, inverse: true);
            var SBar = 1 - 2 * S;
            var A = Matrix.RandomAssignmentMatrix(S.Shape[0], _k);
            var N = S.Count();

            foreach (var iteration in _maxIterations.Range())
            {
                // Remove empty clusters, update number of columns
                A = A.DeleteColumns(c => c.Sum() == 0);
                _k = A.Columns.Count();
                
                // Permute cluster membership that minimizes loss the most: 
                // (a) Compute M = ~SA
                var M = SBar * A;
                
                // (b) Compute v
                var MoA = M.Hadamard(A);
                var v = N.Range().Select(i => M[i].Min() - MoA[i].Sum());

                // Check if we converged
                if (Math.Abs(v.Sum()) < 0.0001)
                    break;

                Console.WriteLine(Math.Abs(v.Sum()));
                    
                // (c) Find the object X with the greatest optimization potential
                var X = v.ArgMin();

                // Reassign X to the cluster C where C = argmin(M[X][j]) w.r.t. j
                var C = M[X].ArgMin();
                
                A[X] = 0f.Repeat(_k).ToArray();
                A[X,C] = 1;
            }

            return A;
        }
    }
}