using System.ComponentModel.DataAnnotations;

namespace HotelesBeachSA.Models
{
    public class Rol
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Debe de ingresar el nombre del rol")]
        public string Name { get; set; }
    }
}
