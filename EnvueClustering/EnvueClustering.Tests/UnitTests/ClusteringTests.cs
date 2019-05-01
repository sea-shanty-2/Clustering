using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnvueClustering.ClusteringBase;
using EnvueClustering.TimelessDenStream;
using EnvueClusteringAPI.Models;
using Newtonsoft.Json;
using NUnit.Framework;

namespace EnvueClustering.Tests
{
    [TestFixture]
    public class ClusteringTests
    {
        
        TimelessDenStream<Streamer> DENSTREAM = new TimelessDenStream<Streamer>(Similarity.Haversine, Similarity.Haversine);

        [SetUp]
        public void SetUp()
        {
            DENSTREAM.Terminate();
            DENSTREAM.Clear();
            DENSTREAM.MaintainClusterMap();
        }

        [Test]
        public void SomeTest()
        {
            dynamic jsonArr =
                JsonConvert.DeserializeObject(
                    File.ReadAllText($"../../../shrinkage_empty_test.json"));

            var streamers = new List<Streamer>();
            foreach (var s in jsonArr)
            {
                var id = (string)s.id;
                var lat = (float)s.latitude;
                var lon = (float)s.longitude;
                var timestamp = (int)s.timeStamp;
                var streamDescription = new [] {1.2f, 1.4f, 1.5f};
                
                streamers.Add(new Streamer(lon, lat, streamDescription, timestamp, id));
            }
            
            foreach (var streamer in streamers)
            {
                DENSTREAM.Add(streamer);
            }
            
            
            var clusters = DENSTREAM.Cluster();
            DENSTREAM.Terminate();
            Assert.That(clusters, Is.Not.Empty);
        }

        [Test]
        public void Cluster_SameTwoPoints_OneCluster()
        {
            // [{"longitude":10.0,"latitude":20.0,"streamDescription":[0.0,1.0,0.0],"id":"Test","timeStamp":0}]
            var streamers = new[]
            {
                new Streamer(10.0f, 20.0f, new [] {0.0f, 1.0f, 0.0f}, 0, "Test"),
                new Streamer(10.0f, 20.0f, new [] {0.0f, 1.0f, 0.0f}, 0, "Test")
            };
            
            DENSTREAM.Add(streamers);
            var clusters = DENSTREAM.Cluster();
            Assert.That(clusters, Has.Exactly(1).Items);
        }
        
        [Test]
        public void Cluster_SameTwoPoints_OneMicroCluster()
        {
            // [{"longitude":10.0,"latitude":20.0,"streamDescription":[0.0,1.0,0.0],"id":"Test","timeStamp":0}]
            var streamers = new[]
            {
                new Streamer(10.0f, 20.0f, new [] {0.0f, 1.0f, 0.0f}, 0, "Test"),
                new Streamer(10.0f, 20.0f, new [] {0.0f, 1.0f, 0.0f}, 0, "Test")
            };
            
            DENSTREAM.Add(streamers);
            var mcs = DENSTREAM.MicroClusters;
            Assert.That(mcs, Has.Exactly(1).Items);
        }

        [Test]
        public void Cluster_TwoDifferentPoints_NonEmptyClusters()
        {
            var streamers = new[]
            {
                new Streamer(9.99f, 57.046707f, new[] {1.2f, 1.4f, 1.5f}, 0, "mikkel1"),
                new Streamer(9.79f, 57.036707f, new[] {1.2f, 1.4f, 1.5f}, 0, "mikkel2")
            };
            
            DENSTREAM.Add(streamers[0]);
            Thread.Sleep(1000);
            Assert.That(DENSTREAM.MicroClusters, Has.Exactly(1).Items);
            Assert.That(DENSTREAM.MicroClusters[0].Points, Has.Exactly(1).Items);
            DENSTREAM.Add(streamers[1]);
            Thread.Sleep(1000);
            Assert.That(DENSTREAM.MicroClusters, Has.Exactly(2).Items);
            Assert.That(DENSTREAM.MicroClusters[0].Points, Has.Exactly(1).Items);

            var clusters = DENSTREAM.Cluster();
            Assert.That(clusters, Is.Not.Empty);
            foreach (var cluster in clusters)
            {
                Assert.That(cluster, Is.Not.Empty);
            }
            
            var SHRINKAGE = new ShrinkageClustering<Streamer>(100, 100, Similarity.Haversine);

            foreach (var cluster in clusters)
            {
                var scCluster = SHRINKAGE.Cluster(cluster);
                Assert.That(scCluster, Is.Not.Empty);
            }

        }
    }
}

