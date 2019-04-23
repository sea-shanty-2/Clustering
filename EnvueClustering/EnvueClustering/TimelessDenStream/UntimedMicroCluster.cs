using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EnvueClustering.ClusteringBase;

namespace EnvueClustering.TimelessDenStream
{
    public class UntimedMicroCluster<T> where T : ITransformable<T>
    {
        private readonly Func<T, T, float> _distanceFunction;
        
        public UntimedMicroCluster(IEnumerable<T> points, Func<T, T, float> distanceFunction)
        {
            Points = new List<T>(points);
            _distanceFunction = distanceFunction;
        }

        public List<T> Points { get; }
        public T Center => Points.ElementWiseSum().Divide(Points.Count);
        public float Radius => Points.Select(p => _distanceFunction(p, Center)).Sum() / Points.Count;
    }
}