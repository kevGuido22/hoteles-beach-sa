namespace HotelesBeachSA.Models
{
    public class ReservacionViewModel
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string UsuarioNombre { get; set; } // No se almacena en la BD
        public int PaqueteId { get; set; }
        public string PaqueteNombre { get; set; } // No se almacena en la BD
        public int CantidadPersonas { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public DateTime? FechaCreacion { get; set; }
    }
}
