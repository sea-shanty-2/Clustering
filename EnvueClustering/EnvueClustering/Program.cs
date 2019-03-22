using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Xml.Linq;
using EnvueClustering.ClusteringBase;
using EnvueClustering.Data;

namespace EnvueClustering
{
    class Program
    {
        static void Main(string[] args)
        {
            //PCMCTest();
            //DenStreamSyntheticTest();
            ShrinkageClusteringTest();
        }

        static void ShrinkageClusteringTest()
        {
            var simplePoints = new List<EuclideanPoint>()
            {
                new EuclideanPoint(0, 0, 0),
                new EuclideanPoint(1, 0, 0),
                new EuclideanPoint(2, 0, 0),
                new EuclideanPoint(3, 0, 0),
                new EuclideanPoint(4, 0, 0)
            };

            var m = Matrix.SimilarityMatrix(simplePoints,
                (p, o) => (float) Math.Sqrt(Math.Pow(p.X - o.X, 2) + Math.Pow(p.Y - o.Y, 2)),
                normalize: true,
                inverse: true);

            Console.WriteLine(m);

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

        static void DenStreamSyntheticTest()
        {
            const string filePath = "Data/Synthesis/DataSteamGenerator/data.synthetic";
            var dataStream = ContinuousDataReader.ReadSyntheticEuclidean(filePath);

            Func<EuclideanPoint, EuclideanPoint, float> simFunc = (x, y) => 
                (float)Math.Sqrt(Math.Pow(x.X - y.X, 2) + Math.Pow(x.Y - y.Y, 2));
            
            var denStream = new DenStream<EuclideanPoint>();
            denStream.MaintainClusterMap(dataStream, simFunc);
            
            var pcmcLines = new List<string>();
            var ocmcLines = new List<string>();
            var pcmcPointLines = new List<string>();
            var ocmcPointLines = new List<string>();

            foreach (var pcmc in denStream.PotentialCoreMicroClusters)
            {
                var _pcmc = pcmc as CoreMicroCluster<EuclideanPoint>;
                pcmcLines.Add($"{_pcmc.Center(denStream.CurrentTime).X} {_pcmc.Center(denStream.CurrentTime).Y} {_pcmc.Radius(denStream.CurrentTime)}");
            }
            
            foreach (var ocmc in denStream.OutlierCoreMicroClusters)
            {                
                var _ocmc = ocmc as CoreMicroCluster<EuclideanPoint>;
                ocmcLines.Add($"{_ocmc.Center(denStream.CurrentTime).X} {_ocmc.Center(denStream.CurrentTime).Y} {_ocmc.Radius(denStream.CurrentTime)}");
            }

            foreach (var pcmc in denStream.PotentialCoreMicroClusters)
            {
                pcmc.Points.ForEach(p => pcmcPointLines.Add($"{p.X} {p.Y} 2"));
            }
            
            foreach (var ocmc in denStream.OutlierCoreMicroClusters)
            {
                ocmc.Points.ForEach(p => ocmcPointLines.Add($"{p.X} {p.Y} 2"));
            }

            File.WriteAllLines("Data/Synthesis/ClusterVisualization/pcmc", pcmcLines);
            File.WriteAllLines("Data/Synthesis/ClusterVisualization/ocmc", ocmcLines);
            File.WriteAllLines("Data/Synthesis/ClusterVisualization/pcmcPoints", pcmcPointLines);
            File.WriteAllLines("Data/Synthesis/ClusterVisualization/ocmcPoints", ocmcPointLines);
            Console.WriteLine($"Done.");
        }
    }
}