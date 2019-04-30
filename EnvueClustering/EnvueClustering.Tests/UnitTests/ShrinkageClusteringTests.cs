using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using EnvueClustering.ClusteringBase;
using EnvueClusteringAPI.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace EnvueClustering.Tests
{
    [TestFixture]
    public class ShrinkageClusteringTests
    {
        private IClusterable<Streamer> SC = new ShrinkageClustering<Streamer>(100, 100, Similarity.Cosine);
        [Test]
        public void SevenBroadcasters_Clustering_NotEmptyClusters()
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

            Console.WriteLine(streamers);

            var clusters = SC.Cluster(streamers);
            Assert.That(clusters, Is.Not.Empty);

        }
    }
}