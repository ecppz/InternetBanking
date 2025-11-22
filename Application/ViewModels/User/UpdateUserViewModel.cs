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

        // Campo especial para clientes
        [Range(0, double.MaxValue, ErrorMessage = "El monto adicional debe ser mayor o igual a 0")]
        public decimal? AdditionalAmount { get; set; }


    }
}
