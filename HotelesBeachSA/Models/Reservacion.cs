using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelesBeachSA.Models
{
    public class Reservacion
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Debe de ingresar la llave foranea del usuario")]
        [ForeignKey("Usuario")]
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "Debe de ingresar la llave foranea del paquete")]
        [ForeignKey("Paquete")]
        public int PaqueteId { get; set; }

        [Required(ErrorMessage = "Debe de ingresar la cantidad de personas")]
        public int CantidadPersonas {  get; set; }

        [Required(ErrorMessage = "Debe de ingresar la fecha de inicio de la reservacion")]
        public DateTime FechaInicio { get; set; }

        [Required(ErrorMessage = "Debe de ingresar la fecha de fin de la reservacion")]
        public DateTime FechaFin { get; set; }

        public DateTime? FechaCreacion { get; set; }//Fecha de creacion de la reserva

    }
}
