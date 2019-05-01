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
    }
}