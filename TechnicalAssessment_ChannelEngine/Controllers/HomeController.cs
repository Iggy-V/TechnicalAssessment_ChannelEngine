using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using TechnicalAssessment_ChannelEngine.Models;
using TechnicalAssessment_ChannelEngine.Services;

namespace TechnicalAssessment_ChannelEngine.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        //private readonly string _apiKey;
        private readonly ChannelEngineInterface _channelEngine;


        public HomeController(ILogger<HomeController> logger, ChannelEngineInterface channelEngine)
        {
            _logger = logger;
            _channelEngine = channelEngine;

        }



        public async Task<IActionResult> Index()
        {
            //Console.WriteLine(_apiKey); // Log the API key to the console for debugging purposes

            var orders = await _channelEngine.GetTopProductsAsync();
            return View(orders);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
