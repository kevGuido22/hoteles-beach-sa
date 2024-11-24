using System.ComponentModel.DataAnnotations;

namespace HotelesBeachSA.Models
{
    public class FacturaViewModel
    {
        [Required]
        [Key]
        public int Id { get; set; } // Identificador de la factura.
        [Required(ErrorMessage = "Debe seleccionar una reservación")]
        public int ReservacionId { get; set; } // Llave foránea para asociar la factura con una reservación.

        [Required(ErrorMessage = "Debe seleccionar una forma de pago")]
        public int FormaPagoId { get; set; } // Llave foránea para la forma de pago.

        public int DetallePagoId { get; set; } // Llave foránea para el detalle del pago.

        [Required(ErrorMessage = "El monto en dólares es requerido")]
        [Range(0, double.MaxValue, ErrorMessage = "El monto debe ser mayor o igual a 0")]
        public decimal TotalDolares { get; set; } // Total en dólares.

        public decimal TotalColones { get; set; } // Total en colones (calculado a partir de dólares).

        [Required(ErrorMessage = "Debe ingresar la cantidad de noches")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad de noches debe ser al menos 1")]
        public int CantidadNoches { get; set; } // Cantidad de noches asociadas a la reservación.

        [Range(0, double.MaxValue, ErrorMessage = "El descuento debe ser mayor o igual a 0")]
        public decimal ValorDescuento { get; set; } // Valor del descuento (opcional).
    }


}
