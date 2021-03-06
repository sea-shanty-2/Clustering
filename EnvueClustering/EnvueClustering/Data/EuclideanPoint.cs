using System;
using EnvueClustering.ClusteringBase;

namespace EnvueClustering.Data
{
    public class EuclideanPoint : IEuclidean, ITransformable<EuclideanPoint>, IIdentifiable
    {
        public string Id => TimeStamp.ToString();

        public float Radius { get; set; }
        public int TimeStamp { get; set; }

        public float X { get; }

        public float Y { get; }

        public EuclideanPoint(float x, float y, int timeStamp)
        {
            X = x;
            Y = y;
            TimeStamp = timeStamp;
        }

        public EuclideanPoint Scale(float scalar)
        {
            return new EuclideanPoint(scalar * X, scalar * Y, TimeStamp);
        }

        public EuclideanPoint Divide(float scalar)
        {
            return new EuclideanPoint(X / scalar, Y / scalar, TimeStamp);
        }

        public EuclideanPoint Add(EuclideanPoint other)
        {
            return new EuclideanPoint(X + other.X, Y + other.Y, TimeStamp);
        }

        public EuclideanPoint Subtract(EuclideanPoint other)
        {
            return new EuclideanPoint(X - other.X, Y - other.Y, TimeStamp);
        }

        public EuclideanPoint Pow(int power)
        {
            return new EuclideanPoint((float)Math.Pow(X, 2), (float)Math.Pow(Y, 2), TimeStamp);
        }

        public EuclideanPoint Sqrt()
        {
            return new EuclideanPoint((float)Math.Sqrt(X), (float)Math.Sqrt(Y), TimeStamp);
        }

        public float Size()
        {
            return new[] {X, Y}.EuclideanLength();
        }

        public void SetRadius(float val)
        {
            Radius = val;
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }
    }
}