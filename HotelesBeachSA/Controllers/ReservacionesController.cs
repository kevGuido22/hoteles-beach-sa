using HotelesBeachSA.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace HotelesBeachSA.Controllers
{
    public class ReservacionesController : Controller
    {
        private readonly HttpClient _client;

        public ReservacionesController(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient("ReservacionesHttpClient");
        }
        public async Task<IActionResult> Index()
        {
            // Llama al endpoint para obtener todas las reservaciones
            HttpResponseMessage response = await _client.GetAsync("Reservacion/Listado");

            if (response.IsSuccessStatusCode)
            {
                var resultado = await response.Content.ReadAsStringAsync();
                var reservaciones = JsonConvert.DeserializeObject<List<Reservacion>>(resultado);

                return View(reservaciones);
            }

            TempData["Error"] = "No se pudo cargar el listado de reservaciones.";
            return View(new List<Reservacion>());
        }
        public IActionResult Create()
        {
            return View();
        }
    }
}
