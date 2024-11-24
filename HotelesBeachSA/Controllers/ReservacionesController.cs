using HotelesBeachSA.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Diagnostics.Eventing.Reader;
using System.Reflection;

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

                // Recargar los paquetes en caso de error en el modelo
                var paquetesResponse = await _client.GetAsync("Paquetes/ListadoHabilitados");
                if (paquetesResponse.IsSuccessStatusCode)
                {
                    var paquetes = JsonConvert.DeserializeObject<List<Paquete>>(await paquetesResponse.Content.ReadAsStringAsync());
                    ViewData["Paquetes"] = new SelectList(paquetes, "id", "nombre");
                }
                else
                {
                    ViewData["Paquetes"] = new SelectList(new List<Paquete>(), "id", "nombre");
                }

                return View("Create", reservacion);
            }

            // Obtener los detalles del paquete seleccionado
            HttpResponseMessage paqueteResponse = await _client.GetAsync($"Paquetes/Buscar?id={reservacion.PaqueteId}");
            if (!paqueteResponse.IsSuccessStatusCode)
            {
                TempData["Error"] = "No se pudo obtener la información del paquete seleccionado.";
                return RedirectToAction("Create");
            }

            var paquete = JsonConvert.DeserializeObject<Paquete>(await paqueteResponse.Content.ReadAsStringAsync());
            // Calcular la cantidad de noches
            var cantidadNoches = (reservacion.FechaFin - reservacion.FechaInicio).Days;

            if (cantidadNoches <= 0)
            {
                TempData["Error"] = "La fecha de fin debe ser posterior a la fecha de inicio.";
                return RedirectToAction("Create");
            }

            // Calcular costos iniciales
            var totalNoche = paquete.CostoPersona * reservacion.CantidadPersonas * cantidadNoches;
            decimal descuento = 0;

            // Aplicar descuentos según la cantidad de noches
            if (cantidadNoches >= 3 && cantidadNoches <= 6)
            {
                descuento = totalNoche * 0.10m; // 10%
            }
            else if (cantidadNoches >= 7 && cantidadNoches <= 9)
            {
                descuento = totalNoche * 0.15m; // 15%
            }
            else if (cantidadNoches >= 10 && cantidadNoches <= 12)
            {
                descuento = totalNoche * 0.20m; // 20%
            }
            else if (cantidadNoches >= 13)
            {
                descuento = totalNoche * 0.25m; // 25%
            }

            // Aplicar descuento si es en efectivo (por defecto)
            var montoConDescuento = totalNoche - descuento;

            // Calcular costos adicionales
            var prima = montoConDescuento * (decimal)paquete.PrimaReserva / 100;
            var restante = montoConDescuento - prima;
            var mensualidad = restante / paquete.Mensualidades;

            // Calcular IVA
            var iva = montoConDescuento * 0.13m;
            var totalFinal = montoConDescuento + iva;

            // Generar detalles de pago
            var detallesPago = new ConfirmarReservacionDTO
            {
                Reservacion = reservacion,
                PaqueteId = paquete.Id,
                PaqueteNombre = paquete.Nombre,
                CostoPorPersona = paquete.CostoPersona,
                TotalPorNoche = totalNoche,
                Descuento = descuento,
                MontoConDescuento = montoConDescuento,
                Prima = prima,
                Mensualidad = mensualidad,
                Iva = iva,
                TotalFinal = totalFinal,
            };

            TempData["Reservacion"] = JsonConvert.SerializeObject(reservacion);
            TempData["DetallesPago"] = JsonConvert.SerializeObject(detallesPago);
            TempData.Keep();

            return RedirectToAction("PasarelaPago");
        }


        [HttpGet]
        public async Task<IActionResult> PasarelaPago()
        {
            var reservacionJson = TempData["Reservacion"] as string;
            var detallesPagoJson = TempData["DetallesPago"] as string;
            TempData.Keep("Reservacion");
            TempData.Keep("DetallesPago");

            if (string.IsNullOrEmpty(reservacionJson) || string.IsNullOrEmpty(detallesPagoJson))
            {
                TempData["Error"] = "Hubo un problema al recuperar los datos de la reservación o los detalles de pago. Por favor, inicia el proceso nuevamente.";
                return RedirectToAction("Create");
            }

            var reservacion = JsonConvert.DeserializeObject<Reservacion>(reservacionJson);
            var detallesPago = JsonConvert.DeserializeObject<ConfirmarReservacionDTO>(detallesPagoJson);

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

            // Pasar la reservación y detalles de pago a la vista
            ViewBag.DetallesPago = detallesPago;
            return View(reservacion);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PasarelaPago(ConfirmarReservacionDTO model)
        {
            if (model.FormaPagoId <= 0)
            {
                TempData["Error"] = "Por favor, selecciona un método de pago válido.";
                return RedirectToAction("PasarelaPago");
            }

            if ((model.FormaPagoId == 2 || model.FormaPagoId == 3) && (string.IsNullOrEmpty(model.NumeroTarjeta) || string.IsNullOrEmpty(model.Banco)))
            {
                TempData["Error"] = "Por favor, ingresa el número de tarjeta/cheque y el nombre del banco.";
                return RedirectToAction("PasarelaPago");
            }

            if (model.FormaPagoId == 1) // efectivo
            {
                decimal descuento = 0m;

                if (model.CantidadNoches >= 3 && model.CantidadNoches <= 6)
                {
                    descuento = model.TotalPorNoche * model.CantidadNoches * 0.10m; // 10% de descuento
                }
                else if (model.CantidadNoches >= 7 && model.CantidadNoches <= 9)
                {
                    descuento = model.TotalPorNoche * model.CantidadNoches * 0.15m; // 15% de descuento
                }
                else if (model.CantidadNoches >= 10 && model.CantidadNoches <= 12)
                {
                    descuento = model.TotalPorNoche * model.CantidadNoches * 0.20m; // 20% de descuento
                }
                else if (model.CantidadNoches > 13)
                {
                    descuento = model.TotalPorNoche * model.CantidadNoches * 0.25m; // 25% de descuento
                }

                model.Descuento = descuento;
                model.MontoConDescuento = (model.TotalPorNoche * model.CantidadNoches) - descuento;
            }
            else if (model.FormaPagoId == 2) // cheque
            {
                model.Descuento = 0m; // Sin descuento
                model.MontoConDescuento = model.TotalPorNoche * model.CantidadNoches;

                // Validar datos del cheque
                if (string.IsNullOrEmpty(model.NumeroTarjeta) || string.IsNullOrEmpty(model.Banco))
                {
                    throw new Exception("Debe ingresar el número de cheque y el banco correspondiente.");
                }
            }
            else if (model.FormaPagoId == 3) // tarjeta
            {
                model.Descuento = 0m; // Sin descuento
                model.MontoConDescuento = model.TotalPorNoche * model.CantidadNoches;
            }

            model.Iva = model.MontoConDescuento * 0.13m;
            model.TotalFinal = model.MontoConDescuento + model.Iva;

            // Recuperar la reservación original desde TempData
            var reservacionJson = TempData["Reservacion"] as string;
            TempData.Keep("Reservacion");

            if (string.IsNullOrEmpty(reservacionJson))
            {
                TempData["Error"] = "Hubo un problema al recuperar los datos de la reservación. Por favor, inicia el proceso nuevamente.";
                return RedirectToAction("Create");
            }

            var reservacionOriginal = JsonConvert.DeserializeObject<Reservacion>(reservacionJson);

            // Guardar los detalles de la reservación y el pago en TempData
            TempData["Reservacion"] = JsonConvert.SerializeObject(reservacionOriginal);
            TempData["Pago"] = JsonConvert.SerializeObject(model);
            TempData["DetallesPago"] = JsonConvert.SerializeObject(model);
            TempData.Keep("Pago");

            return RedirectToAction("ConfirmarReservacion");
        }





        [HttpGet]
        public IActionResult ConfirmarReservacion()
        {
            // Recuperar la reservación desde TempData
            var reservacionJson = TempData["Reservacion"] as string;
            TempData.Keep("Reservacion");

            // Recuperar la info del pago desde TempData
            var pagoJson = TempData["Pago"] as string;
            TempData.Keep("Pago");

            // Recuperar los detalles del pago desde TempData
            var detallesPagoJson = TempData["DetallesPago"] as string;
            TempData.Keep("DetallesPago");

            if (string.IsNullOrEmpty(reservacionJson) || string.IsNullOrEmpty(pagoJson))
            {
                TempData["Error"] = "Hubo un problema al recuperar los datos. Por favor, vuelve a intentarlo.";
                return RedirectToAction("Create");
            }

            var reservacion = JsonConvert.DeserializeObject<Reservacion>(reservacionJson);
            var pago = JsonConvert.DeserializeObject<PagoViewModel>(pagoJson);
            var detallesPago = JsonConvert.DeserializeObject<ConfirmarReservacionDTO>(detallesPagoJson);

            // Pasar los datos de la reservación y el pago a la vista
            var model = new ConfirmacionViewModel
            {
                Reservacion = reservacion,
                Pago = pago,
                FormaPagoId = pago.FormaPagoId,
                Banco = pago.Banco,
                NumeroPago = "123",
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
