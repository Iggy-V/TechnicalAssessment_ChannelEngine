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
        private readonly IOrderClient _orderClient;


        public HomeController(ILogger<HomeController> logger, ChannelEngineInterface channelEngine, IOrderClient orderClient)
        {
            _logger = logger;
            _channelEngine = channelEngine;
            _orderClient = orderClient;

        }



        public async Task<IActionResult> Index()
        {
            //Console.WriteLine(_apiKey); // Log the API key to the console for debugging purposes

            var orders = await _channelEngine.GetTopProductsAsync();
            await _orderClient.UpdateStock(orders.First(), 10);
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
