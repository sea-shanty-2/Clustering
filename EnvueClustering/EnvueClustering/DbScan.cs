using System;
using System.Collections.Generic;
using System.Linq;
using EnvueClustering.ClusteringBase;
using EnvueClustering.Exceptions;

namespace EnvueClustering
{
    public class DbScan<T> : IClusterable<T>
    {
        private readonly float _eps;
        private readonly int _minPts, _time;
        private readonly Func<T, T, int, float> _similarityFunction;

        /// <summary>
        /// Initialize a DBSCAN clustering algorithm
        /// </summary>
        /// <param name="eps">The epsilon value defining the radius required for points to be considered nearby</param>
        /// <param name="minPts">The minimum amount of points required within a points eps in order for it to be a core-point</param>
        /// <param name="time">The time to evaluate the centers of potential microclusters</param>
        /// <param name="similarityFunction">The function used to compare distance between points</param>
        /// <exception cref="EnvueArgumentException">Throws EnvueArgumentException if invalid input is received</exception>
        public DbScan(float eps, int minPts, int time, Func<T, T, int, float> similarityFunction)
        {
            if (eps < 0)
                throw new EnvueArgumentException("Epsilon cannot be less than zero.");
            if (minPts < 2) 
                throw new EnvueArgumentException("Minimum number of points in clusters cannot be less than 2.");
            
            _eps = eps;
            _time = time;
            _minPts = minPts;
            _similarityFunction = similarityFunction;
        }

        /// <summary>
        /// Get all points within "point"'s eps-neighbourhood
        /// </summary>
        /// <param name="point">The center point of the neighbourhood</param>
        /// <param name="dataSet">The data set from which to retrieve points</param>
        /// <returns>An array of points in radius eps of center point "point"</returns>
        private T[] GetNeighbours(T point, IEnumerable<T> dataSet)
        {
            return dataSet.Where(other => _similarityFunction(point, other, _time) <= _eps).ToArray();
        }

        /// <summary>
        /// Performs DBSCAN clustering on "dataStream"
        /// </summary>
        /// <param name="dataStream">Enumerable list of points to cluster</param>
        /// <returns>An array containing clusters based on "dataStream"</returns>
        /// <exception cref="EnvueArgumentException">Throws EnvueArgumentException if invalid input is recieved</exception>
        public T[][] Cluster(IEnumerable<T> dataStream)
        {
            var dataArr = dataStream.ToArray();
            if (dataArr == null)
                throw new EnvueArgumentException("Data stream provided was null.");
            if (dataArr.Length == 0)
                throw new EnvueArgumentException("Data stream is empty.");
            if (dataArr.Count() < 2)
                throw new EnvueArgumentException("Data stream only contains two elements, i.e. only one cluster.");

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
                        // This is a potential cluster. 
                        // For every point in this neighbourhood, merge/union *their*
                        // neighbours.
                        var i = 0;
                        while (neighbours.Length > i)
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
                        
                        // We processed all neighbours (also the ones we found along the way). 
                        // Add this cluster to the final cluster set.
                        clusters.Add(neighbours);
                    }
                }
                k++;
            }
            return clusters.ToArray();
        }
    }
}