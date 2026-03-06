using Application.Dtos.SavingsAccount;

namespace Application.ViewModels.SavingsAccount
{
    public class SavingsAccountDetailViewModel
    {
        public required SavingsAccountDetailDto Account { get; set; }
        public string OwnerFullName { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
    }
}
