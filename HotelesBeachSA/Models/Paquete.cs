using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace HotelesBeachSA.Models
{
    public class Paquete
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Debe ingresar el nombre del paquete")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "Debe de indicar el costo del paquete por persona")]
        public decimal CostoPersona { get; set; }

        [Required(ErrorMessage = "Debe de ingresar la cantidad de prima de la reserva del paquete")]
        public decimal PrimaReserva { get; set; }

        [Required(ErrorMessage = "Debe de ingresar la cantidad de mensualidades del paquete")]
        public int Mensualidades { get; set; }

    }
}
