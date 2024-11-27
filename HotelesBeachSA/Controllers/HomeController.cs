using HotelesBeachSA.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

//Json Library
using Newtonsoft.Json;

namespace HotelesBeachSA.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _client;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _client = httpClientFactory.CreateClient("ReservacionesHttpClient");
        }

        public async Task<IActionResult> Index()
        {
            //Obtener paquetes para mostralos en el Index
            var paquetesResponse = await _client.GetAsync("Paquetes/ListadoCompleto");
            var paquetes = new List<Paquete>();

            if (paquetesResponse.IsSuccessStatusCode) { 
                var paquetesData = await paquetesResponse.Content.ReadAsStringAsync();
                paquetes = JsonConvert.DeserializeObject<List<Paquete>>(paquetesData);
            }

            var rolUsuario = HttpContext.Session.GetString("rolUsuario");

            if (rolUsuario != null && rolUsuario == "admin") {
                return RedirectToAction("AdminIndex","Home");
            }
            return View(paquetes);
        }

        public IActionResult AdminIndex() 
        {
            return View();    
        }

        public IActionResult AboutUs()
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
