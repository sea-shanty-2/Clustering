using System;
using System.Collections.Generic;
using System.IO;
using EnvueClustering.ClusteringBase;
using EnvueClustering.Data;
using Newtonsoft.Json;
using NUnit.Framework;

namespace EnvueClustering.Tests
{
    [TestFixture]
    public class DbScanTests
    {
        [Test]
        public void Cluster_SameInputAsSyntheticTest_SameResultAsSyntheticTest()
        {
            string filePath = $"{Environment.CurrentDirectory}/Data/Synthesis/DataSteamGenerator/data.synthetic.json";
            string correctPath = $"{Environment.CurrentDirectory}/Data/Synthesis/Unittests/dbscan.json";
            var dataStream = ContinuousDataReader.ReadSyntheticEuclidean(filePath);
            var correctResult = JsonConvert.DeserializeObject(File.ReadAllText(correctPath));

            Func<EuclideanPoint, EuclideanPoint, float> simFunc = (x, y) => 
                (float)Math.Sqrt(Math.Pow(x.X - y.X, 2) + Math.Pow(x.Y - y.Y, 2));


            Func<CoreMicroCluster<EuclideanPoint>, CoreMicroCluster<EuclideanPoint>, int, float> cmcSimFunc = (u, v, t) =>
                (float) Math.Sqrt(Math.Pow(u.Center(t).X - v.Center(t).X, 2) +
                                  Math.Pow(u.Center(t).Y - v.Center(t).Y, 2));
            
            var denStream = new DenStream<EuclideanPoint>(simFunc, cmcSimFunc);
            denStream.SetDataStream(dataStream);
            denStream.MaintainClusterMap();

            var clusters = denStream.Cluster();

            var clusterPoints = new List<dynamic>();
            foreach (var (i, cluster) in clusters.Enumerate())
            {
                foreach (var point in cluster)
                {
                    clusterPoints.Add(new {x = point.X, y = point.Y, c = i});
                }
            }

            Assert.Equals(clusterPoints, correctResult);
        }
    }
}