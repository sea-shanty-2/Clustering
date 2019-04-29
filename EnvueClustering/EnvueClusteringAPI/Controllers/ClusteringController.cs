using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using EnvueClustering;
using EnvueClustering.ClusteringBase;
using EnvueClustering.Data;
using EnvueClustering.TimelessDenStream;
using EnvueClusteringAPI.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EnvueClusteringAPI.Controllers
{
    /// <summary>
    /// API endpoints for controlling the clustering of streams in Envue and querying streams clusters (event streams).
    /// </summary>
    [ApiController]
    public class ClusteringController : ControllerBase
    {
        private readonly TimelessDenStream<Streamer> _denStream;
        private readonly IClusterable<Streamer> _shrinkageClustering;
        private readonly IHostingEnvironment _env;
        
        public ClusteringController(IHostingEnvironment env, TimelessDenStream<Streamer> denStream)
        {
            _env = env;
            _denStream = denStream;
            _shrinkageClustering = new ShrinkageClustering<Streamer>(100, 100, 
                Similarity.Cosine);
            
            
            
        }

        /// <summary>
        /// Initializes the micro-cluster maintenance procedure in the density-based clustering module.
        /// Will run until the program is terminated. Can be terminated manually with the "clustering/terminate" endpoint.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("clustering/init")]
        public ActionResult InitClustering()
        {
            try
            {
                _denStream.MaintainClusterMap();
                return Ok();
            }
            catch (Exception e)
            {
                if (_env.IsDevelopment())
                    return BadRequest(e.Message);
                return BadRequest();
            }
        }

        /// <summary>
        /// Adds a range of data objects to the data stream being clustering by the clustering module.
        /// Does not require that the micro-cluster maintenance procedure has been initialized.
        /// </summary>
        /// <param name="streamers"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("data/add")]
        public ActionResult AddDataPoints(IEnumerable<Streamer> streamers)
        {
            try
            {
                _denStream.Add(streamers);
                return Ok();
            }
            catch (Exception e)
            {
                if (_env.IsDevelopment())
                    return BadRequest(e.Message);
                return BadRequest();
            }
        }

        /// <summary>
        /// Removes the data point from the cluster map and reinserts it in order
        /// to reflect a change in position.
        /// </summary>
        /// <param name="streamer"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("data/update")]
        public ActionResult UpdateDataPoint(Streamer streamer)
        {
            try
            {
                _denStream.Update(streamer);
                return Ok();
            }
            catch (Exception e)
            {
                if (_env.IsDevelopment())
                    return BadRequest(e.Message);
                return BadRequest();
            }
        }

        /// <summary>
        /// Terminates the micro-cluster maintenance procedure.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("clustering/terminate")]
        public ActionResult TerminateClusterMaintenance()
        {
            try
            {
                _denStream.Terminate();
                return Ok();
            }
            catch (Exception e)
            {
                if (_env.IsDevelopment())
                    return BadRequest(e.Message);
                return BadRequest();
            }
        }

        /// <summary>
        /// Returns clusters of Streamer objects that represent event clusters.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("clustering/events")]
        public ActionResult GetClusters()
        {
            try
            {
                var eventClusters = new List<Streamer[]>();
                
                // Cluster on geographical positions
                var geoClusters = _denStream.Cluster();
                
                // Cluster on the stream descriptions
                foreach (var geoCluster in geoClusters)
                    eventClusters.AddRange(_shrinkageClustering.Cluster(geoCluster));
                
                return Ok(eventClusters);
                
            }
            catch (Exception e)
            {
                if (_env.IsDevelopment())
                    return BadRequest(e.Message);
                return BadRequest();
            }
        }

        /// <summary>
        /// Returns the clusters generated by dummy data points in euclidean space (see report for the visuals).
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("test")]
        public ActionResult Test()
        {
            var filePath = $"{Directory.GetCurrentDirectory()}/Data/data.synthetic.json";
            var dataStream = ContinuousDataReader.ReadSyntheticEuclidean(filePath);

            Func<EuclideanPoint, EuclideanPoint, float> simFunc = (x, y) => 
                (float)Math.Sqrt(Math.Pow(x.X - y.X, 2) + Math.Pow(x.Y - y.Y, 2));


            Func<CoreMicroCluster<EuclideanPoint>, CoreMicroCluster<EuclideanPoint>, int, float> cmcSimFunc = (u, v, t) =>
                (float) Math.Sqrt(Math.Pow(u.Center(t).X - v.Center(t).X, 2) +
                                  Math.Pow(u.Center(t).Y - v.Center(t).Y, 2));
            
            var denStream = new DenStream<EuclideanPoint>(simFunc, cmcSimFunc);
            denStream.AddToDataStream(dataStream);
            var terminate = denStream.MaintainClusterMap();
            Thread.Sleep(5000);
            var clusters = denStream.Cluster();

            var clusterPoints = new List<dynamic>();
            foreach (var (i, cluster) in clusters.Enumerate())
            {
                foreach (var point in cluster)
                {
                    clusterPoints.Add(new {x = point.X, y = point.Y, c = i});
                }
            }

            Console.WriteLine($"Waiting...");
            Thread.Sleep(2000);
            Console.WriteLine($"Terminating MaintainClusterMap");
            terminate();

            return Ok(JsonConvert.SerializeObject(clusters));
        }
    }
}