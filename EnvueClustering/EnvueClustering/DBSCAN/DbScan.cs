using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using EnvueClustering.ClusteringBase;
using EnvueClustering.Data;

namespace EnvueClustering.DBSCAN
{
    public class DbScan<T> : IClusterable<T>
    {
        private List<T[]> clusters = new List<T[]>();
        private List<T> points;
        private List<T> corePoints;
        private readonly Func<T, T, float> SimFunc;
        private readonly int minPts;
        private readonly float eps;

        public DbScan(int minPts, float eps, Func<T, T, float> similarityFunction)
        {
            this.minPts = minPts;
            this.eps = eps;
            SimFunc = similarityFunction;
        }

        public T[][] Cluster(IEnumerable<T> dataStream)
        {
            if (dataStream == null)
            {
                return null;
            }

            points = dataStream as List<T>;
            corePoints = points.Where(x => points.Count(y => eps > SimFunc(x, y)) >= minPts).ToList();

            int clusterIndex = 0;
            foreach (var corePoint in corePoints)
            {
                clusters[clusterIndex] = points.Where(x => DirectlyReachable(x, corePoint)).ToArray();
                clusterIndex++;
            }

            return clusters.ToArray();
        }
        
        public bool DirectlyReachable(T origin, T destination)
        {
            if (!corePoints.Contains(destination))
            {
                return false;
            }
            
            return eps > SimFunc(origin, destination);
        }

        public bool DensityReachable(T origin, T destination)
        {
            T currentPoint = origin;
            
            for (int i = 0; i < corePoints.Count; i++)
            {
                if (DirectlyReachable(currentPoint, corePoints[i]))
                {
                    if (corePoints[i].Equals(destination))
                    {
                        return true;
                    }

                    currentPoint = corePoints[i];
                    i = 0;
                }

                i++;
            }

            return false;
        }
    }
}