using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using HotelesBeachSA.Models;
using System.Text;

namespace HotelesBeachSA.Controllers
{
    public class PaquetesController : Controller
    {
        private readonly HttpClient _client;

        public PaquetesController(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient("ReservacionesHttpClient");
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            HttpResponseMessage response = await _client.GetAsync("Paquetes/ListadoCompleto");

            if (response.IsSuccessStatusCode)
            {
                var resultado = await response.Content.ReadAsStringAsync();
                var paquetes = JsonConvert.DeserializeObject<List<Paquete>>(resultado);

                return View(paquetes);
            }

            TempData["Error"] = "No se pudo cargar el listado de paquetes.";
            return View(new List<Paquete>());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind] Paquete paquete)
        {
            //la id es autoincrementable
            paquete.Id = 0;

            var response = await _client.PostAsJsonAsync("Paquetes/Crear", paquete);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();

                TempData["Exito"] = "El paquete se registró correctamente.";
                return RedirectToAction("Index");
            }

            TempData["Error"] = "No se pudo registrar el paquete. Por favor, verifica los datos e inténtalo nuevamente.";
            return View(paquete);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            HttpResponseMessage response = await _client.GetAsync($"Paquetes/Buscar?id={id}");

            if (response.IsSuccessStatusCode)
            {
                var resultado = await response.Content.ReadAsStringAsync();
                var paquete = JsonConvert.DeserializeObject<Paquete>(resultado);

                return View(paquete);
            }

            TempData["Error"] = "No se pudo cargar el paquete.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind] Paquete paquete)
        {
            var response = await _client.PutAsJsonAsync("Paquetes/Editar", paquete);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();

                TempData["Exito"] = "El paquete se actualizó correctamente.";
                return RedirectToAction("Index");
            }

            TempData["Error"] = "No se pudo actualizar el paquete. Por favor, verifica los datos e inténtalo nuevamente.";
            return View(paquete);
        }

        [HttpGet]
        public async Task<IActionResult> CambiarHabilitado(int id)
        {
            HttpResponseMessage response = await _client.GetAsync($"Paquetes/Buscar?id={id}");

            if (response.IsSuccessStatusCode)
            {
                var resultado = await response.Content.ReadAsStringAsync();
                var paquete = JsonConvert.DeserializeObject<Paquete>(resultado);

                return View(paquete);
            }

            TempData["Error"] = "No se pudo cargar el paquete.";
            return RedirectToAction("Index");
        }

    }
}
