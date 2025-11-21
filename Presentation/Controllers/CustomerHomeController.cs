using Application.Interfaces;
using Application.ViewModels.HomeCustomerAccounts;
using AutoMapper;
using Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    public class CustomerHomeController : Controller
    {

        private readonly ISavingsAccountService savingsAccountService;
        private readonly ITransactionService transactionlService;
        private readonly IUserAccountService userAccountService;
        private readonly IMapper mapper;

        public CustomerHomeController(ISavingsAccountService savingsAccountService, IUserAccountService userAccountService, ITransactionService transactionlService, IMapper mapper)
        {
            this.savingsAccountService = savingsAccountService;
            this.transactionlService = transactionlService;
            this.mapper = mapper;
            this.userAccountService = userAccountService;
        }

        [HttpGet]
        public async Task<IActionResult> CustomerHome()
        {
            var userId = await GetAuthenticatedUserIdAsync();

            var accounts = await savingsAccountService.GetActiveByUserIdAsync(userId);

            var orderedAccounts = accounts
        .OrderByDescending(a => a.IsPrimary) //  Principal primero
        .ThenByDescending(a => a.Balance)    // Secundarias por balance
        .Select(a => new AccountSummaryViewModel
        {
            Id = a.Id,
            AccountNumber = a.AccountNumber,
            Balance = a.Balance,
            IsPrimary = a.IsPrimary
        }).ToList();

            var model = new CustomerHomeViewModel
            {
                Accounts = orderedAccounts
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> AccountDetails(Guid accountId)
        {
            var account = await savingsAccountService.GetAccountDetailAsync(accountId);
            if (account == null)
                return NotFound();

            var transactions = await transactionlService.GetAllByAccountIdOrderedAsync(accountId);

            var model = new AccountTransactionDetailViewModel
            {
                AccountNumber = account.AccountNumber,
                Transactions = transactions.Select(tx => new TransactionDetailViewModel
                {
                    Date = tx.Date,
                    Amount = tx.Amount,
                    Type = tx.VisualType,
                    Origin = tx.Origin,
                    Destination = tx.Beneficiary,
                    Status = tx.Status,
                    Description = tx.Description
                }).ToList()
            };

            return View(model);
        }

        private async Task<Guid> GetAuthenticatedUserIdAsync()
        {
            var userName = User?.Identity?.Name;

            if (string.IsNullOrWhiteSpace(userName))
                throw new UnauthorizedAccessException("Usuario no autenticado.");

            var user = await userAccountService.GetUserByUserName(userName);

            if (user == null || string.IsNullOrWhiteSpace(user.Id))
                throw new Exception("No se pudo obtener el usuario autenticado.");

            if (!Guid.TryParse(user.Id, out var userId))
                throw new Exception("El ID del usuario no tiene un formato válido.");

            return userId;
        }



    }
}
