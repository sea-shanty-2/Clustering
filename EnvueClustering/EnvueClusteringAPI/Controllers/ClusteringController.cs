using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using EnvueClustering;
using EnvueClustering.ClusteringBase;
using EnvueClustering.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EnvueClusteringAPI.Controllers
{
    [ApiController]
    public class ClusteringController : ControllerBase
    {
        private readonly DenStream<EuclideanPoint> _denStream;
        private readonly IHostingEnvironment _env;
        private Action _terminateClusterMaintenance;
        
        public ClusteringController(IHostingEnvironment env)
        {
            _env = env;
            float SimFunc(EuclideanPoint x, EuclideanPoint y) => 
                (float) Math.Sqrt(Math.Pow(x.X - y.X, 2) + Math.Pow(x.Y - y.Y, 2));


            float CmcSimFunc(CoreMicroCluster<EuclideanPoint> u, CoreMicroCluster<EuclideanPoint> v, int t) => 
                (float) Math.Sqrt(Math.Pow(u.Center(t).X - v.Center(t).X, 2) + Math.Pow(u.Center(t).Y - v.Center(t).Y, 2));

            _denStream = new DenStream<EuclideanPoint>(SimFunc, CmcSimFunc);
        }

        [HttpGet]
        [Route("init-clustering-maintenance")]
        public ActionResult InitClustering()
        {
            try
            {
                _terminateClusterMaintenance = _denStream.MaintainClusterMap();
                return Ok();
            }
            catch (Exception e)
            {
                if (_env.IsDevelopment())
                    return BadRequest(e.Message);
                return BadRequest();
            }
        }

        [HttpPost]
        [Route("add-data-points")]
        public ActionResult AddDataPoints(IEnumerable<EuclideanPoint> points)
        {
            try
            {
                _denStream.AddToDataStream(points);
                return Ok();
            }
            catch (Exception e)
            {
                if (_env.IsDevelopment())
                    return BadRequest(e.Message);
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("terminate-clustering-maintenance")]
        public ActionResult TerminateClusterMaintenance()
        {
            try
            {
                if (_terminateClusterMaintenance == null)
                {
                    return BadRequest(
                        "Cluster maintenance has no termination handler. " +
                        "Verify that cluster maintenance has been initialized.");
                }

                _terminateClusterMaintenance?.Invoke();
                return Ok();
            }
            catch (Exception e)
            {
                if (_env.IsDevelopment())
                    return BadRequest(e.Message);
                return BadRequest();
            }
        }

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