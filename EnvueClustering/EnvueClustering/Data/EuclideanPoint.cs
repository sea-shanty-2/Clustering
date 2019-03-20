using System;
using EnvueClustering.ClusteringBase;

namespace EnvueClustering.Data
{
    public class EuclideanPoint : ITransformable<EuclideanPoint>
    {
        private int _timeStamp;

        public float X { get; }

        public float Y { get; }

        public EuclideanPoint(float x, float y, int timeStamp)
        {
            X = x;
            Y = y;
            _timeStamp = timeStamp;
        }

        public EuclideanPoint Scale(float scalar)
        {
            return new EuclideanPoint(scalar * X, scalar * Y, _timeStamp);
        }

        public EuclideanPoint Divide(float scalar)
        {
            return new EuclideanPoint(X / scalar, Y / scalar, _timeStamp);
        }

        public EuclideanPoint Add(EuclideanPoint other)
        {
            return new EuclideanPoint(X + other.X, Y + other.Y, _timeStamp);
        }

        public EuclideanPoint Subtract(EuclideanPoint other)
        {
            return new EuclideanPoint(X - other.X, Y - other.Y, _timeStamp);
        }

        public EuclideanPoint Pow(int power)
        {
            return new EuclideanPoint((float)Math.Pow(X, 2), (float)Math.Pow(Y, 2), _timeStamp);
        }

        public EuclideanPoint Sqrt()
        {
            return new EuclideanPoint((float)Math.Sqrt(X), (float)Math.Sqrt(Y), _timeStamp);
        }

        public float Size()
        {
            return new[] {X, Y}.EuclideanLength();
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }
    }
}