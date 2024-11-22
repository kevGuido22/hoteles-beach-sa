using System.ComponentModel.DataAnnotations;

namespace HotelesBeachSA.Models
{
    public class FormaPago
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Debe de ingresar el nombre de la forma de pago")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Debe indicar si el detalle de pago es requerido")]
        public bool IsPaymentDetailRequired { get; set; }
    }
}
