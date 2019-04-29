using EnvueClustering;
using EnvueClustering.TimelessDenStream;
using EnvueClusteringAPI.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EnvueClusteringAPI.Controllers
{
    public class IndexModel : PageModel
    {
        public TimelessDenStream<Streamer> DenStream;

        public IndexModel(TimelessDenStream<Streamer> denStream)
        {
            DenStream = denStream;
        }
    }
}