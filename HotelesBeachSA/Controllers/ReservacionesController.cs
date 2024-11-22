using HotelesBeachSA.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Obtener reservaciones
            var reservacionesResponse = await _client.GetAsync("Reservacion/Listado");
            var reservaciones = new List<Reservacion>();

            if (reservacionesResponse.IsSuccessStatusCode)
            {
                var resultado = await reservacionesResponse.Content.ReadAsStringAsync();
                reservaciones = JsonConvert.DeserializeObject<List<Reservacion>>(resultado);
            }

            // Obtener paquetes
            var paquetesResponse = await _client.GetAsync("Paquetes/ListadoCompleto");
            var paquetes = new List<Paquete>();

            if (paquetesResponse.IsSuccessStatusCode)
            {
                var paquetesResultado = await paquetesResponse.Content.ReadAsStringAsync();
                paquetes = JsonConvert.DeserializeObject<List<Paquete>>(paquetesResultado);
            }

            // Obtener usuarios
            var usuariosResponse = await _client.GetAsync("Usuarios/Listado");
            var usuarios = new List<Usuario>();

            if (usuariosResponse.IsSuccessStatusCode)
            {
                var usuariosResultado = await usuariosResponse.Content.ReadAsStringAsync();
                usuarios = JsonConvert.DeserializeObject<List<Usuario>>(usuariosResultado);
            }


            // Mapear los datos al ViewModel
            var viewModel = reservaciones.Select(r => new ReservacionViewModel
            {
                Id = r.Id,
                UsuarioId = r.UsuarioId,
                UsuarioNombre = usuarios.FirstOrDefault(p => p.Id == r.UsuarioId)?.Nombre_Completo ?? "Desconocido",
                PaqueteId = r.PaqueteId,
                PaqueteNombre = paquetes.FirstOrDefault(p => p.Id == r.PaqueteId)?.Nombre ?? "Desconocido",
                CantidadPersonas = r.CantidadPersonas,
                FechaInicio = r.FechaInicio,
                FechaFin = r.FechaFin,
                FechaCreacion = r?.FechaCreacion,
            }).ToList();

            return View(viewModel);
        }
        public async Task<IActionResult> Create()
        {
            // Llama al endpoint para obtener los paquetes
            HttpResponseMessage response = await _client.GetAsync("Paquetes/ListadoCompleto");
            // llamar las formas de pago
            HttpResponseMessage responseFormasPago = await _client.GetAsync("FormaPago/Listado");

            if (response.IsSuccessStatusCode)
            {
                var paquetes = JsonConvert.DeserializeObject<List<Paquete>>(await response.Content.ReadAsStringAsync());
                var formasPago = JsonConvert.DeserializeObject<List<FormaPago>>(await responseFormasPago.Content.ReadAsStringAsync());

                // Pasar los paquetes a la vista
                ViewData["Paquetes"] = new SelectList(paquetes, "Id", "Nombre");
                ViewData["FormasPago"] = new SelectList(formasPago, "Id", "Nombre");
            }
            else
            {
                TempData["Error"] = "No se pudo cargar el listado de paquetes.";
                ViewData["Paquetes"] = new SelectList(new List<Paquete>(), "Id", "Nombre");
                ViewData["FormasPago"] = new SelectList(new List<FormaPago>(), "Id", "Nombre");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind] Reservacion reservacion)
        {
            
            // La id es autoincrementable
            reservacion.Id = 0;

            var response = await _client.PostAsJsonAsync("Reservacion/Crear", reservacion);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();

                TempData["Exito"] = "La reservación se registró correctamente.";
                return RedirectToAction("Index");
            }



            TempData["Error"] = "No se pudo registrar la reservación. Por favor, verifica los datos e inténtalo nuevamente.";
            return View(reservacion);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            HttpResponseMessage response = await _client.GetAsync($"Reservacion/Buscar?id={id}");

            if (response.IsSuccessStatusCode)
            {
                var resultado = await response.Content.ReadAsStringAsync();
                var reservacion = JsonConvert.DeserializeObject<Reservacion>(resultado);

                return View(reservacion);
            }

            TempData["Error"] = "No se pudo cargar la reservación.";
            return RedirectToAction("Index");
        }

    }
}
