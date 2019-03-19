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

        
        public float Cluster(IEnumerable<T> dataStream, Func<T, T, float> similarityFunction)
        {
            throw new NotImplementedException();
        }

        public void Merge(T p, float time, Func<T, T, float> similarityFunction)
        {
            // Find the PCMC closest to p
            //     Try insert
            // Find the OCMC closest to p
            //     Try insert
            // Create new OCMC 
            //     Insert

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