using System;
using System.Collections.Generic;
using System.Linq;
using EnvueClustering.Data;
using NUnit.Framework;

namespace EnvueClustering.Tests
{
    [TestFixture]
    public class MatrixTests
    {
        [Test]
        public void SimilarityMatrix_SimplePoints_Correct()
        {
            var simplePoints = new List<EuclideanPoint>()
            {
                new EuclideanPoint(0, 0, 0),
                new EuclideanPoint(1, 0, 0),
                new EuclideanPoint(2, 0, 0),
                new EuclideanPoint(3, 0, 0),
                new EuclideanPoint(4, 0, 0)
            };

            var expected = new []
            {
                new [] {0, 0, 0, 0, 0}, 
                new [] {1, 0, 0, 0, 0}, 
                new [] {2, 1, 0, 0, 0}, 
                new [] {3, 2, 1, 0, 0}, 
                new [] {4, 3, 2, 1, 0}
            };

            var m = Matrix.SimilarityMatrix(simplePoints,
                (p, o) => (float) Math.Sqrt(Math.Pow(p.X - o.X, 2) + Math.Pow(p.Y - o.Y, 2)));

            foreach (var (row, expectedRow) in m.Zip(expected, (r, e) => (r, e)))
                Assert.That(row, Is.EquivalentTo(expectedRow));
        }
    }
}