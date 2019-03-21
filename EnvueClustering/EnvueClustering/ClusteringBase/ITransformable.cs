namespace EnvueClustering.ClusteringBase
{
    public interface ITransformable<T>
    {
        /// <summary>
        /// Scale all elements in the data point with a scalar.
        /// </summary>
        /// <param name="scalar"></param>
        /// <returns></returns>
        T Scale(float scalar);
        
        /// <summary>
        /// Divide all elements in the data point with a scalar.
        /// </summary>
        /// <param name="scalar"></param>
        /// <returns></returns>
        T Divide(float scalar);
        
        /// <summary>
        /// Add two data points together in an element-wise manner.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        T Add(T other);
        
        /// <summary>
        /// Subtract two data points from each other in an element-wise manner.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        T Subtract(T other);

        /// <summary>
        /// Apply a power operation to all elements in a data point.
        /// </summary>
        /// <param name="power"></param>
        /// <returns></returns>
        T Pow(int power);
        
        /// <summary>
        /// Take the square root of all elements in a data point.
        /// </summary>
        /// <returns></returns>
        T Sqrt();

        /// <summary>
        /// Return the size (|A|) of the data point.
        /// </summary>
        /// <returns></returns>
        float Size();

        /// <summary>
        /// The timestamp of a point. 
        /// </summary>
        int TimeStamp { get; set; }
    }
}