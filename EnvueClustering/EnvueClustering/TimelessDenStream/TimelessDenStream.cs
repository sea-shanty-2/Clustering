using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnvueClustering.ClusteringBase;
using EnvueClustering.Exceptions;

namespace EnvueClustering.TimelessDenStream
{
    public class TimelessDenStream<T> where T : ITransformable<T>, IIdentifiable
    {
        // DenStream parameters
        private const float MAX_RADIUS = 15;
        
        // DBSCAN parameters
        private const float EPS = 250;
        private const int MIN_POINTS = 2;
        
        // Data stream and micro cluster map
        private readonly List<UntimedMicroCluster<T>> _microClusters;
        private ConcurrentQueue<T> _dataStream;

        // DBSCAN instance
        private IClusterable<UntimedMicroCluster<T>> _dbscan;
        
        // Similarity functions
        private Func<T, T, float> _pointSimilarityFunction;
        private Func<UntimedMicroCluster<T>, UntimedMicroCluster<T>, float> _microClusterSimilarityFunction;
        
        // Local control variables
        private bool _userTerminated, _clusteringInProgress;

        public TimelessDenStream(
            Func<T, T, float> pointSimilarityFunction,
            Func<UntimedMicroCluster<T>, UntimedMicroCluster<T>, float> microClusterSimilarityFunction)
        {
            _pointSimilarityFunction = pointSimilarityFunction;
            _microClusterSimilarityFunction = microClusterSimilarityFunction;

            _microClusters = new List<UntimedMicroCluster<T>>();
            _dataStream = new ConcurrentQueue<T>();
            
            _dbscan = new TimelessDbScan<UntimedMicroCluster<T>>(EPS, MIN_POINTS, microClusterSimilarityFunction);
        }

        public List<UntimedMicroCluster<T>> MicroClusters => _microClusters;

        public void Add(T dataPoint)
        {
            _dataStream.Enqueue(dataPoint);
        }

        public void Add(IEnumerable<T> dataPoints)
        {
            foreach (var dataPoint in dataPoints)
                Add(dataPoint);
        }

        /// <summary>
        /// Removes a data point from the cluster map. The object to be removed is
        /// determined by the Id attribute.
        /// </summary>
        /// <param name="dataPoint"></param>
        public void Remove(T dataPoint)
        {
            var emptyMicroClusters = new List<UntimedMicroCluster<T>>();
            foreach (var microCluster in _microClusters)
            {
                microCluster.Points.RemoveAll(p => p.Id.Equals(dataPoint.Id));
                if (microCluster.Points.Count == 0)
                {
                    emptyMicroClusters.Add(microCluster);
                }
            }

            foreach (var emptyMicroCluster in emptyMicroClusters)
            {
                _microClusters.Remove(emptyMicroCluster);
            }
        }

        /// <summary>
        /// Removes the data point from the cluster map and reinserts it in order
        /// to reflect a change in position.
        /// </summary>
        /// <param name="dataPoint"></param>
        public void Update(T dataPoint)
        {
            Remove(dataPoint);
            Add(dataPoint);
        }

        /// <summary>
        /// Launches the MaintainClusterMap algorithm in a background thread.
        /// </summary>
        /// <returns>An action that, when called, terminates the thread.</returns>
        /// <exception cref="DenStreamUninitializedDataStreamException">If the data stream has not been initialized with
        /// SetDataStream() before calling, this exception is thrown.</exception>
        public Action MaintainClusterMap()
        {
            if (_dataStream == null)
            {
                throw new DenStreamUninitializedDataStreamException(
                    $"The shared data stream resource has not been initialized.\n " +
                    $"Use the SetDataStream() method to initialize the data stream before calling.\n ");
            }
            
            var maintainClusterMapThread = Task.Run(() => MaintainClusterMapAsync());  // Run in background thread
            return () =>
            {
                _userTerminated = true;
                _dataStream = null;
                maintainClusterMapThread.Wait();
            };  // Return an action to force the thread to terminate
        }

        private void MaintainClusterMapAsync()
        {
            while (_dataStream != null)
            {
                if (_userTerminated)
                    return;
                
                // Get the next data point in the data stream
                var successfulDequeue = _dataStream.TryDequeue(out var p);
                if (!successfulDequeue || _clusteringInProgress)
                    continue;
                
                // Merge the dataPoint into the micro cluster map
                Merge(p);
            }
        }

        public T[][] Cluster()
        {
            Console.WriteLine($"We have {_microClusters.Count} MCs.");
            _clusteringInProgress = true;  // Lock micro cluster map during clustering
            
            _dbscan = new TimelessDbScan<UntimedMicroCluster<T>>(50, 2, _microClusterSimilarityFunction);
            var pcmcClusters = _dbscan.Cluster(_microClusters);
            var clusters = new List<T[]>();
            
            foreach (var pcmcCluster in pcmcClusters)
            {
                var cluster = new List<T>();
                foreach (var pcmc in pcmcCluster)
                {
                    cluster.AddRange(pcmc.Points);
                }
                
                clusters.Add(cluster.ToArray());
            }
                        
            _clusteringInProgress = false;  // Unlock PCMC and OCMC collections
            return clusters.ToArray();
        }

        private void Merge(T p)
        {
            // Sort micro clusters by distance to p
            _microClusters.Sort((u, v) =>
                _pointSimilarityFunction(u.Center, p)
                    .CompareTo(_pointSimilarityFunction(v.Center, p)));

            if (_microClusters.Count == 0)
            {
                Console.WriteLine($"Creating new MC with p.");
                // Create a new micro cluster, add to the cluster map
                _microClusters.Add(new UntimedMicroCluster<T>(
                    new [] {p}, _pointSimilarityFunction));
            }

            var closestMicroCluster = _microClusters.First();
            
            // Try to insert the point into this cluster
            var successfulInsert = TryInsert(p, closestMicroCluster, 
                (mc) => mc.Radius <= MAX_RADIUS);
            
            if (!successfulInsert)
            {
                Console.WriteLine($"Creating new MC with p.");
                // Create a new micro cluster, add to the cluster map
                _microClusters.Add(new UntimedMicroCluster<T>(
                    new [] {p}, _pointSimilarityFunction));
            }

            Console.WriteLine($"Merged p into existing MC.");
        }
        
        /// <summary>
        /// Attempts to insert a point p into a cluster.
        /// The success of the insertion is based on a predicate on the cluster.
        /// If the predicate succeeds after inserting p into the cluster, the method
        /// returns true. If the predicate fails, p is removed from the cluster and
        /// the method returns false.
        /// </summary>
        /// <param name="p">The point to insert in the cluster.</param>
        /// <param name="cluster">The cluster.</param>
        /// <param name="predicate">The predicate that must pass before insertion is legal.</param>
        /// <returns></returns>
        private bool TryInsert(T p, UntimedMicroCluster<T> cluster, Predicate<UntimedMicroCluster<T>> predicate)
        {
            cluster.Points.Add(p);
            if (predicate(cluster))
            {
                return true;
            }

            cluster.Points.Remove(p);
            return false;
        }
    }
}