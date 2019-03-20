using System;
using System.Reflection.Metadata.Ecma335;
using System.Xml.Linq;
using EnvueClustering.Data;

namespace EnvueClustering
{
    class Program
    {
        static void Main(string[] args)
        {
            DenStreamSyntheticTest();
        }

        static void DenStreamSyntheticTest()
        {
            const string filePath = "Data/Synthesis/data.synthetic";
            var dataStream = ContinuousDataReader.ReadSyntheticEuclidean(filePath);

            Func<EuclideanPoint, EuclideanPoint, float> simFunc = (x, y) => 
                (float)Math.Sqrt(Math.Pow(x.X - y.X, 2) + Math.Pow(x.Y - y.Y, 2));
            
            var denStream = new DenStream<EuclideanPoint>();
            denStream.MaintainClusterMap(dataStream, simFunc);
        }
    }
}