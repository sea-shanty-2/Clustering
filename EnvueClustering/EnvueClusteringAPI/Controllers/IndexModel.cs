using System;
using System.Collections.Generic;
using EnvueClustering;
using EnvueClustering.ClusteringBase;
using EnvueClustering.TimelessDenStream;
using EnvueClusteringAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EnvueClusteringAPI.Controllers
{
    public class StatefulDenStream : PageModel
    {
        private TimelessDenStream<Streamer> _denStream;
        private IClusterable<Streamer> _shrinkageClustering;

        public StatefulDenStream(TimelessDenStream<Streamer> denStream)
        {
            _denStream = denStream;
            _shrinkageClustering = new ShrinkageClustering<Streamer>(100, 100,
                Similarity.Cosine);
        }

        public void AddDataPoints(IEnumerable<Streamer> streamers)
        {
            _denStream.Add(streamers);            
        }
        
        public void UpdateDataPoint(Streamer streamer)
        {
            _denStream.Update(streamer);
        }
        
        public void TerminateClusterMaintenance()
        {
            _denStream.Terminate();
        }
        
        public Streamer[][] GetClusters()
        {
            var eventClusters = new List<Streamer[]>();
                
            // Cluster on geographical positions
            var geoClusters = _denStream.Cluster();
                
            // Cluster on the stream descriptions
            foreach (var geoCluster in geoClusters)
                eventClusters.AddRange(_shrinkageClustering.Cluster(geoCluster));

            return eventClusters.ToArray();
        }

    }
}