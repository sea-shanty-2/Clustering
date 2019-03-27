using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using EnvueClustering.ClusteringBase;

namespace EnvueClustering
{
    public class DbScan<T> : IClusterable<T>
    {
        private readonly float _eps;
        private readonly int _minPts, _time;
        private readonly Func<T, T, int, float> _similarityFunction;
        
        public DbScan(float eps, int minPts, int time, Func<T, T, int, float> similarityFunction)
        {
            if (eps < 0)
                throw new ArgumentException("Epsilon cannot be less than zero.");
            if (minPts < 2) 
                throw new ArgumentException("Minimum number of points in clusters cannot be less than 2.");
            
            _eps = eps;
            _time = time;
            _minPts = minPts;
            _similarityFunction = similarityFunction;
        }

        private T[] GetNeighbours(T point, IEnumerable<T> dataSet)
        {
            return dataSet.Where(other => _similarityFunction(point, other, _time) <= _eps).ToArray();
        }

        

        public T[][] Cluster(IEnumerable<T> dataStream)
        {
            var dataArr = dataStream.ToArray();
            if (dataArr == null)
                throw new ArgumentException("Data stream provided was null.");
            if (dataArr.Length == 0)
                throw new ArgumentException("Data stream is empty.");
            if (dataArr.Count() < 2)
                throw new ArgumentException("Data stream only contains two elements, i.e. only one cluster.");

            var visitedPoints = new List<T>();
            var clusters = new List<T[]>();

            var k = 0;
            while (dataArr.Length > k)
            {
                var p = dataArr[k];
                if (!visitedPoints.Contains(p))
                {
                    visitedPoints.Add(p);
                    var neighbours = GetNeighbours(p, dataArr);

                    if (neighbours.Length >= _minPts)
                    {
                        var i = 0;
                        while (neighbours.Count() > i)
                        {
                            var neighbour = neighbours[i];
                            if (!visitedPoints.Contains(neighbour))
                            {
                                visitedPoints.Add(neighbour);
                                var individualNeighbours = GetNeighbours(neighbour, dataArr);
                                if (individualNeighbours.Length > _minPts)
                                {
                                    neighbours = neighbours.Union(individualNeighbours).ToArray();
                                }
                            }
                            i++;
                        }
                        clusters.Add(neighbours);
                    }
                }
                k++;
            }
            return clusters.ToArray();
        }
    }
}