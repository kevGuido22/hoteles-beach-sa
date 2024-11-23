namespace HotelesBeachSA.Models
{
    public class ConfirmarReservacionDTO
    {
        public int UsuarioId { get; set; }
        public Reservacion Reservacion { get; set; }
        public int PaqueteId { get; set; }
        public string PaqueteNombre { get; set; }
        public int CantidadPersonas { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int FormaPagoId { get; set; }
        public int DetallePagoId { get; set; }
        public int CantidadNoches { get; set; }
        public decimal ValorDescuento { get; set; }
        public decimal TotalDolares { get; set; }
        public decimal TotalColones { get; set; }
        public decimal CostoPorPersona { get; set; }
        public decimal Iva { get; set; }
        public decimal Mensualidad { get; set; }
        public decimal Descuento { get; set; }
        public decimal Prima { get; set; }
        public decimal MontoConDescuento { get; set; }
        public decimal TotalPorNoche { get; set; }
        public decimal TotalFinal { get; set; }
        public string? NumeroCheque { get; set; }
        public string? NumeroTarjeta { get; set; }
        public string? Banco { get; set; }
    }
}
