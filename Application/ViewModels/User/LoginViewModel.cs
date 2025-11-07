using System.ComponentModel.DataAnnotations;

namespace Application.ViewModels.User
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Debes ingresar el username")]
        [DataType(DataType.Text)]
        public required string UserName { get; set; }

        [Required(ErrorMessage = "Debes ingresar la contraseña")]
        [DataType(DataType.Password)]
        public required string Password { get; set; }
    }
}
