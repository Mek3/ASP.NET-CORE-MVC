using ManejoPresupuestos.Validaciones;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuestos.Models
{
    public class TipoCuenta //:// IValidatableObject
    {

    public int Id { get; set; }
        [Required (ErrorMessage = "El campo {0} es requerido")]
        [PrimeraLetraMayuscula]
        [Remote (action: "VerificarExisteTipoCuenta", controller:"TiposCuentas",
            AdditionalFields = nameof(Id))]
        public string Nombre { get; set; }
        public int UsuarioId { get; set; }
        public int Orden { get; set; }

        //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        //{
        //    if(Nombre != null && Nombre.Length > 0) 
        //    {
        //        var primeraLetra= Nombre[0].ToString();

        //        if (primeraLetra != primeraLetra.ToUpper()) 
        //        {
        //            yield return new ValidationResult("La primera letra debe ser mayúscula",
        //                                new[] { nameof(Nombre) });
        //        }
        //    }
        //}

        /* Pruebas de otras validaciones*/
        //[Required (ErrorMessage = "El campo {0} es requerido")]
        //[EmailAddress(ErrorMessage = "El Campo debe ser un correo electrónico válido.")]
        //public string Email { get; set; }

        //[Required(ErrorMessage ="El campo {0} es requerido")]
        //[Range (minimum: 18, maximum:130, ErrorMessage ="El valor debe estar entre {1} y {2}")]
        //public int Edad { get; set; }
        //[Url (ErrorMessage ="El campo debe ser una URL válida")]
        //public string URL{ get; set; }
        //[CreditCard (ErrorMessage = "La tarjeta de crédito no es válida" )]
        //[Display(Name ="Tarjeta de Crédito")]
        //public string TarjetaDeCredito { get; set; }

        //[StringLength(maximumLength:50, MinimumLength = 3, ErrorMessage= "La longitud del campo {0} debe estar entre {2} y {1}")]
        //[Display (Name = "Nombre del tipo cuenta")]

    }
}
