using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EnvueClustering.ClusteringBase;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using EnvueClustering.Exceptions;

namespace EnvueClustering
{
    public class DenStream<T> where T : ITransformable<T>
    {
        // Parameter constants
        public const float LAMBDA = .001f;  // Weight fading coefficient - the higher the value, the faster the fade
        public const float EPSILON = 15f;   // Minimum number of points in a core-micro-cluster
        public const float BETA = 1.2f;     // Indicator for threshold between potential- and outlier MCs
        public const float MU = 6f;         // Minimum overall weight of a micro-cluster
        
        // Local CMC collections
        private readonly List<PotentialCoreMicroCluster<T>> _pcmcs;
        private readonly List<OutlierCoreMicroCluster<T>> _ocmcs;
        private ConcurrentQueue<T> _queuedStream;

        // DBSCAN instance
        private IClusterable<CoreMicroCluster<T>> _dbscan;

        // Similarity functions
        private readonly Func<T, T, float> _simFunc;
        private readonly Func<CoreMicroCluster<T>, CoreMicroCluster<T>, int, float> _microClusterSimilarityFunction;
        
        // Local control variables
        private bool _userTerminated = false; 
        private bool _clusteringInProgress = false;

        // Public informational variables
        public int CurrentTime;

        
        public DenStream(
            Func<T, T, float> similarityFunction, 
            Func<CoreMicroCluster<T>, CoreMicroCluster<T>, int, float> microClusterSimilarityFunction)
        {
            _pcmcs = new List<PotentialCoreMicroCluster<T>>();
            _ocmcs = new List<OutlierCoreMicroCluster<T>>();
            _simFunc = similarityFunction;
            _microClusterSimilarityFunction = microClusterSimilarityFunction;
        }

        /// <summary>
        /// Implements the fading functions as described in the DenStream paper.
        /// </summary>
        /// <param name="t">A floating point value denoting the number of time units passed since the
        /// algorithm began running. </param>
        public static float Fading(float t)
        {
            return (float)Math.Pow(2, -LAMBDA * t);
        }

        public void SetDataStream(IEnumerable<T> dataStream)
        {
            _queuedStream = new ConcurrentQueue<T>(dataStream);
        }

        /// <summary>
        /// Launches the MaintainClusterMap algorithm in a background thread.
        /// </summary>
        /// <returns>An action that, when called, terminates the thread.</returns>
        /// <exception cref="DenStreamUninitializedDataStreamException">If the data stream has not been initialized with
        /// SetDataStream() before calling, this exception is thrown.</exception>
        public Action MaintainClusterMap()
        {
            if (_queuedStream == null)
            {
                throw new DenStreamUninitializedDataStreamException(
                    $"The shared data stream resource has not been initialized.\n " +
                    $"Use the SetDataStream() method to initialize the data stream before calling.\n ");
            }
            
            var maintainClusterMapThread = Task.Run(() => MaintainClusterMapAsync());  // Run in background thread
            return () =>
            {
                _userTerminated = true;
                _queuedStream = null;
                maintainClusterMapThread.Dispose(); 
            };  // Return an action to force the thread to terminate
        }

        private void MaintainClusterMapAsync()
        {
            var checkInterval = (int) Math.Ceiling((1 / LAMBDA) * Math.Log(
                                                        (BETA * MU) /
                                                       ((BETA * MU) - 1)));

            while (_queuedStream != null)  // Allow exit by forcing queued stream to null
            {
                // Get the next data point in the data stream
                var successfulDequeue = _queuedStream.TryDequeue(out var p);
                if (!successfulDequeue || _clusteringInProgress)
                    continue;

                CurrentTime = p.TimeStamp;
                
                // Merge p into the cluster map
                Merge(p, _simFunc);
                
                if (CurrentTime % checkInterval != 0) continue;
                
                // Prune PCMCs
                _pcmcs.RemoveAll(pcmc => pcmc.Weight(CurrentTime) < BETA * MU);

                // Prune OCMCs
                _ocmcs.RemoveAll(ocmc =>
                {
                    var threshold = GetXiThreshold(ocmc.CreationTime, CurrentTime, checkInterval);
                    return ocmc.Weight(CurrentTime) < threshold;
                });
            }

            if (!_userTerminated)  // The _userTerminated field is only true if the terminating action was called
            {
                throw new DenStreamNullReferenceException(
                    $"Data stream was nullified while the method was running. Aborting MaintainClusterMap...");
            }
        }

