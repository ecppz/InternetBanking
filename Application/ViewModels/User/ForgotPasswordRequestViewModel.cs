using System.ComponentModel.DataAnnotations;

namespace Application.ViewModels.User
{
    public class ForgotPasswordRequestViewModel
    {
        [Required(ErrorMessage = "Debes ingresar el username")]
        [DataType(DataType.Text)]
        public required string UserName { get; set; }      
    }
}
