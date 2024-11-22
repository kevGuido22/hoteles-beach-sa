using HotelesBeachSA.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace HotelesBeachSA.Controllers
{
    public class UsuariosController : Controller
    {
        private HotelBeachAPI hotelAPI;
        private HttpClient httpClient;

        public UsuariosController()
        {
            hotelAPI = new HotelBeachAPI();
            httpClient = hotelAPI.Initial();
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        //probando github desktop...

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind] Usuario pUsuario)
        {
            var agregar = httpClient.PostAsJsonAsync<Usuario>("Usuarios/Agregar", pUsuario);

            await agregar;

            var resultado = agregar.Result;

            if (resultado.IsSuccessStatusCode)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["Mensaje"] = "No se logró registrar el usuario";
                return View(pUsuario);
            }
        }

        [HttpGet]
        public IActionResult ObtenerInfo()
        {
            return View();
        }

        public async Task<IActionResult> ObtenerInfo(string cedula)
        {
            Usuario usuario = new Usuario();

            var response = await httpClient.GetAsync("https://apis.gometa.org/cedulas/" + cedula);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                dynamic data = JObject.Parse(content);

                Console.WriteLine(data.ToString());

                var persona = new Usuario
                {
                    Nombre_Completo = data.nombre,
                    Tipo_Cedula = data.results[0].guess_type,
                    Cedula = data.cedula,
                    Telefono = "",
                    Direccion = "",
                    Email = "",
                    Password = "",
                };

                usuario = persona;

            }

            TempData["NombreCompleto"] = usuario.Nombre_Completo;
            TempData["Cedula"] = usuario.Cedula;
            TempData["TipoCedula"] = usuario.Tipo_Cedula;
            TempData["Password"] = usuario.Password;
            return RedirectToAction("Create", "Usuarios");
        }
    }
}


