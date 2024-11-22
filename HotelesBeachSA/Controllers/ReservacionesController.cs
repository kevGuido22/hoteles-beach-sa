using HotelesBeachSA.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Diagnostics.Eventing.Reader;

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

            // Obtener usuarios para mostrar nomrbe
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
            HttpResponseMessage response = await _client.GetAsync("Paquetes/ListadoCompleto");

            if (response.IsSuccessStatusCode)
            {
                var paquetes = JsonConvert.DeserializeObject<List<Paquete>>(await response.Content.ReadAsStringAsync());

                // Pasar los paquetes a la vista
                ViewData["Paquetes"] = new SelectList(paquetes, "Id", "Nombre");
            }
            else
            {
                TempData["Error"] = "No se pudo cargar el listado de paquetes.";
                ViewData["Paquetes"] = new SelectList(new List<Paquete>(), "Id", "Nombre");
            }
            return View();
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind] Reservacion reservacion)
        {
            reservacion.FechaCreacion = DateTime.Now;

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Por favor, completa todos los campos requeridos correctamente.";

                var paquetesResponse = await _client.GetAsync("Paquetes/ListadoCompleto");
                if (paquetesResponse.IsSuccessStatusCode)
                {
                    var paquetes = JsonConvert.DeserializeObject<List<Paquete>>(await paquetesResponse.Content.ReadAsStringAsync());
                    ViewData["Paquetes"] = new SelectList(paquetes, "Id", "Nombre");
                }
                else
                {
                    ViewData["Paquetes"] = new SelectList(new List<Paquete>(), "Id", "Nombre");
                }

                return View("Create", reservacion);
            }

            TempData["Reservacion"] = JsonConvert.SerializeObject(reservacion);

            return RedirectToAction("PasarelaPago");
        }




        [HttpGet]
        public async Task<IActionResult> PasarelaPago()
        {
            // Recuperar datos de la reservación desde TempData
            var reservacionJson = TempData["Reservacion"] as string;

            if (string.IsNullOrEmpty(reservacionJson))
            {
                TempData["Error"] = "Hubo un problema al recuperar los datos de la reservación. Por favor, inicia el proceso nuevamente.";
                return RedirectToAction("Create");
            }

            var reservacion = JsonConvert.DeserializeObject<Reservacion>(reservacionJson);

            // Llamar al endpoint para obtener las formas de pago
            HttpResponseMessage responseFormasPago = await _client.GetAsync("FormaPago/Listado");
            if (responseFormasPago.IsSuccessStatusCode)
            {
                var formasPago = JsonConvert.DeserializeObject<List<FormaPago>>(await responseFormasPago.Content.ReadAsStringAsync());
                if (formasPago != null && formasPago.Any())
                {
                    ViewData["FormasPago"] = new SelectList(formasPago, "Id", "Nombre");
                }
                else
                {
                    ViewData["FormasPago"] = new SelectList(new List<FormaPago>(), "Id", "Nombre");
                    TempData["Error"] = "No se encontraron formas de pago disponibles.";
                }
            }
            else
            {
                TempData["Error"] = "No se pudieron cargar las formas de pago.";
                ViewData["FormasPago"] = new SelectList(new List<FormaPago>(), "Id", "Nombre");
            }

            // Pasar la reservación actual a la vista
            return View(reservacion);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PasarelaPago(PagoViewModel model)
        {
            if (model.FormaPagoId <= 0)
            {
                TempData["Error"] = "Por favor, selecciona un método de pago válido.";
                return RedirectToAction("PasarelaPago");
            }

            if ((model.FormaPagoId == 2 || model.FormaPagoId == 3) && (string.IsNullOrEmpty(model.NumeroPago) || string.IsNullOrEmpty(model.Banco)))
            {
                TempData["Error"] = "Por favor, ingresa el número de tarjeta/cheque y el nombre del banco.";
                return RedirectToAction("PasarelaPago");
            }

            // Recuperar la reservación original desde TempData
            var reservacionJson = TempData["Reservacion"] as string;
            if (string.IsNullOrEmpty(reservacionJson))
            {
                TempData["Error"] = "Hubo un problema al recuperar los datos de la reservación. Por favor, inicia el proceso nuevamente.";
                return RedirectToAction("Create");
            }

            var reservacionOriginal = JsonConvert.DeserializeObject<Reservacion>(reservacionJson);

            var factura = new Factura
            {
                ReservacionId = reservacionOriginal.Id,
                FormaPagoId = model.FormaPagoId
            };

            // Llamada a la API para crear la factura
            var response = await _client.PostAsJsonAsync("Factura/Crear", factura);

            if (response.IsSuccessStatusCode)
            {
                // Guardar los datos de la reservación y el pago en TempData
                TempData["Reservacion"] = JsonConvert.SerializeObject(reservacionOriginal);
                TempData["Pago"] = JsonConvert.SerializeObject(model); 

                TempData["Exito"] = "La factura se creó correctamente.";
                return RedirectToAction("ConfirmarReservacion");
            }
            else
            {
                TempData["Error"] = "Hubo un problema al crear la factura. Por favor, inténtalo nuevamente.";
                return RedirectToAction("PasarelaPago");
            }
        }


        [HttpGet]
        public IActionResult ConfirmarReservacion()
        {
            // Recuperar la reservación desde TempData
            var reservacionJson = TempData["Reservacion"] as string;

            // Recuperar la info del pago
            var pagoJson = TempData["Pago"] as string;

            if (string.IsNullOrEmpty(reservacionJson) || string.IsNullOrEmpty(pagoJson))
            {
                TempData["Error"] = "Hubo un problema al recuperar los datos. Por favor, vuelve a intentarlo.";
                return RedirectToAction("Create");
            }

            var reservacion = JsonConvert.DeserializeObject<Reservacion>(reservacionJson);
            var pago = JsonConvert.DeserializeObject<PagoViewModel>(pagoJson);

            // Pasar los datos de la reservación y el pago a la vista
            var model = new ConfirmacionViewModel
            {
                Reservacion = reservacion,
                Pago = pago
            };

            return View(model);
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
