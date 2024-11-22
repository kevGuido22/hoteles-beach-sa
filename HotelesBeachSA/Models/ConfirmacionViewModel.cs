namespace HotelesBeachSA.Models
{
    public class ConfirmacionViewModel
    {
        public Reservacion Reservacion { get; set; }
        public PagoViewModel Pago { get; set; }
        public int FormaPagoId { get; set; }
        public string NumeroPago { get; set; }
        public string Banco { get; set; }
    }

}
