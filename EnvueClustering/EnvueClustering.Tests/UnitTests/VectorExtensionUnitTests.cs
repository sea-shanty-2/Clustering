using System;
using System.Linq;
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

        [Test]
        public void Scale_EmptyPointList_EmptyListReturned()
        {
            var arr = new float[] { };
            Assert.That(arr.Scale(1), Is.EquivalentTo(new float[] { }));
        }

        [Test]
        public void Scale_SimpleFloatList_AllElementsZero()
        {
            var arr = new[] {2f, 3, 4, 5, 6, 0};
            Assert.That(arr.Scale(0), Has.Exactly(6).Items.EqualTo(0));
        }

        [Test]
        public void Divide_SimpleFloatList_Halved()
        {
            var arr = new[] {2f, 3, 4, 5};
            var expected = new[] {2f / 2, 3f / 2, 4f / 2, 5f / 2};
            Assert.That(arr.Divide(2), Is.EquivalentTo(expected));
        }

        [Test]
        public void Divide_DivideByZero_RaisedException()
        {
            var arr = new[] {2f, 3, 4, 5};
            Assert.That(() => { arr.Divide(0); }, Throws.ArgumentException);
        }

        [Test]
        public void ElementWiseSum_SimplePointList_SinglePoint()
        {
            var arr = new[]
            {
                new EuclideanPoint(1, 2, 0), 
                new EuclideanPoint(3, 4, 0)
            };
            
            var expected = new EuclideanPoint(4, 6, 0);
            var actual = arr.ElementWiseSum();
            Assert.That(actual.X, Is.EqualTo(expected.X));
            Assert.That(actual.Y, Is.EqualTo(expected.Y));
        }

        [Test]
        public void Slice_SimpleFloatList_CorrectSlice()
        {
            var arr = new[] {1, 2, 3, 4, 5, 6};
            var expected = new[] {1, 2, 3};
            var actual = arr.Slice(0, 3);
            Assert.That(actual.Length, Is.EqualTo(3));
            Assert.That(actual, Is.EquivalentTo(expected));
        }

        [Test]
        public void Slice_LengthOutOfBounds_RemainingSlice()
        {
            var arr = new[] {1, 2, 3, 4, 5, 6};
            var expected = new[] {5, 6};
            var actual = arr.Slice(4, 100);
            Assert.That(actual.Length, Is.EqualTo(2));
            Assert.That(actual, Is.EquivalentTo(expected));
        }

        [Test]
        public void Range_TenPointList_RangeToNine()
        {
            var expected = new[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9};
            var actual = 10.Range().ToList();
            Assert.That(actual.Count, Is.EqualTo(10));
            Assert.That(actual, Has.None.GreaterThan(9));
            Assert.That(actual, Is.EquivalentTo(expected));
        }

        [Test]
        public void Range_Zero_EmptyList()
        {
            var actual = 0.Range().ToList();
            Assert.That(actual, Is.Empty);
        }

        [TestCase(5, 10)]
        [TestCase(-5, 5)]
        [TestCase(-5, 1)]
        [TestCase(5, 0)]
        [TestCase(0, 1)]
        public void Repeat_RepeatNTimesK_5Times10(int n, int k)
        {
            var actual = n.Repeat(k);
            Assert.That(actual, Has.Exactly(k).Items.EqualTo(n));
        }

        [Test]
        public void Enumerate_SimpleList_CorrectlyEnumerated()
        {
            var actual = 5.Repeat(10).Enumerate().ToList();
            for (var i = 0; i < 10; i++)
            {
                var (index, value) = actual[i];
                Assert.That(index, Is.EqualTo(i));
                Assert.That(actual, Has.Exactly(10).Items);
            }
        }

        [Test]
        public void Enumerate_EmptyList_ReturnsEmptyList()
        {
            var actual = new float[] { }.Enumerate();
            Assert.That(actual, Is.Empty);
        }

        [Test]
        public void ArgMin_SimpleFloatList_CorrectIndex()
        {
            var arr = new [] {1f, 2, 3, 0, 3, 2};
            Assert.That(arr.ArgMin(), Is.EqualTo(3));
        }

        [Test]
        public void ArgMin_EmptyList_RaisedException()
        {
            var arr = new float[] { };
            Assert.That(arr.ArgMin, Throws.ArgumentException);
        }

        [Test]
        public void ArgMax_SimpleFloatList_CorrectIndex()
        {
            var arr = new [] {1f, 2, 3, 9, 3, 2};
            Assert.That(arr.ArgMax(), Is.EqualTo(3));
        }
        
        [Test]
        public void ArgMax_EmptyList_RaisedException()
        {
            var arr = new float[] { };
            Assert.That(arr.ArgMax, Throws.ArgumentException);
        }
    }
}