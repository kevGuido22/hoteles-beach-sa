namespace HotelesBeachSA.Models
{
    public class PagoViewModel
    {
        public int FormaPagoId { get; set; }
        public string NumeroPago { get; set; }  // Número de tarjeta o cheque
        public string Banco { get; set; }  // Nombre del banco
    }
}
