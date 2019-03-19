using System;
using System.Collections.Generic;
using EnvueClustering.ClusteringBase;

namespace EnvueClustering
{
    public class DenStream<T> : IClusterable<T>
    {
        public const float LAMBDA = 0.5f;
        public const float EPSILON = 0.5f;
        
        
        
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

        public void Merge(T p)
        {
            // Find the PCMC closest to p
            //     Try insert
            // Find the OCMC closest to p
            //     Try insert
            // Create new OCMC 
            //     Insert
            
            
        }
    }
}