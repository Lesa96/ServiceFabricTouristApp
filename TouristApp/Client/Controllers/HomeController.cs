using Client.Models;
using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Remoting.Client;
//using Microsoft.ServiceFabric.Services.Communication.Wcf;
//using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using System.Diagnostics;
using System.Fabric;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Client;
using Common.Model;

namespace Client.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : Controller
    {
        private const string fabricService = "fabric:/TouristApp/";
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("get")]
        //http://localhost:9031/home/get
        public async Task<IActionResult> Index()
        {
            _logger.Log(LogLevel.Information, "Get method was called");

            List<Recommendation> recomendations = new List<Recommendation>();

            var statelessProxy = Microsoft.ServiceFabric.Services.Remoting.Client.ServiceProxy.Create<IRecommendationService>(
                new Uri(fabricService + "RecommendationService"), new ServicePartitionKey(0));
            recomendations = await statelessProxy.GetRecommendations();
            return View(recomendations);
        }

        [HttpPost]
        [Route("post")]
        public async Task AddRecommendation([FromBody]Recommendation recommendation)
        {
            _logger.Log(LogLevel.Information, "Post method was called");

            var statelessProxy = ServiceProxy.Create<IRecommendationService>(
                new Uri(fabricService + "RecommendationService"), new ServicePartitionKey(0));

            await statelessProxy.AddRecomendation(recommendation);

        }

    }
}