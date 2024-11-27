namespace HotelesBeachSA.Models
{
    public class PagoViewModel
    {
        // Información básica del paquete o servicio
        public string Paquete { get; set; }  // Nombre del paquete
        public decimal CostoPorPersonaPorNoche { get; set; }  // Costo por persona por noche

        // Información del cálculo del pago
        public int CantidadPersonas { get; set; }  // Número de personas para la reservación
        public int NumeroDeNoches { get; set; }  // Número de noches de la reservación

        // Cálculos del precio total
        public decimal TotalPorNoche
        {
            get
            {
                return CostoPorPersonaPorNoche * CantidadPersonas * NumeroDeNoches;  // Total por noche sin descuento
            }
        }

        // Descuento aplicado si el pago es en efectivo
        public decimal Descuento { get; set; } = 0m;  // Descuento por pago en efectivo (por ejemplo, 10%)

        // Monto con descuento (si aplica)
        public decimal MontoConDescuento
        {
            get
            {
                return TotalPorNoche - Descuento;  // Total con descuento aplicado
            }
            set
            {
                // Permite establecer el valor del monto con descuento si se necesita
            }
        }

        // IVA (por ejemplo, 13%)
        public decimal Iva
        {
            get
            {
                return MontoConDescuento * 0.13m;  // Calcular el IVA sobre el monto con descuento
            }
            set
            {
                // Permite establecer el valor del monto con descuento si se necesita
            }
        }

        // Total final (con IVA)
        public decimal TotalFinal
        {
            get
            {
                return MontoConDescuento + Iva;  // Total final después de aplicar IVA
            }
            set
            {
                // Permite establecer el valor del total final si se necesita
            }
        }

        // Forma de pago
        public int FormaPagoId { get; set; }  // Identificador de la forma de pago, como "Efectivo", "Tarjeta", etc.

        // Campos adicionales para el pago con tarjeta o cheque
        public string NumeroPago { get; set; }  // Número de tarjeta o cheque
        public string Banco { get; set; }  // Nombre del banco

        // Método para validar si el pago es por tarjeta o cheque
        public bool EsPagoConTarjetaCheque
        {
            get
            {
                // Si el ID de la forma de pago es 2 (tarjeta) o 3 (cheque), se considera pago con tarjeta o cheque
                return FormaPagoId == 2 || FormaPagoId == 3;
            }
        }
    }


}
