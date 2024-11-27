using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelesBeachSA.Models
{
    public class Factura
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Debe de ingresar la llave foranea del modelo Reservacion")]
        [ForeignKey("Reservacion")]
        public int ReservacionId { get; set; }

        [Required(ErrorMessage = "Debe de ingresar la llave foranea del modelo FormaPago")]
        [ForeignKey("FormaPago")]
        public int FormaPagoId { get; set; }

        [Required(ErrorMessage = "Debe de ingresar la llave foranea del modelo DetallePago")]
        [ForeignKey("DetallePago")]
        public int? DetallePagoId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad de noches debe ser mayor a 0.")]
        [Required(ErrorMessage = "Debe de ingresar la cantidad de noches")]
        public int CantidadNoches { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "El valor de descuesto debe ser mayor a 0.")]
        [Required(ErrorMessage = "Debe de ingresar el valor del descuento")]
        public decimal ValorDescuento { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "El total en dólares debe ser mayor a 0.")]
        [Required(ErrorMessage = "Debe de ingresar el valor total en dolares")]
        public decimal TotalDolares { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "El total en colones debe ser mayor a 0.")]
        [Required(ErrorMessage = "Debe de ingresar el valor total en colones")]
        public decimal TotalColones { get; set; }
    }
}