        public T[][] Cluster()
        {
            _clusteringInProgress = true;  // Lock PCMC and OCMC collections
            
            _dbscan = new DbScan<CoreMicroCluster<T>>(50, 2, CurrentTime, _microClusterSimilarityFunction);
            var pcmcClusters = _dbscan.Cluster(PotentialCoreMicroClusters);
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

        /// <summary>
        /// Merges a new point p into the cluster map maintained by the DenStream object.
        /// </summary>
        /// <param name="p">The point to insert</param>
        /// <param name="similarityFunction">Function to determine the distance between two points.</param>
        private void Merge(T p, Func<T, T, float> similarityFunction)
        {
            var successfulInsert = false;
            var t = p.TimeStamp;

            if (_pcmcs.Count() != 0)
            {
                // Find the closest PCMCs
                var clustersWithDistance = _pcmcs
                    .Select(pcmc => (pcmc, similarityFunction(p, pcmc.Center(t))))
                    .ToList();
                
                clustersWithDistance.Sort((x, y) => x.Item2.CompareTo(y.Item2));
                var (cluster, _) = clustersWithDistance.First();
                
                // Try to insert considering the new radius
                successfulInsert = TryInsert(p, cluster, 
                    newCluster => newCluster.Radius(t) <= EPSILON);
            }

            if (!successfulInsert && _ocmcs.Count() != 0)
            {
                // Find the closest OCMC
                var clustersWithDistance = _ocmcs
                    .Select(pcmc => (pcmc, similarityFunction(p, pcmc.Center(t))))
                    .ToList();
                
                clustersWithDistance.Sort((x, y) => x.Item2.CompareTo(y.Item2));
                var (cluster, _) = clustersWithDistance.First();
                
                // Try to insert considering the new radius
                successfulInsert = TryInsert(p, cluster, 
                    newCluster => newCluster.Radius(t) <= EPSILON);
                
                if (successfulInsert)
                {
                    
                    // Check the weight
                    if ((cluster.Weight(t) > BETA * MU) && cluster.Radius(t) <= EPSILON)
                    {
                        // Convert this OCMC into a new PCMC
                        _ocmcs.Remove(cluster);
                        _pcmcs.Add(new PotentialCoreMicroCluster<T>(
                            cluster.Points, 
                            similarityFunction));
                    }
                }
            }

            if (!successfulInsert)
            {
                var cluster = new OutlierCoreMicroCluster<T>(
                    new[] {p},
                    similarityFunction);
                
                // Create a new OCMC
                _ocmcs.Add(cluster);
            }
        }

        public List<PotentialCoreMicroCluster<T>> PotentialCoreMicroClusters => _pcmcs.ToList();
        public List<OutlierCoreMicroCluster<T>> OutlierCoreMicroClusters => _ocmcs.ToList();

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
        private bool TryInsert(T p, CoreMicroCluster<T> cluster, Predicate<CoreMicroCluster<T>> predicate)
        {
            cluster.Points.Add(p);
            if (predicate(cluster))
            {
                return true;
            }

            cluster.Points.Remove(p);
            return false;
        }

        private float GetXiThreshold(int creationTime, int dataPointTime, int checkInterval)
        {
            var numerator = Math.Pow(2, (-LAMBDA * (dataPointTime - creationTime + checkInterval))) - 1;
            var denominator = Math.Pow(2, (-LAMBDA * checkInterval)) - 1;
            return (float) (numerator / denominator);
        }
    }
}