using Domain.Common.Enums;
using System.ComponentModel.DataAnnotations;

public class SaveUserViewModel
{
    public string? Id { get; set; }

    [Required(ErrorMessage = "Debes ingresar el nombre del usuario")]
    public required string Name { get; set; }

    [Required(ErrorMessage = "Debes ingresar el apellido del usuario")]
    public required string LastName { get; set; }

    [Required(ErrorMessage = "Debes ingresar el email del usuario")]
    [EmailAddress]
    public required string Email { get; set; }

    [Required(ErrorMessage = "Debes ingresar el username del usuario")]
    public required string UserName { get; set; }

    [Required(ErrorMessage = "Debes ingresar la cédula del usuario")]

    [RegularExpression(@"^\d{3}-\d{7}-\d{1}$|^\d{11}$", ErrorMessage = "Formato de cédula inválido")]
    public required string DocumentNumber { get; set; }

    [Required(ErrorMessage = "Debes ingresar la contraseña")]
    [DataType(DataType.Password)]
    public required string Password { get; set; }

    [Required(ErrorMessage = "Debes confirmar la contraseña")]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden")]
    [DataType(DataType.Password)]
    public required string ConfirmPassword { get; set; }

    [Required(ErrorMessage = "Debes seleccionar el tipo de usuario")]

    public required Roles Role { get; set; }

    [DataType(DataType.Currency)]
    public decimal? CurrentBalance { get; set; }
}
