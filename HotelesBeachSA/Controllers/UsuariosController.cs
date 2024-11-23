using HotelesBeachSA.Models;
using HotelesBeachSA.Models.Custom;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Security.Claims;

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

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind] Usuario usuario)
        {

            AutorizacionResponse autorizacion = null;

            if (usuario == null)
            {
                TempData["Error"] = "Usuario o contraseña incorrecta";
                return View(usuario);
            }

            //se utiliza el metodo de la API para generar el token
            HttpResponseMessage response = await httpClient.PostAsync(
                $"Usuarios/AutenticarPW?email={usuario.Email}&password={usuario.Password}", null);

            if (response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadAsStringAsync().Result; //se realiza lectura de los datos en formato JSON
                autorizacion = JsonConvert.DeserializeObject<AutorizacionResponse>(result); //se convierte los datos JSON a un object con su toke n
            }

            if (autorizacion != null && autorizacion.Resultado == true)
            {
                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme); //se instancia la identidad a iniciar sesión

                identity.AddClaim(new Claim(ClaimTypes.Name, usuario.Email)); //se rellena los datos

                var principal = new ClaimsPrincipal(identity); //se crea la entidad principal
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal); //se inicia sesión

                HttpContext.Session.SetString("Token", autorizacion.Token); //se guarda el token en la sesión, se almacena el token otorgado

                return RedirectToAction("Index", "Home"); //se redirecciona al form principal
            }
            else
            {
                return View(usuario);
            }
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();

            return RedirectToAction("Login", "Users");
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind] Usuario pUsuario)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Verifique que todos los campos estén correctos";

                if (pUsuario.Telefono == null)
                {
                    ModelState.AddModelError("Telefono", "Debe ingresar un Teléfono");
                }

                if (pUsuario.Direccion == null)
                {
                    ModelState.AddModelError("Direccion", "Debe ingresar una Dirección");
                }
                if (pUsuario.Email == null)
                {
                    ModelState.AddModelError("Email", "Debe ingresar un email");
                }
                if (pUsuario.Password == null)
                {
                    ModelState.AddModelError("Password", "Debe ingresar un Password");
                }

                return View(pUsuario);
            }

            var agregar = httpClient.PostAsJsonAsync<Usuario>("Usuarios/Agregar", pUsuario);

            await agregar;

            var resultado = agregar.Result;

            if (resultado.IsSuccessStatusCode)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["Mensaje"] = "Ya existe un usuario con este correo";
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

                if (data.results.Count > 0)
                {
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

                    TempData["NombreCompleto"] = usuario.Nombre_Completo;
                    TempData["Cedula"] = usuario.Cedula;
                    TempData["TipoCedula"] = usuario.Tipo_Cedula;
                    TempData["Password"] = usuario.Password;

                    return RedirectToAction("Create", "Usuarios");
                }
                else
                {
                    TempData["Error"] = "No se encontró información para la cédula ingresada.";
                    return RedirectToAction("ObtenerInfo", "Usuarios");
                }
            }
            else
            {
                TempData["Error"] = "Error al comunicarse con el servicio de cédulas.";
                return RedirectToAction("ObtenerInfo", "Usuarios");
            }
        }

    }
}


