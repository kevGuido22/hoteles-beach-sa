namespace HotelesBeachSA.Models
{
    public class HotelBeachAPI
    {
        public HttpClient Initial()
        {
            var client = new HttpClient();

            client.BaseAddress = new Uri("https://localhost:7016/api/");
            return client;
        }
    }
}
