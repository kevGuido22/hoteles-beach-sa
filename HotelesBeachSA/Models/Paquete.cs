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

        [Required(ErrorMessage = "Debe de especificar si el paquete está habilitado.")]
        [Range(0, 1, ErrorMessage = "El valor de 'Esta habilitado' debe ser 0 (No) o 1 (Sí).")]
        public int IsEnabled { get; set; }
    }
}
