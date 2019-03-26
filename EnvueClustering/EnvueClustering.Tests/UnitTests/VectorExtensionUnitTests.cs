using System;
using EnvueClustering.Data;
using NUnit.Framework;

namespace EnvueClustering.Tests
{
    [TestFixture]
    public class VectorExtensionUnitTests
    {
        [Test]
        public void Sum_SimpleList_CorrectSum()
        {
            var arr = new[] {1f, 2, 3, 4, 5, 0};
            var expected = 1 + 2 + 3 + 4 + 5 + 0;
            Assert.That(arr.Sum(), Is.EqualTo(expected));
        }

        [Test]
        public void Sum_EmptyList_RaisedException()
        {
            var arr = new float[] { };
            Assert.That(arr.Sum, Throws.ArgumentException);
        }

        [Test]
        public void EuclideanLength_SimpleList_CorrectLength()
        {
            var arr = new[] {2f, 3};
            var expected = (float) Math.Sqrt(Math.Pow(2, 2) + Math.Pow(3, 2));
            Assert.That(arr.EuclideanLength(), Is.EqualTo(expected));
        }

        [Test]
        public void EuclideanLength_EmptyList_RaisedException()
        {
            var arr = new float[] { };
            Assert.That(arr.EuclideanLength, Throws.ArgumentException);
        }

        [Test]
        public void Scale_SimplePointList_DoublyScaled()
        {
            var arr = new[]
            {
                new EuclideanPoint(1, 2, 0), 
                new EuclideanPoint(3, 4, 0)
            };

            var expected = new[]
            {
                new EuclideanPoint(2, 4, 0),
                new EuclideanPoint(6, 8, 0)
            };

            var actual = arr.Scale(2);
            foreach (var (p1, p2) in expected.Zip(actual))
            {
                Assert.That(p1.X, Is.EqualTo(p2.X));
                Assert.That(p1.Y, Is.EqualTo(p2.Y));
            }
        }
    }
}