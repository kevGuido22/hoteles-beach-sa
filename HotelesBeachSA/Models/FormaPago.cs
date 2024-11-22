using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace HotelesBeachSA.Models
{
    public class FormaPago
    {
        [Key]
        public int Id { get; set; }

        [JsonProperty("name")]
        [Required(ErrorMessage = "Debe ingresar el nombre de la forma de pago")]
        public string Nombre { get; set; }

        [JsonProperty("isPaymentDetailRequired")]
        [Required(ErrorMessage = "Debe indicar si el detalle de pago es requerido")]
        public bool IsPaymentDetailRequired { get; set; }
    }
}
