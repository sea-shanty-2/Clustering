using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using EnvueClustering.ClusteringBase;

namespace EnvueClustering
{
    public class DenStream<T> : IClusterable<T> where T : ITransformable<T>
    {
        public const float LAMBDA = 0.5f;
        public const float EPSILON = 0.5f;
        public const float BETA = 0.5f;
        public const float MU = 0.5f;

        private List<PotentialCoreMicroCluster<T>> PCMCs;
        private List<OutlierCoreMicroCluster<T>> OCMCs;

        /// <summary>
        /// Implements the fading functions as described in the DenStream paper.
        /// </summary>
        /// <param name="t">A floating point value denoting the number of time units passed since the
        /// algorithm began running. </param>
        public static float Fading(float t)
        {
            return (float)Math.Pow(2, LAMBDA * t);
        }

        
        public T[][] Cluster(IEnumerable<(T, float)> dataStream, Func<T, T, float> similarityFunction)
        {
            // TODO: Make a new class inheriting from Stream so we can add to the data stream while the function runs
            var clusters = new List<T[]>();
            var checkInterval = (int)Math.Ceiling((1 / LAMBDA) * Math.Log((BETA * MU) / ((BETA * MU) - 1)));

            while (dataStream.Count() != 0)
            {
                // TODO: Make a GetNext() method for the data stream class.
            }

            return null;
        }

        /// <summary>
        /// Merges a new point p into the cluster map maintained by the DenStream object.
        /// </summary>
        /// <param name="p">The point to insert</param>
        /// <param name="time">The current time (timestamp of the point)</param>
        /// <param name="similarityFunction">Function to determine the distance between two points.</param>
        public void Merge(T p, float time, Func<T, T, float> similarityFunction)
        {
            var successfulInsert = false;

            if (PCMCs.Count() != 0)
            {
                // Find the closest PCMC
                var clustersWithDistance = PCMCs
                    .Select(pcmc => (pcmc, similarityFunction(p, pcmc.Center(time))))
                    .ToList();
                
                clustersWithDistance.Sort((x, y) => x.Item2.CompareTo(y.Item2));
                var (cluster, distance) = clustersWithDistance.First();
                
                // Try to insert considering the new radius
                successfulInsert = TryInsert(p, cluster, 
                    newCluster => newCluster.Radius(time) <= EPSILON);
            }

            if (!successfulInsert && OCMCs.Count() != 0)
            {
                // Find the closest OCMC
                var clustersWithDistance = OCMCs
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
                        OCMCs.Remove(cluster);
                        PCMCs.Add(new PotentialCoreMicroCluster<T>(
                            cluster.Points, 
                            cluster.TimeStamps, 
                            similarityFunction));
                    }
                }
            }

            if (!successfulInsert)
            {
                // Create a new OCMC
                OCMCs.Add(new OutlierCoreMicroCluster<T>(
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
    }
}