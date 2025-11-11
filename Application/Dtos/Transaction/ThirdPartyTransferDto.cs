
using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.Transaction
{
    public class ThirdPartyTransferDto
    {
        [Required]
        public string OriginAccountNumber { get; set; } = string.Empty;

        [Required]
        [Range(1, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public string DestinationAccountNumber { get; set; } = string.Empty;
    }
}
