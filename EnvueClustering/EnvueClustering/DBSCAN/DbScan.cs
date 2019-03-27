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
        private readonly List<T[]> _clusters = new List<T[]>();
        private readonly List<T> _visitedPoints = new List<T>();
        private readonly Func<T, T, float> _simFunc;
        private readonly int _minPts;
        private readonly float _eps;
        private List<T> _points;

        public DbScan(int minPts, float eps, Func<T, T, float> similarityFunction)
        {
            _minPts = minPts;
            _eps = eps;
            _simFunc = similarityFunction;
        }

        public T[][] Cluster(IEnumerable<T> dataStream)
        {
            if (dataStream == null)
            {
                return null;
            }

            _points = dataStream as List<T>;
            foreach (var dataPoint in _points)
            {
                _visitedPoints.Add(dataPoint);

                var neighborPts = new Queue<T>(RegionQuery(dataPoint));
                if (neighborPts.Count >= _minPts)
                {
                    _clusters.Add(ExpandCluster(dataPoint, neighborPts));
                }
            }

            return _clusters.ToArray();
        }

        private T[] ExpandCluster(T point, Queue<T> neighborPts)
        {
            List<T> cluster = new List<T>() {point};

            while (neighborPts.Any())
            {
                T pt = neighborPts.Dequeue();
                
                if (!_visitedPoints.Contains(pt))
                {
                    _visitedPoints.Add(pt);
                    var ptNeighborPts = new List<T>(RegionQuery(pt));

                    if (ptNeighborPts.Count >= _minPts)
                        ptNeighborPts.ForEach(neighborPts.Enqueue);
                }
                if (!cluster.Contains(pt) && !IsInCluster(pt))
                    cluster.Add(pt);
            }
                
            return cluster.ToArray();
        }

        private bool IsInCluster(T point)
        {
            foreach (var cluster in _clusters)
            {
                if (cluster.Contains(point))
                {
                    return true;
                }
            }

            return false;
        }

        private IEnumerable<T> RegionQuery(T point)
        {
            return _points.Where(x => _eps > _simFunc(point, x));
        }
    }
}