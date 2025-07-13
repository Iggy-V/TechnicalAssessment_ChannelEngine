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

            var products = await _channelEngine.GetTopProductsAsync();

            // Check if we've already updated the first item in this session
            // So that I automatically set the first item to 25
            if (TempData["UpdatedFirstItem"] == null && products.Any())
            {
                await _orderClient.UpdateStock(products.First(), 25);
                TempData["UpdatedFirstItem"] = true; // Mark as done
            }

            return View(products);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStock(string gtin, int newStock)
        {
            var products = await _channelEngine.GetTopProductsAsync();
            var productToUpdate = products.FirstOrDefault(p => p.Gtin == gtin);

            if (productToUpdate != null)
            {
                await _orderClient.UpdateStock(productToUpdate, newStock);
            }

            return RedirectToAction(nameof(Index));
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
