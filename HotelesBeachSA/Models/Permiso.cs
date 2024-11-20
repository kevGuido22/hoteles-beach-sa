using System.ComponentModel.DataAnnotations;

namespace HotelesBeachSA.Models
{
    public class Permiso
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Debe de ingresar el nombre del permiso")]
        public string Name { get; set; }
    }
}
