using System;
using EnvueClustering.Data;
using EnvueClustering.DBSCAN;
using NUnit.Framework;

namespace EnvueClustering.Tests
{
    [TestFixture]
    public class DbScanTests
    {
        [Test]
        public void DirectlyReachable_DirectlyReachablePoints_True()
        {
            var p1 = new EuclideanPoint(0, 0, 100);
            var p2 = new EuclideanPoint(0, 1, 101);

            float SimFunc(EuclideanPoint x, EuclideanPoint y) =>
                (float) Math.Sqrt(Math.Pow(x.X - y.X, 2) + Math.Pow(x.Y - y.Y, 2));

            var dbScan = new DbScan<EuclideanPoint>(2, 3, SimFunc);
            Assert.True(dbScan.DirectlyReachable(p1, p2));
        }

        [Test]
        public void DensityReachable_DirectlyReachablePoints_True()
        {
            var p1 = new EuclideanPoint(0, 0, 100);
            var p2 = new EuclideanPoint(0, 1, 101);

            float SimFunc(EuclideanPoint x, EuclideanPoint y) =>
                (float) Math.Sqrt(Math.Pow(x.X - y.X, 2) + Math.Pow(x.Y - y.Y, 2));

            var dbScan = new DbScan<EuclideanPoint>(2, 3, SimFunc);
            Assert.True(dbScan.DensityReachable(p1, p2));
        }
        
        [Test]
        public void DensityReachable_DensityReachablePoints_True()
        {
            var p1 = new EuclideanPoint(0, 0, 100);
            var p2 = new EuclideanPoint(0, 1, 101);

            float SimFunc(EuclideanPoint x, EuclideanPoint y) =>
                (float) Math.Sqrt(Math.Pow(x.X - y.X, 2) + Math.Pow(x.Y - y.Y, 2));

            var dbScan = new DbScan<EuclideanPoint>(2, 3, SimFunc);
            Assert.True(dbScan.DirectlyReachable(p1, p2));
        }
    }
}