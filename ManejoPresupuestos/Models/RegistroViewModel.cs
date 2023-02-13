using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuestos.Models
{
    public class RegistroViewModel
    {
        [Required (ErrorMessage = "El campo {0} es requerido")]
        [EmailAddress (ErrorMessage = "El correo debe ser un campo válido")]
        public string Email { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
