using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelesBeachSA.Models
{
    public class RolPermiso
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Debe de ingresar la llave foranea del modelo Rol")]
        [ForeignKey("Rol")]
        public int RolId { get; set; }

        [Required(ErrorMessage = "Debe de ingresar la llave foranea del modelo Permiso")]
        [ForeignKey("Permiso")]
        public int PermisoId { get; set; }
    }
}
