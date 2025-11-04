using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.ViewModels.User
{
    public class RegisterUserViewModel
    {
        [Required(ErrorMessage = "Debes ingresar el nombre del usuario")]
        [DataType(DataType.Text)]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Debes ingresar el apellido del usuario")]
        [DataType(DataType.Text)]
        public required string LastName { get; set; }

        [Required(ErrorMessage = "Debes ingresar el email del usuario")]
        [DataType(DataType.EmailAddress)]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Debes ingresar el username del usuario")]
        [DataType(DataType.Text)]
        public required string UserName { get; set; }

        [Required(ErrorMessage = "Debes ingresar la contraseña")]
        [DataType(DataType.Password)]
        public required string Password { get; set; }

        [Compare(nameof(Password),ErrorMessage = "Las contraseñas no coinciden")]
        [Required(ErrorMessage = "Debes confirmar la contraseña")]
        [DataType(DataType.Password)]
        public required string ConfirmPassword { get; set; }

        [DataType(DataType.Text)]
        public string? Phone { get; set; }

        [DataType(DataType.Upload)]
        [Required(ErrorMessage = "Debes ingresar una foto de perfil")]
        public IFormFile? ProfileImageFile { get; set; }
    }
}
