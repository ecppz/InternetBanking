using System.ComponentModel.DataAnnotations;

namespace Application.ViewModels.User
{
    public class UpdateUserViewModel
    {
        public required string Id { get; set; }

        [Required(ErrorMessage = "Debes ingresar el nombre del usuario")]
        [DataType(DataType.Text)]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Debes ingresar el apellido del usuario")]
        [DataType(DataType.Text)]
        public required string LastName { get; set; }

        [Required(ErrorMessage = "Debes ingresar la cedula del usuario")]
        [DataType(DataType.Text)]
        public required string DocumentNumber { get; set; }

        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden")]       
        [DataType(DataType.Password)]
        public string? ConfirmPassword { get; set; }
    }
}
