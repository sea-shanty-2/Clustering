
using System;
using EnvueClustering.ClusteringBase;

namespace EnvueClusteringAPI.Models
{
    /// <summary>
    /// Model class for containing information about streamers.
    /// </summary>
    public class Streamer : ITransformable<Streamer>
    {
        private int _started;

        public float Longitude { get; }

        public float Latitude { get; }

        public Guid Id { get; }

        public Streamer(float lon, float lat, int timestamp, Guid id)
        {
            Longitude = lon;
            Latitude = lat;
            _started = timestamp;
            Id = id;
        }

        public Streamer Scale(float scalar)
        {
            return new Streamer(Longitude * scalar, Latitude * scalar, TimeStamp, Id);
        }

        public Streamer Divide(float scalar)
        {
            return new Streamer(Longitude / scalar, Latitude / scalar, TimeStamp, Id);
        }

        public Streamer Add(Streamer other)
        {
            return new Streamer(Longitude + other.Longitude, Latitude + other.Latitude, TimeStamp, Id);
        }

        public Streamer Subtract(Streamer other)
        {
            return new Streamer(Longitude - other.Longitude, Latitude - other.Latitude, TimeStamp, Id);
        }

        public Streamer Pow(int power)
        {
            return new Streamer((float) Math.Pow(Longitude, 2), (float)Math.Pow(Longitude, 2), TimeStamp, Id);
        }

        public Streamer Sqrt()
        {
            return new Streamer((float) Math.Sqrt(Longitude), (float)Math.Sqrt(Longitude), TimeStamp, Id);
        }

        public float Size()
        {
            // This is only used for CF1 and CF2 calculations in the CMC classes, and we don't use those.
            throw new NotImplementedException();
        }

        public int TimeStamp
        {
            get => _started;
            set => _started = value;
        }
    }
}