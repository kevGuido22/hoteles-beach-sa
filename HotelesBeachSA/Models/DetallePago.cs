using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelesBeachSA.Models
{
    public class DetallePago
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Debe de ingresar la llave foranea del modelo Factura")]
        [ForeignKey("Factura")]
        public int FacturaId { get; set; }

        public string? NumeroCheque { get; set; }

        public string? NumeroTarjeta { get; set; }

        public string? Banco {  get; set; }





    }
}
