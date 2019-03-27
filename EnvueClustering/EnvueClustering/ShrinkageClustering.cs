using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EnvueClustering.ClusteringBase;

namespace EnvueClustering
{
    public class ShrinkageClustering<T> : IClusterable<T> where T : ITransformable<T>
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
            var clusters = new List<T>[assignmentMatrix.Columns.Count()];

            foreach (var (i, val) in dataArr.Enumerate())
            {
                var clusterIndex = assignmentMatrix[i].ArgMax();
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
            var S = Matrix.SimilarityMatrix(dataStream, _similarityFunction);
            var SBar = 1 - 2 * S;
            var A = Matrix.RandomAssignmentMatrix(S.Shape[0], _k);

            foreach (var iteration in _maxIterations.Range())
            {
                // Remove empty clusters
                var emptyColumns = A.Columns.Enumerate().Where(pair =>
                {
                    var (i, column) = pair;
                    return Math.Abs(column.Sum()) < 0.0000;
                }).Select(pair => 
                { 
                    var (i, column) = pair;
                    return i;
                });

                A = emptyColumns
                    .Aggregate(A, (m, emptyColumn) => m.DeleteColumn(emptyColumn));

                _k = A.Columns.Count();
                
                // Permute cluster membership that minimizes loss the most: 
                // (a) Compute M = ~SA
                var M = SBar * A;
                
                // (b) Compute v
                var MoA = M.Hadamard(A);
                var v = MoA.Select(row => row.Min()).ToArray();
                
                // Check if we converged
                if (Math.Abs(v.Sum()) < 0.0001)
                    break;
                    
                // (c) Find the object X with the greatest optimization potential
                var X = v.ArgMin();
                
                // Reassign X to the cluster C where C = argmin(M[X][j]) w.r.t. j
                var C = M[X].ArgMin();
                A[X] = 0f.Repeat(_k).ToArray();
                A[X][C] = 1;
            }

            return A;
        }
    }
}