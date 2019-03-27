using System;
using System.Collections.Generic;
using System.IO;
using EnvueClustering.ClusteringBase;
using EnvueClustering.Data;
using Newtonsoft.Json;
using EnvueClustering.DBSCAN;

namespace EnvueClustering
{
    class Program
    {
        private static void Main(string[] args)
        {
            //PCMCTest();
            //DenStreamSyntheticTest();
            DbScanSyntheticTest();
        }

        static void PCMCTest()
        {
            var points = new List<EuclideanPoint>()
            {
                new EuclideanPoint(250, 300, 1),
                new EuclideanPoint(300, 250, 1),
                new EuclideanPoint(300, 350, 1),
                new EuclideanPoint(350, 300, 1)
            };

            var pcmc = new PotentialCoreMicroCluster<EuclideanPoint>(
                points, 
                (x, y) => 
                    (float)Math.Sqrt(Math.Pow(x.X - y.X, 2) + Math.Pow(x.Y - y.Y, 2)));

            Console.WriteLine($"Time 1");
            Console.WriteLine($"Center is {pcmc.Center(1)}");
            Console.WriteLine($"Radius is {pcmc.Radius(1)}");
            Console.WriteLine($"Weight is {pcmc.Weight(1)}");
            Console.WriteLine();
            Console.WriteLine($"Time 100");
            Console.WriteLine($"Center is {pcmc.Center(100)}");
            Console.WriteLine($"Radius is {pcmc.Radius(100)}");
            Console.WriteLine($"Weight is {pcmc.Weight(100)}");
            Console.WriteLine();
            Console.WriteLine($"Time 500");
            Console.WriteLine($"Center is {pcmc.Center(500)}");
            Console.WriteLine($"Radius is {pcmc.Radius(500)}");
            Console.WriteLine($"Weight is {pcmc.Weight(500)}");
            
        }
        
        private static void DbScanSyntheticTest()
        {
            Console.WriteLine("Performing Synthetic Test for DBSCAN..");
            const string filePath = "Data/Synthesis/DataSteamGenerator/data.synthetic.json";
            var dataStream = ContinuousDataReader.ReadSyntheticEuclidean(filePath);

            float DenSimFunc(EuclideanPoint x, EuclideanPoint y) =>
                (float) Math.Sqrt(Math.Pow(x.X - y.X, 2) + Math.Pow(x.Y - y.Y, 2));
            
            float DbSimFunc(CoreMicroCluster<EuclideanPoint> x, CoreMicroCluster<EuclideanPoint> y) =>
                (float) Math.Sqrt(Math.Pow(x.Center(x.Points[x.Points.Count - 1].TimeStamp).X - y.Center(y.Points[y.Points.Count - 1].TimeStamp).X, 2) +
                                  Math.Pow(x.Center(x.Points[x.Points.Count - 1].TimeStamp).Y - y.Center(y.Points[y.Points.Count - 1].TimeStamp).Y, 2));

            var denStream = new DenStream<EuclideanPoint>(DenSimFunc);
            denStream.MaintainClusterMap(dataStream);
            var inputStream = new List<CoreMicroCluster<EuclideanPoint>>(denStream.PotentialCoreMicroClusters);

            Console.WriteLine($"Clustering {inputStream.Count} data points");
            
            var dbScan = new DbScan<CoreMicroCluster<EuclideanPoint>>(2, 300, DbSimFunc);
            CoreMicroCluster<EuclideanPoint>[][] clusters = dbScan.Cluster(inputStream);

            Console.WriteLine($"Result: {clusters.Length} clusters");
            
            File.WriteAllText("Data/Synthesis/ClusterVisualization/dbscan.json", JsonConvert.SerializeObject(clusters));

            Console.WriteLine("Done.");
        }

        private static void DenStreamSyntheticTest()
        {
            const string filePath = "Data/Synthesis/DataSteamGenerator/data.synthetic.json";
            var dataStream = ContinuousDataReader.ReadSyntheticEuclidean(filePath);

            Func<EuclideanPoint, EuclideanPoint, float> simFunc = (x, y) => 
                (float)Math.Sqrt(Math.Pow(x.X - y.X, 2) + Math.Pow(x.Y - y.Y, 2));
            
            var denStream = new DenStream<EuclideanPoint>(simFunc);
            denStream.MaintainClusterMap(dataStream);
            
            var pcmcs = new List<EuclideanPoint>();
            var ocmcs = new List<EuclideanPoint>();
            var pcmcPoints = new List<EuclideanPoint>();
            var ocmcPoints = new List<EuclideanPoint>();

            foreach (var pcmc in denStream.PotentialCoreMicroClusters)
            {
                var _pcmc = pcmc as CoreMicroCluster<EuclideanPoint>;
                var p = new EuclideanPoint(_pcmc.Center(denStream.CurrentTime).X, _pcmc.Center(denStream.CurrentTime).Y,
                    (int) _pcmc.Radius(denStream.CurrentTime));
                p.SetRadius(_pcmc.Radius(denStream.CurrentTime));
                pcmcs.Add(p);
            }
            
            foreach (var ocmc in denStream.OutlierCoreMicroClusters)
            {                
                var _ocmc = ocmc as CoreMicroCluster<EuclideanPoint>;
                var p = new EuclideanPoint(_ocmc.Center(denStream.CurrentTime).X, _ocmc.Center(denStream.CurrentTime).Y,
                    (int) _ocmc.Radius(denStream.CurrentTime));
                p.SetRadius(_ocmc.Radius(denStream.CurrentTime));
                ocmcs.Add(p);
            }

            foreach (var pcmc in denStream.PotentialCoreMicroClusters)
            {
                pcmc.Points.ForEach(p =>
                {
                    var ep = new EuclideanPoint(p.X, p.Y, 2);
                    ep.SetRadius(2);
                    pcmcPoints.Add(ep);
                });
            }
            
            foreach (var ocmc in denStream.OutlierCoreMicroClusters)
            {
                ocmc.Points.ForEach(p =>
                {
                    var ep = new EuclideanPoint(p.X, p.Y, 2);
                    ep.SetRadius(2);
                    ocmcPoints.Add(ep);
                });
            }

            File.WriteAllText("Data/Synthesis/ClusterVisualization/pcmcs.json", JsonConvert.SerializeObject(pcmcs));
            File.WriteAllText("Data/Synthesis/ClusterVisualization/ocmcs.json", JsonConvert.SerializeObject(ocmcs));
            File.WriteAllText("Data/Synthesis/ClusterVisualization/pcmcPoints.json", JsonConvert.SerializeObject(pcmcPoints));
            File.WriteAllText("Data/Synthesis/ClusterVisualization/ocmcPoints.json", JsonConvert.SerializeObject(ocmcPoints));

            Console.WriteLine($"Wrote {pcmcs.Count} PCMCs and {ocmcs.Count} OCMCs to disk.");
        }
    }
}