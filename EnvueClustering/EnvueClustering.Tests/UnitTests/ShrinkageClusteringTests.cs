using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using EnvueClustering.ClusteringBase;
using EnvueClustering.Data;
using EnvueClustering.TimelessDenStream;
using EnvueClusteringAPI.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace EnvueClustering.Tests
{
    [TestFixture]
    public class ShrinkageClusteringTests
    {
        private IClusterable<Streamer> SC = new ShrinkageClustering<Streamer>(100, 1000, Similarity.Cosine);
        [Test]
        public void Cluster_SevenBroadcasters_NotEmptyClusters()
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

            var clusters = SC.Cluster(streamers);
            Assert.That(clusters, Is.Not.Empty);
        }

        [Test]
        public void Cluster_EuclideanTargetPoints_ThreeClusters()
        {
            // ONLY RUN THIS IF WE WANT TO WAIT FOR 2.5 MINUTES
            
            
//            var points = File.ReadAllLines("../../../target.json").Select(line =>
//            {
//                var attrs = line.Split(' ');
//                return new EuclideanPoint(float.Parse(attrs[0]), float.Parse(attrs[1]), 0);
//            });
//            
//            var sc = new ShrinkageClustering<EuclideanPoint>(35, 1000, Similarity.EuclideanDistance);
//            
//            Assert.That(sc.Cluster(points), Has.Exactly(3).Items);
        }

        [Test]
        public void Cluster_OneOutlier_TwoClusters()
        {
            dynamic jsonArr =
                JsonConvert.DeserializeObject(
                    File.ReadAllText($"../../../two_clusters_test.json")); 
            
            var streamers = new List<Streamer>();
            foreach (var s in jsonArr)
            {
                var id = (string)s.id;
                var lat = (float)s.latitude;
                var lon = (float)s.longitude;
                var timestamp = (int)s.timeStamp;
                var streamDescription = (s.streamDescription as JArray).ToObject<float[]>();
                
                streamers.Add(new Streamer(lon, lat, streamDescription, timestamp, id));
            }

            var clusters = SC.Cluster(streamers);

            Assert.That(clusters, Has.Exactly(2).Items);
        }
    }
}