using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        
        // Termination handler
        private Action _terminate;

        public TimelessDenStream(
            Func<T, T, float> pointSimilarityFunction,
            Func<UntimedMicroCluster<T>, UntimedMicroCluster<T>, float> microClusterSimilarityFunction)
        {
            _pointSimilarityFunction = pointSimilarityFunction;
            _microClusterSimilarityFunction = microClusterSimilarityFunction;

            _microClusters = new List<UntimedMicroCluster<T>>();
            _dataStream = new ConcurrentQueue<T>();
            
            _dbscan = new TimelessDbScan<UntimedMicroCluster<T>>(EPS, MIN_POINTS, microClusterSimilarityFunction);
            _terminate = MaintainClusterMap();
        }

        public List<UntimedMicroCluster<T>> MicroClusters => _microClusters;

        /// <summary>
        /// Terminates the cluster maintenance algorithm.
        /// </summary>
        public void Terminate()
        {
            _terminate();
        }

        public void Add(T dataPoint)
        {
            if (dataPoint.Id == null)
                throw new ArgumentException("Cannot add a broadcast with a NULL id.");
            
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
            var allIds = _microClusters.Select(mc => mc.Points.Select(p => p.Id)).Flatten();
            Console.WriteLine($"Trying to remove {dataPoint.Id} from cluster map.");
            Console.WriteLine($"Currently held ids are {allIds.Pretty()}");

            var totalRemoved = 0;
            var emptyMicroClusters = new List<UntimedMicroCluster<T>>();
            foreach (var microCluster in _microClusters)
            {
                totalRemoved += microCluster.Points.RemoveAll(p => p.Id.Equals(dataPoint.Id));
                if (microCluster.Points.Count == 0)
                {
                    emptyMicroClusters.Add(microCluster);
                }
            }
            
            if (totalRemoved == 0)
            {
                throw new KeyNotFoundException($"Failed to remove id {dataPoint.Id} from cluster map. Currently held ids are {allIds.Pretty()}");
            }

            foreach (var emptyMicroCluster in emptyMicroClusters)
            {
                _microClusters.Remove(emptyMicroCluster);
            }
        }

        public void Remove(string id)
        {
            var allIds = _microClusters.Select(mc => mc.Points.Select(p => p.Id)).Flatten();
            Console.WriteLine($"Trying to remove {id} from cluster map.");
            Console.WriteLine($"Currently held ids are {allIds.Pretty()}");

            var totalRemoved = 0;
            var emptyMicroClusters = new List<UntimedMicroCluster<T>>();
            foreach (var microCluster in _microClusters)
            {
                totalRemoved += microCluster.Points.RemoveAll(p => p.Id.Equals(id));
                if (microCluster.Points.Count == 0)
                {
                    emptyMicroClusters.Add(microCluster);
                }
            }

            if (totalRemoved == 0)
            {
                throw new KeyNotFoundException($"Failed to remove id {id} from cluster map. Currently held ids are {allIds.Pretty()}");
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
            
            _userTerminated = false;  // Reset this in case it was stopped earlier
            var maintainClusterMapThread = Task.Run(() => MaintainClusterMapAsync());  // Run in background thread
            _terminate = () =>
            {
                _userTerminated = true;
                maintainClusterMapThread.Wait();
            };  // Return an action to force the thread to terminate

            return _terminate;
        }

        private async void MaintainClusterMapAsync()
        {
            while (_dataStream != null)
            {
                await Task.Delay(1000); // Spare the CPU a bit
                
                if (_userTerminated)
                    return;
                
                if (_clusteringInProgress)
                    continue;
                
                // Get the next data point in the data stream
                var successfulDequeue = _dataStream.TryDequeue(out var p);
                if (!successfulDequeue)
                    continue;
                
                // Merge the dataPoint into the micro cluster map
                Merge(p);
            }
        }

        public T[][] Cluster()
        {
            if (_userTerminated)
            {
                throw new InvalidOperationException(
                    "Cluster maintenance has been terminated by the user. " +
                    "Cannot cluster without maintenance.");
            }

            // Lock micro cluster map during clustering, force merge the remaining points
            _clusteringInProgress = true; 

            while (!_dataStream.IsEmpty)
            {
                var successfulDequeue = _dataStream.TryDequeue(out var p);
                if (!successfulDequeue)
                    continue;
                
                // Merge the dataPoint into the micro cluster map
                Merge(p);
            }

            try
            {
                // Check if there is less than 10 streamers - if so, just return them
                if (MicroClusters.Count == 1 && MicroClusters.First().Points.Count < 10)
                {
                    return new[] {MicroClusters.First().Points.ToArray() };
                }

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

                _clusteringInProgress = false; // Unlock cluster map
                return clusters.ToArray();
            }
            catch (Exception e)
            {
                // Throw to return to client as a 400
                throw e;
            }
            finally
            {
                _clusteringInProgress = false;
            }
        }

        private void Merge(T p)
        {            
            if (_microClusters.Count == 0)
            {
                // Create a new micro cluster, add to the cluster map
                _microClusters.Add(new UntimedMicroCluster<T>(
                    new [] {p}, _pointSimilarityFunction));
                return;
            }

            // Sort micro clusters by distance to p
            _microClusters.Sort((u, v) =>
                _pointSimilarityFunction(u.Center, p)
                    .CompareTo(_pointSimilarityFunction(v.Center, p)));

            var closestMicroCluster = _microClusters.First();
            
            // Try to insert the point into this cluster if the cluster is close enough
            // and if the cluster radius is small enough after insertion
            var successfulInsert = _pointSimilarityFunction(p, closestMicroCluster.Center) < MAX_RADIUS
                && TryInsert(p, closestMicroCluster, 
                    (mc) => mc.Radius <= MAX_RADIUS);
            

            if (!successfulInsert)
            {
                // Create a new micro cluster, add to the cluster map
                _microClusters.Add(new UntimedMicroCluster<T>(
                    new [] {p}, _pointSimilarityFunction));
            }
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
            Console.WriteLine($"Trying to insert {p.Id} into existing MC containing {cluster.Points.Select(s => s.Id).Pretty()}.");

            cluster.Points.Add(p);

            if (predicate(cluster))
            {
                return true;
            }
            
            Console.WriteLine($"Insertion failed (MC radius became {cluster.Radius}), max is {MAX_RADIUS}. Creating new micro cluster for {p.Id}.");

            cluster.Points.Remove(p);
            return false;
        }

        public string Clear()
        {
            var mcs = _microClusters.Count;
            var dps = _dataStream.Count;
            
            _microClusters.Clear();
            _dataStream.Clear();

            return $"Cleared {mcs} micro clusters and {dps} unclustered points.";
        }

        public string Statistics()
        {
            var numPointsInClusters = _microClusters.Select(mc => mc.Points.Count).Sum();
            var numMcs = _microClusters.Count;
            var numDps = _dataStream.Count;
            return
                $"There are {numMcs} micro clusters, in total {numPointsInClusters} points clustered. " +
                $"There are {numDps} unclustered points." +
                $"State: " +
                $"    _clusteringInProgress: {_clusteringInProgress}" +
                $"    _userTerminated:       {_userTerminated}" +
                $"    _dataStream (isNull)   {_dataStream == null}";
        }
    }
}