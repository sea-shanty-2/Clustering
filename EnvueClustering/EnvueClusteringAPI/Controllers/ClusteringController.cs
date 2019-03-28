using System;
using EnvueClustering;
using EnvueClustering.ClusteringBase;
using EnvueClustering.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace EnvueClusteringAPI.Controllers
{
    [ApiController]
    public class ClusteringController : ControllerBase
    {
        private readonly DenStream<EuclideanPoint> _denStream;
        private Action _terminateClusterMaintenance;
        private IHostingEnvironment _env;
        
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
        [Route("initClusterMaintenance")]
        public ActionResult InitClustering()
        {
            try
            {
                _denStream.SetDataStream(new EuclideanPoint[] {});
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

        [HttpGet]
        [Route("terminateClusterMaintenance")]
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
    }
}