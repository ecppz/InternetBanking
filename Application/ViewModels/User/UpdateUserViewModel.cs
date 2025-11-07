using System.ComponentModel.DataAnnotations;

namespace Application.ViewModels.User
{
    public class UpdateUserViewModel
    {
        public required string Id { get; set; }

        [Required(ErrorMessage = "Debes ingresar el nombre del usuario")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Debes ingresar el apellido del usuario")]
        public required string LastName { get; set; }

        [Required(ErrorMessage = "Debes ingresar la cédula del usuario")]
        [RegularExpression(@"^\d{3}-\d{7}-\d{1}$|^\d{11}$", ErrorMessage = "Formato de cédula inválido")]
        public required string DocumentNumber { get; set; }

        [Required(ErrorMessage = "Debes ingresar el nombre de usuario")]
        public required string UserName { get; set; }

        [Required(ErrorMessage = "Debes ingresar el correo electrónico")]
        [EmailAddress(ErrorMessage = "Correo inválido")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Debes seleccionar el tipo de usuario")]
        public required string Role { get; set; }

        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden")]
        [DataType(DataType.Password)]
        public string? ConfirmPassword { get; set; }
    }
}
