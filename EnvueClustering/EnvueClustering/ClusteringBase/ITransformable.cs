namespace EnvueClustering.ClusteringBase
{
    public interface ITransformable<T>
    {
        T Scale(float scalar);
        T Divide(float scalar);
        T Add(T other);
        T Minus(T other);

        T Pow(int power);
    }
}