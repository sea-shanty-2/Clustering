using System;
using System.Collections.Generic;
using EnvueClustering.ClusteringBase;

namespace EnvueClustering.DBSCAN
{
    public class DbScan<T> : IClusterable<T>
    {
        private List<List<T>> clusters = new List<List<T>>();
        private int minPts = 2;
        private float eps = 3;

        public T[][] Cluster(IEnumerable<T> dataStream, Func<T, T, float> similarityFunction)
        {
            if (dataStream == null)
            {
                return null;
            }
            
            List<T> visitedPoints = new List<T>();
            List<T> corePoints = new List<T>();
            int clusterIndex = 0;

            foreach (T point in dataStream)
            {
                if (visitedPoints.Contains(point))
                    continue;
                visitedPoints.Add(point);
                
                List<T> nearbyPoints = RegionQuery(point, dataStream as List<T>, corePoints,
                    similarityFunction);
                if (nearbyPoints.Count < minPts)
                {
                    continue;
                }
                else
                {
                    corePoints.Add(point);
                    clusterIndex++;
                    ExpandCluster(clusters[clusterIndex], point, nearbyPoints, visitedPoints,
                        dataStream as List<T>, corePoints, similarityFunction);
                }
            }

            List<T[]> returnClusters = new List<T[]>();
            foreach (List<T> cluster in clusters)
            {
                returnClusters.Add(cluster.ToArray());
            }
            return returnClusters.ToArray();
        }

        private void ExpandCluster(List<T> cluster, T point, List<T> nearbyPoints, List<T> visitedPoints,
            List<T> allPoints, List<T> corePoints, Func<T, T, float> similarityFunction)
        {
            if (cluster == null)
            {
                cluster = new List<T>();
            }
            
            cluster.Add(point);
            
            List<T> additionalPoints = new List<T>();
            bool additionalPointsAdded = false;

            foreach (T nearbyPoint in nearbyPoints)
            {
                if (!visitedPoints.Contains(nearbyPoint))
                {
                    visitedPoints.Add(nearbyPoint);
                    List<T> nearby = RegionQuery(nearbyPoint, allPoints, corePoints, similarityFunction);
                    if (nearby.Count >= minPts)
                    {
                        additionalPointsAdded = true;
                        additionalPoints.AddRange(nearby);
                    }
                    if (!InCluster(nearbyPoint))
                    {
                        cluster.Add(nearbyPoint);
                    }
                }
            }

            while (additionalPointsAdded)
            {
                additionalPointsAdded = false;
                List<T> moreAdditionalPoints = new List<T>();
                
                foreach (T nearbyPoint in nearbyPoints)
                {
                    if (!visitedPoints.Contains(nearbyPoint))
                    {
                        visitedPoints.Add(nearbyPoint);
                        List<T> nearby = RegionQuery(nearbyPoint, allPoints, corePoints, similarityFunction);
                        if (nearby.Count >= minPts)
                        {
                            additionalPointsAdded = true;
                            moreAdditionalPoints.AddRange(nearby);
                        }

                        if (!InCluster(nearbyPoint))
                        {
                            cluster.Add(nearbyPoint);
                        }
                    }
                }

                if (additionalPointsAdded)
                {
                    additionalPoints = new List<T>();
                    additionalPointsAdded = false;
                    
                    foreach (T nearbyPoint in nearbyPoints)
                    {
                        if (!visitedPoints.Contains(nearbyPoint))
                        {
                            visitedPoints.Add(nearbyPoint);
                            List<T> nearby = RegionQuery(nearbyPoint, allPoints, corePoints, similarityFunction);
                            if (nearby.Count >= minPts)
                            {
                                additionalPointsAdded = true;
                                additionalPoints.AddRange(nearby);
                            }
                            if (!InCluster(nearbyPoint))
                            {
                                cluster.Add(nearbyPoint);
                            }
                        }
                    }
                }
            }
        }

        private bool InCluster(T nearbyPoint)
        {
            foreach (List<T> cluster in clusters)
            {
                if (cluster.Contains(nearbyPoint))
                {
                    return true;
                }
            }

            return false;
        }

        private List<T> RegionQuery(T point, List<T> allPoints, List<T> corePoints, Func<T, T, float> similarityFunction)
        {
            List<T> nearbyPoints = new List<T>();

            foreach (T otherPoint in allPoints)
            {
                if (DensityReachable(point, otherPoint, corePoints, similarityFunction))
                {
                    nearbyPoints.Add(otherPoint);
                }
            }

            return nearbyPoints;
        }


        private bool DirectlyReachable(T p1, T p2, Func<T, T, float> similarityFunction)
        {
            return eps < similarityFunction(p1, p2);
        }

        private bool DensityReachable(T p1, T p2, List<T> corePoints, Func<T, T, float> similarityFunction)
        {
            if (DirectlyReachable(p1, p2, similarityFunction))
            {
                return true;
            }
            
            foreach (T corePoint in corePoints)
            {
                if (DirectlyReachable(p1, corePoint, similarityFunction))
                {
                    List<T> newCorePoints = corePoints;
                    newCorePoints.Remove(corePoint);
                    return DensityReachable(corePoint, p2, newCorePoints, similarityFunction);
                }
            }
            return false;
        }
    }
}