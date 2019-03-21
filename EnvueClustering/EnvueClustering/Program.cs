using System;
using System.Collections.Generic;
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
            DenStreamSyntheticTest();
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

            foreach (var pcmc in denStream.PotentialCoreMicroClusters)
            {
                var _pcmc = pcmc as CoreMicroCluster<EuclideanPoint>;
                Console.WriteLine($"{_pcmc.Center(denStream.CurrentTime).X} {_pcmc.Center(denStream.CurrentTime).Y} {_pcmc.Radius(denStream.CurrentTime)}");
            }
        }
    }
}