using Azure;
using HotelesBeachSA.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MotoscrAPI.Models.custom;
using Newtonsoft.Json;
using System.Diagnostics.Eventing.Reader;
using System.Net.Http.Headers;
using System.Reflection;

namespace HotelesBeachSA.Controllers
{
    public class ReservacionesController : Controller
    {
        private readonly HttpClient _client;
        private readonly HttpClient _clientGometa;

        public ReservacionesController(IHttpClientFactory httpClientFactory)
        {
            //GometaHttpClient
            _client = httpClientFactory.CreateClient("ReservacionesHttpClient");
            _clientGometa = httpClientFactory.CreateClient("GometaHttpClient");
            //_clientGometa = new HttpClient();
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

        public async Task<IActionResult> UserReservationView(int id)
        {
            // Obtener paquetes
            var paquetesResponse = await _client.GetAsync("Paquetes/ListadoCompleto");
            var paquetes = new List<Paquete>();

            if (paquetesResponse.IsSuccessStatusCode)
            {
                var paquetesResultado = await paquetesResponse.Content.ReadAsStringAsync();
                paquetes = JsonConvert.DeserializeObject<List<Paquete>>(paquetesResultado);
            }

            TempData["paqueteId"] = id;
            TempData["paquetes"] = paquetes;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserReservationView([Bind] Reservacion reservacion)
        {

            var usuarioJson = HttpContext.Session.GetString("usuarioActual");

            if (string.IsNullOrEmpty(usuarioJson))
            {
                return Content("No hay usuario en la sesión.");
            }

            // Deserializar el JSON al objeto original (ejemplo con clase Usuario)
            var usuario = JsonConvert.DeserializeObject<Usuario>(usuarioJson);

            reservacion.Id = 0;
            reservacion.FechaCreacion = DateTime.Now;
            reservacion.UsuarioId = usuario.Id;


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

            ConfirmarReservacionDTO reservacionDTO = new ConfirmarReservacionDTO
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
                CantidadNoches = cantidadNoches,
                CantidadPersonas = reservacion.CantidadPersonas
            };

            //TempData["Reservacion"] = JsonConvert.SerializeObject(reservacion);
            //TempData["DetallesPago"] = JsonConvert.SerializeObject(detallesPago);
            TempData["ReservacionDTO"] = JsonConvert.SerializeObject(reservacionDTO);
            TempData.Keep();

            return RedirectToAction("PasarelaPago");
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

            if (TempData["ReservacionDTO"] == null)
            {
                TempData["Error"] = "Hubo un problema al procesar la reservación.";
                return RedirectToAction("Create");
            }

            ConfirmarReservacionDTO reservacionDTO = JsonConvert.DeserializeObject<ConfirmarReservacionDTO>(TempData["ReservacionDTO"].ToString());


            HttpResponseMessage responseFormasPago = await _client.GetAsync("FormaPago/Listado");

            if (!responseFormasPago.IsSuccessStatusCode)
            {

                TempData["Error"] = "No se pudieron cargar las formas de pago.";
                ViewData["FormasPago"] = new SelectList(new List<FormaPago>(), "Id", "Nombre");
            }

            List<FormaPago> formasPago = JsonConvert.DeserializeObject<List<FormaPago>>(await responseFormasPago.Content.ReadAsStringAsync());

            PasarelaPagoViewModel pasarelaViewModel = new PasarelaPagoViewModel
            {
                ConfirmarReservacionDTO =reservacionDTO,
                FormasPagos = formasPago
            };

            if (formasPago != null && formasPago.Any())
            {
                ViewData["FormasPago"] = new SelectList(formasPago, "Id", "Nombre");
            }
            else
            {
                ViewData["FormasPago"] = new SelectList(new List<FormaPago>(), "Id", "Nombre");
                TempData["Error"] = "No se encontraron formas de pago disponibles.";
            }


            // Pasar la reservación y detalles de pago a la vista
            //ViewBag.DetallesPago = detallesPago;
            TempData["reservacionBackup"] = JsonConvert.SerializeObject(reservacionDTO.Reservacion);
            return View(pasarelaViewModel);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PasarelaPago(PasarelaPagoViewModel model)
        {
            Reservacion reservacionBackup = JsonConvert.DeserializeObject<Reservacion>(TempData["reservacionBackup"].ToString());

            PasarelaPagoViewModel pasarelaPagoModel = model;
            var formaPagoId = pasarelaPagoModel.ConfirmarReservacionDTO.FormaPagoId;
            ConfirmarReservacionDTO confirmarReservacion = pasarelaPagoModel.ConfirmarReservacionDTO;

            if (formaPagoId <= 0)
            {
                TempData["Error"] = "Por favor, selecciona un método de pago válido.";
                return RedirectToAction("PasarelaPago");
            }

            if ((formaPagoId == 2 || formaPagoId == 3) && (string.IsNullOrEmpty(confirmarReservacion.NumeroTarjeta) || string.IsNullOrEmpty(confirmarReservacion.Banco)))
            {
                TempData["Error"] = "Por favor, ingresa el número de tarjeta/cheque y el nombre del banco.";
                return RedirectToAction("PasarelaPago");
            }

            if (formaPagoId == 1) // efectivo
            {
                decimal descuento = 0m;

                if (confirmarReservacion.CantidadNoches >= 3 && confirmarReservacion.CantidadNoches <= 6)
                {
                    descuento = confirmarReservacion.TotalPorNoche * confirmarReservacion.CantidadNoches * 0.10m; // 10% de descuento
                }
                else if (confirmarReservacion.CantidadNoches >= 7 && confirmarReservacion.CantidadNoches <= 9)
                {
                    descuento = confirmarReservacion.TotalPorNoche * confirmarReservacion.CantidadNoches * 0.15m; // 15% de descuento
                }
                else if (confirmarReservacion.CantidadNoches >= 10 && confirmarReservacion.CantidadNoches <= 12)
                {
                    descuento = confirmarReservacion.TotalPorNoche * confirmarReservacion.CantidadNoches * 0.20m; // 20% de descuento
                }
                else if (confirmarReservacion.CantidadNoches > 13)
                {
                    descuento = confirmarReservacion.TotalPorNoche * confirmarReservacion.CantidadNoches * 0.25m; // 25% de descuento
                }

                confirmarReservacion.Descuento = descuento;
                confirmarReservacion.MontoConDescuento = (confirmarReservacion.TotalPorNoche * confirmarReservacion.CantidadNoches) - descuento;
            }
            else if (confirmarReservacion.FormaPagoId == 2) // cheque
            {
                confirmarReservacion.Descuento = 0m; // Sin descuento
                confirmarReservacion.MontoConDescuento = confirmarReservacion.TotalPorNoche * confirmarReservacion.CantidadNoches;

                // Validar datos del cheque
                if (string.IsNullOrEmpty(confirmarReservacion.NumeroTarjeta) || string.IsNullOrEmpty(confirmarReservacion.Banco))
                {
                    throw new Exception("Debe ingresar el número de cheque y el banco correspondiente.");
                }
            }
            else if (confirmarReservacion.FormaPagoId == 3) // tarjeta
            {
                confirmarReservacion.Descuento = 0m; // Sin descuento
                confirmarReservacion.MontoConDescuento = confirmarReservacion.TotalPorNoche * confirmarReservacion.CantidadNoches;
            }

            confirmarReservacion.Iva = confirmarReservacion.MontoConDescuento * 0.13m;
            confirmarReservacion.TotalFinal = confirmarReservacion.MontoConDescuento + confirmarReservacion.Iva;


            pasarelaPagoModel.ConfirmarReservacionDTO = confirmarReservacion;
            pasarelaPagoModel.ConfirmarReservacionDTO.Reservacion = reservacionBackup;

            TempData["pasarelaPagoModel"] = JsonConvert.SerializeObject(pasarelaPagoModel);

            return RedirectToAction("ConfirmarReservacion");
        }



        [HttpGet]
        public async Task<IActionResult>  ConfirmarReservacion()
        {

            //recuperar la pasarelaPagoViewModel
            PasarelaPagoViewModel pasarelaPagoViewModel = JsonConvert.DeserializeObject<PasarelaPagoViewModel>(TempData["pasarelaPagoModel"].ToString());

            ApiGometaResponse data = null;

            //obtener tipos de cambio
            var gometaResponse = await _clientGometa.GetAsync("");

            if (gometaResponse.IsSuccessStatusCode)
            {
                var result = await gometaResponse.Content.ReadAsStringAsync();
                data = JsonConvert.DeserializeObject<ApiGometaResponse>(result);
            }

            //convertir a decimal la venta de dolares
            decimal precioVenta = decimal.Parse(data.Venta, System.Globalization.CultureInfo.InvariantCulture);
            decimal precioCompra = decimal.Parse(data.Compra, System.Globalization.CultureInfo.InvariantCulture);

            //set total doalres
            //pasarelaPagoViewModel.ConfirmarReservacionDTO.TotalDolares = Math.Round(pasarelaPagoViewModel.ConfirmarReservacionDTO.TotalFinal / precioVenta, 2);

            //set total en colones
            pasarelaPagoViewModel.ConfirmarReservacionDTO.TotalColones = Math.Round(pasarelaPagoViewModel.ConfirmarReservacionDTO.TotalFinal * precioCompra, 2);

            pasarelaPagoViewModel.ConfirmarReservacionDTO.TotalDolares = pasarelaPagoViewModel.ConfirmarReservacionDTO.TotalFinal;

            //empaquetar el modelo para persistir en la siguiente accion
            TempData["pasarelaPagoModel"] = JsonConvert.SerializeObject(pasarelaPagoViewModel);
            return View(pasarelaPagoViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("ConfirmarReservacion")]
        public async Task<IActionResult> ConfirmarReservacionPost() 
        {
            PasarelaPagoViewModel pasarelaPagoViewModel = JsonConvert.DeserializeObject<PasarelaPagoViewModel>(TempData["pasarelaPagoModel"].ToString());

            _client.DefaultRequestHeaders.Authorization = AutorizacionToken();

            ConfirmarReservacionDTO confirmarReservacion = pasarelaPagoViewModel.ConfirmarReservacionDTO;

            Reservacion reservacionTemp = pasarelaPagoViewModel.ConfirmarReservacionDTO.Reservacion;
            //reservacionTemp.UsuarioId = 2;

            var reservacionResponse = await _client.PostAsJsonAsync("Reservacion/Crear", reservacionTemp);

            //falta construir, devolver al inicio para que sae vuleva a hacer la reservacion 
            if (!reservacionResponse.IsSuccessStatusCode)
            {
                //TERMINAR DE CONSTRUIR     
                TempData["Error"] = "El paquete se registró correctamente.";
                return RedirectToAction("Index");
            }

            var resultReservacion = await reservacionResponse.Content.ReadAsStringAsync();
            reservacionTemp = JsonConvert.DeserializeObject<Reservacion>(resultReservacion.ToString());

            DetallePago detallePagoTemp = new DetallePago { 
                NumeroCheque = confirmarReservacion.NumeroCheque,
                NumeroTarjeta = confirmarReservacion.NumeroTarjeta,
                Banco = confirmarReservacion.Banco
            };

            //si la forma de pago no es efectivo
            if (confirmarReservacion.FormaPagoId != 1) {
                var detallePagoResponse = await _client.PostAsJsonAsync("DetallePago/Crear", detallePagoTemp);

                if (detallePagoResponse.IsSuccessStatusCode)
                {
                    var resultDetallePago = await detallePagoResponse.Content.ReadAsStringAsync();

                    TempData["Exito"] = "El paquete se registró correctamente.";

                    detallePagoTemp = JsonConvert.DeserializeObject<DetallePago>(resultDetallePago.ToString());
                }
            }

            

            Factura facturaTemp = new Factura
            {
                Id = 0,
                ReservacionId = reservacionTemp.Id,
                DetallePagoId = detallePagoTemp.Id,
                FormaPagoId = confirmarReservacion.FormaPagoId,
                CantidadNoches = confirmarReservacion.CantidadNoches,
                ValorDescuento = confirmarReservacion.ValorDescuento,
                TotalColones = confirmarReservacion.TotalColones,
                TotalDolares = confirmarReservacion.TotalDolares
            };

            var facturaResponse = await _client.PostAsJsonAsync("Factura/Crear", facturaTemp);

            //falta construir, devolver al inicio para que sae vuleva a hacer la reservacion 
            if (!facturaResponse.IsSuccessStatusCode) {
                    TempData["Error"] = "El paquete se registró correctamente.";
                return RedirectToAction("Index");
            }

            var resultFacturaResponse = await facturaResponse.Content.ReadAsStringAsync();

            facturaTemp = JsonConvert.DeserializeObject<Factura>(resultFacturaResponse.ToString());

            return RedirectToAction("Index", "Home");
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

        private AuthenticationHeaderValue AutorizacionToken()
        {
            //se extrae el token almacenado dentro de la sesion
            var token = HttpContext.Session.GetString("token");

            //varible para almacenar el token
            AuthenticationHeaderValue authentication = null;

            if (token != null && token.Length != 0)
            {
                //almacenar el token otorgado
                authentication = new AuthenticationHeaderValue("Bearer", token);
            }

            //retornar token
            return authentication;
        }

    }
}
