using System;
using System.Collections.Generic;
using EnvueClustering.ClusteringBase;
using EnvueClustering.Data;
using NUnit.Framework;

namespace EnvueClustering.Tests
{
    [TestFixture]
    public class OutlierCoreMicroClusterTests
    {
        private int _currentTime;
        private EuclideanPoint _pZero;
        private EuclideanPoint _p500;
        private IEnumerable<EuclideanPoint> _fullCluster;
        private IEnumerable<EuclideanPoint> _singletonClusterZero;
        private IEnumerable<EuclideanPoint> _singletonCluster500;
        private Func<EuclideanPoint, EuclideanPoint, float> _simFunc;

        [SetUp]
        public void SetUp()
        {
            _currentTime = 0;
            _pZero = new EuclideanPoint(0, 0, _currentTime);
            _p500 = new EuclideanPoint(500, 500, _currentTime);
            _singletonClusterZero = new [] {_pZero};
            _singletonCluster500 = new [] {_p500};
            _fullCluster = new[] {_pZero, _p500};
            _simFunc = (x, y) => 
                (float)Math.Sqrt(Math.Pow(x.X - y.X, 2) + Math.Pow(x.Y - y.Y, 2));
        }

        [Test]
        public void Radius_SameTime_SameRadius()
        {
            var pcmc = new OutlierCoreMicroCluster<EuclideanPoint>(_fullCluster, _simFunc);
            var r1 = pcmc.Radius(_currentTime);
            var r2 = pcmc.Radius(_currentTime);
            Assert.That(r1, Is.EqualTo(r2));
        }

        [Test]
        public void Radius_TimePassed_SameRadius()
        {
            var pcmc = new OutlierCoreMicroCluster<EuclideanPoint>(_fullCluster, _simFunc);
            var r1 = pcmc.Radius(_currentTime);
            _currentTime++;
            var r2 = pcmc.Radius(_currentTime);
            Assert.That(r1, Is.EqualTo(r2));
        }
        
        [Test]
        public void Weight_SameTime_SameWeight()
        {
            var pcmc = new OutlierCoreMicroCluster<EuclideanPoint>(_fullCluster, _simFunc);
            var w1 = pcmc.Weight(_currentTime);
            var w2 = pcmc.Weight(_currentTime);
            Assert.That(w1, Is.EqualTo(w2));
        }

        [Test]
        public void Weight_TimePassed_SmallerWeight()
        {
            var pcmc = new OutlierCoreMicroCluster<EuclideanPoint>(_fullCluster, _simFunc);
            var w1 = pcmc.Weight(_currentTime);
            _currentTime++;
            var w2 = pcmc.Weight(_currentTime);
            Assert.That(w1, Is.GreaterThan(w2));
        }

        [Test]
        public void Radius_PointsWithDifferentXY_SameRadius()
        {
            var c1 = new OutlierCoreMicroCluster<EuclideanPoint>(_singletonClusterZero, _simFunc);
            var c2 = new OutlierCoreMicroCluster<EuclideanPoint>(_singletonCluster500, _simFunc);
            Assert.That(c1.Radius(_currentTime), Is.EqualTo(c2.Radius(_currentTime)));
        }
    }
}