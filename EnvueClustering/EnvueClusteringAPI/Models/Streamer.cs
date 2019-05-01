
using System;
using EnvueClustering;
using EnvueClustering.ClusteringBase;

namespace EnvueClusteringAPI.Models
{
    /// <summary>
    /// Model class for containing information about streamers.
    /// </summary>
    public class Streamer : IGeospatial, ITransformable<Streamer>, IVectorRepresentable<float[]>, IIdentifiable
    {
        public float Longitude { get; set; }

        public float Latitude { get; set; }
        public float[] StreamDescription { get; set; }
        public float[] AsVector() => StreamDescription;

        public string Id { get; }

        public Streamer(float lon, float lat, float[] streamDescription, int timestamp, string id)
        {
            Longitude = lon;
            Latitude = lat;
            StreamDescription = streamDescription;
            TimeStamp = timestamp;
            Id = id;
        }

        public Streamer Scale(float scalar)
        {
            return new Streamer(Longitude * scalar, Latitude * scalar, StreamDescription, TimeStamp, Id);
        }

        public Streamer Divide(float scalar)
        {
            return new Streamer(Longitude / scalar, Latitude / scalar, StreamDescription, TimeStamp, Id);
        }

        public Streamer Add(Streamer other)
        {
            return new Streamer(Longitude + other.Longitude, Latitude + other.Latitude, StreamDescription, TimeStamp, Id);
        }

        public Streamer Subtract(Streamer other)
        {
            return new Streamer(Longitude - other.Longitude, Latitude - other.Latitude, StreamDescription, TimeStamp, Id);
        }

        public Streamer Pow(int power)
        {
            return new Streamer((float) Math.Pow(Longitude, 2), (float)Math.Pow(Longitude, 2), StreamDescription, TimeStamp, Id);
        }

        public Streamer Sqrt()
        {
            return new Streamer((float) Math.Sqrt(Longitude), (float)Math.Sqrt(Longitude), StreamDescription, TimeStamp, Id);
        }

        public float Size()
        {
            // This is only used for CF1 and CF2 calculations in the CMC classes, and we don't use those.
            throw new NotImplementedException();
        }

        public int TimeStamp { get; set; }

        public override string ToString()
        {
            return $"{Id}: ({Latitude}, {Longitude}) --> {StreamDescription.Pretty()}";
        }
    }
}