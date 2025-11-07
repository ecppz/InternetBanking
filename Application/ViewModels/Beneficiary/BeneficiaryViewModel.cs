
namespace Application.ViewModels.Beneficiary
{
    public class BeneficiaryViewModel
    {
        public Guid Id { get; set; }
        public Guid OwnerUserId { get; set; }
        public required string Name { get; set; }
        public required string DocumentNumber { get; set; }
        public required string Email { get; set; }
    }
}
