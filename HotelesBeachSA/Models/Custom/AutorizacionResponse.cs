namespace HotelesBeachSA.Models.Custom
{
    public class AutorizacionResponse
    {
        public string Token { get; set; }

        public bool Resultado { get; set; }

        public string Msj { get; set; }

        public Usuario Usuario { get; set; }

        public List<RolPermisoResponse> Roles { get; set; }
    }
}
