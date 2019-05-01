using System.Collections.Generic;
using System.IO;
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
            
            TimelessDenStream<Streamer> ds = new TimelessDenStream<Streamer>(Similarity.Haversine, Similarity.Haversine);
            foreach (var streamer in streamers)
            {
                ds.Add(streamer);
            }
            
            
            var clusters = ds.Cluster();
            Assert.That(clusters, Is.Not.Empty);
        }

        [Test]
        public void Cluster_SameTwoPoints_OneCluster()
        {
            var ds = new TimelessDenStream<Streamer>(Similarity.Haversine, Similarity.Haversine);
            // [{"longitude":10.0,"latitude":20.0,"streamDescription":[0.0,1.0,0.0],"id":"Test","timeStamp":0}]
            var streamers = new[]
            {
                new Streamer(10.0f, 20.0f, new [] {0.0f, 1.0f, 0.0f}, 0, "Test"),
                new Streamer(10.0f, 20.0f, new [] {0.0f, 1.0f, 0.0f}, 0, "Test")
            };
            
            ds.Add(streamers);
            var clusters = ds.Cluster();
            Assert.That(clusters, Has.Exactly(1).Items);
        }
        
        [Test]
        public void Cluster_SameTwoPoints_OneMicroCluster()
        {
            var ds = new TimelessDenStream<Streamer>(Similarity.Haversine, Similarity.Haversine);
            // [{"longitude":10.0,"latitude":20.0,"streamDescription":[0.0,1.0,0.0],"id":"Test","timeStamp":0}]
            var streamers = new[]
            {
                new Streamer(10.0f, 20.0f, new [] {0.0f, 1.0f, 0.0f}, 0, "Test"),
                new Streamer(10.0f, 20.0f, new [] {0.0f, 1.0f, 0.0f}, 0, "Test")
            };
            
            ds.Add(streamers);
            var mcs = ds.MicroClusters;
            Assert.That(mcs, Has.Exactly(1).Items);
        }
    }
}