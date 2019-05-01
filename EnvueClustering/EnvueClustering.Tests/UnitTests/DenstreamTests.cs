using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using EnvueClustering.ClusteringBase;
using EnvueClustering.Exceptions;
using EnvueClustering.TimelessDenStream;
using EnvueClusteringAPI.Models;
using NUnit.Framework;

namespace EnvueClustering.Tests
{
    [TestFixture]
    public class DenstreamTests
    {
        private TimelessDenStream<Streamer> _denStream;

        [SetUp]
        public void SetupDenstream()
        {
            _denStream = new TimelessDenStream<Streamer>(
                Similarity.Haversine, 
                Similarity.Haversine);
        }
        
        [Test]
        public void AddEnumerable_SameTwoPoints_OneMicroCluster()
        {
            List<Streamer> streamers = new List<Streamer>
            {
                new Streamer(10, 20, new float[] {1, 0, 1}, 0, "Test"),
                new Streamer(10, 20, new float[] {1, 0, 1}, 0, "Test")
            };

            _denStream.Add(streamers);
            
            Thread.Sleep(1000);
            Assert.AreEqual(1, _denStream.MicroClusters.Count);
        }

        [Test]
        public void AddEnumerable_CloseTwoPoints_OneMicroCluster()
        {
            List<Streamer> streamers = new List<Streamer>
            {
                new Streamer(10, 20, new float[] {1, 0, 1}, 0, "Test"),
                new Streamer(10, 22, new float[] {1, 0, 1}, 1, "Test2")
            };

            _denStream.Add(streamers);
            
            Thread.Sleep(1000);
            Assert.AreEqual(1, _denStream.MicroClusters.Count);
        }
        
        [Test]
        public void AddEnumerable_DifferentTwoPoints_TwoMicroClusters()
        {
            List<Streamer> streamers = new List<Streamer>();
            streamers.Add(new Streamer(10, 20, new float[]{1, 0, 1}, 0, "Test"));
            streamers.Add(new Streamer(0, 100, new float[]{0, 1, 1}, 1, "Test2"));
            
            _denStream.Add(streamers);
            
            Thread.Sleep(1000);
            Assert.AreEqual(2, _denStream.MicroClusters.Count);
        }
        
        [Test]
        public void AddEnumerable_AddOnePoint_OneMicroCluster()
        {
            List<Streamer> streamers = new List<Streamer> {new Streamer(10, 20, new float[] {1, 0, 1}, 0, "Test")};
            
            _denStream.Add(streamers);

            Thread.Sleep(1000);
            Assert.AreEqual(1, _denStream.MicroClusters.Count);
        }

        [Test]
        public void AddSingle_AddOnePoint_OneMicroCluster()
        {
            _denStream.Add(new Streamer(10, 20, new float[]{1, 0, 1}, 0, "Test"));
            
            Thread.Sleep(1000);
            Assert.AreEqual(1, _denStream.MicroClusters.Count);
        }
    }
}