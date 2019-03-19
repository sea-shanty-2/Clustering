using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using EnvueClustering.ClusteringBase;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace EnvueClustering
{
    public class DenStream<T> : IClusterable<T> where T : ITransformable<T>
    {
        public const float LAMBDA = 0.5f;
        public const float EPSILON = 0.5f;
        public const float BETA = 0.5f;
        public const float MU = 0.5f;

        private List<PotentialCoreMicroCluster<T>> Pcmcs;
        private List<OutlierCoreMicroCluster<T>> Ocmcs;

        private IClusterable<CoreMicroCluster<T>> DbScan;

        /// <summary>
        /// Implements the fading functions as described in the DenStream paper.
        /// </summary>
        /// <param name="t">A floating point value denoting the number of time units passed since the
        /// algorithm began running. </param>
        public static float Fading(float t)
        {
            return (float)Math.Pow(2, LAMBDA * t);
        }

        public async void MaintainClusterMap(
            IEnumerable<(T, int)> dataStream, 
            Func<T, T, float> similarityFunction)
        {
            // TODO: Make a new class inheriting from Stream so we can add to the data stream while the function runs
            // For now, we will use a concurrent queue.
            var clusters = new List<T[]>();
            var queuedStream = dataStream as ConcurrentQueue<(T, int)>;
            var checkInterval = (int) Math.Ceiling((1 / LAMBDA) * Math.Log(
                                                        (BETA * MU) /
                                                       ((BETA * MU) - 1)));

            while (queuedStream != null && !queuedStream.IsEmpty)
            {
                var successfulDequeue = queuedStream.TryDequeue(out var dataPoint);
                if (!successfulDequeue)
                    continue;

                var (p, t) = dataPoint;
                
                // Merge p into the cluster map
                Merge(p, t, similarityFunction);
                
                if (t % checkInterval != 0) continue;
                
                foreach (var pcmc in Pcmcs)
                {
                    if (pcmc.Weight(t) < BETA * MU)
                        Pcmcs.Remove(pcmc);
                }

                foreach (var ocmc in Ocmcs)
                {
                    var threshold = GetXiThreshold(ocmc.CreationTime, t, checkInterval);
                    if (ocmc.Weight(t) < threshold)
                        Ocmcs.Remove(ocmc);
                }
            }
        }

        public T[][] Cluster(IEnumerable<(T, int)> dataStream, Func<T, T, float> similarityFunction)
        {
            // Apply DBSCAN to PCMC.
            throw new NotImplementedException();
        }

        /// <summary>
        /// Merges a new point p into the cluster map maintained by the DenStream object.
        /// </summary>
        /// <param name="p">The point to insert</param>
        /// <param name="time">The current time (timestamp of the point)</param>
        /// <param name="similarityFunction">Function to determine the distance between two points.</param>
        public void Merge(T p, int time, Func<T, T, float> similarityFunction)
        {
            var successfulInsert = false;

            if (Pcmcs.Count() != 0)
            {
                // Find the closest PCMC
                var clustersWithDistance = Pcmcs
                    .Select(pcmc => (pcmc, similarityFunction(p, pcmc.Center(time))))
                    .ToList();
                
                clustersWithDistance.Sort((x, y) => x.Item2.CompareTo(y.Item2));
                var (cluster, distance) = clustersWithDistance.First();
                
                // Try to insert considering the new radius
                successfulInsert = TryInsert(p, cluster, 
                    newCluster => newCluster.Radius(time) <= EPSILON);
            }

            if (!successfulInsert && Ocmcs.Count() != 0)
            {
                // Find the closest OCMC
                var clustersWithDistance = Ocmcs
                    .Select(pcmc => (pcmc, similarityFunction(p, pcmc.Center(time))))
                    .ToList();
                
                clustersWithDistance.Sort((x, y) => x.Item2.CompareTo(y.Item2));
                var (cluster, distance) = clustersWithDistance.First();
                
                // Try to insert considering the new radius
                successfulInsert = TryInsert(p, cluster, 
                    newCluster => newCluster.Radius(time) <= EPSILON);
                
                if (successfulInsert)
                {
                    // Check the weight
                    if (cluster.Weight(time) > BETA * MU)
                    {
                        // Convert this OCMC into a new PCMC
                        Ocmcs.Remove(cluster);
                        Pcmcs.Add(new PotentialCoreMicroCluster<T>(
                            cluster.Points, 
                            cluster.TimeStamps, 
                            similarityFunction));
                    }
                }
            }

            if (!successfulInsert)
            {
                // Create a new OCMC
                Ocmcs.Add(new OutlierCoreMicroCluster<T>(
                    new [] {p}, 
                    new [] {time}, 
                    similarityFunction));
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