
using System;

namespace EnvueClusteringAPI.Models
{
    /// <summary>
    /// Model class for containing information about streamers.
    /// </summary>
    public class Streamer
    {
        private float _lon, _lat;
        private int _started;
        private Guid _id;

        public Streamer(float lon, float lat, int timestamp, Guid id)
        {
            _lon = lon;
            _lat = lat;
            _started = timestamp;
            _id = id;
        }
    }
}