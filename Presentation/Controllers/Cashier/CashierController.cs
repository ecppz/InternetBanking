using Application.Interfaces;
using Application.ViewModels.Cashier;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternetBankingApp.Controllers.Cashier
{
    [Authorize(Roles = "Cashier")]
    public class CashierController : Controller
    {
        private readonly ISavingsAccountService savingsAccountService;
        private readonly ITransactionService transactionService;
        private readonly IUserAccountService userAccountService;
        private readonly IMapper mapper;
        private readonly ILoanService loanService;
        private readonly ICreditCardService creditCardService;
        private readonly ICreditCardTransactionService creditCardTransactionService;

        // Constructor: recibe las dependencias necesarias mediante inyección.
        public CashierController(
            ISavingsAccountService savingsAccountService,
            IUserAccountService userAccountService,
            ITransactionService transactionService,
            IMapper mapper, ILoanService loanService, ICreditCardService creditCardService, ICreditCardTransactionService creditCardTransactionService)
        {
            this.savingsAccountService = savingsAccountService;
            this.transactionService = transactionService;
            this.mapper = mapper;
            this.userAccountService = userAccountService;
            this.loanService = loanService;
            this.creditCardService = creditCardService;
            this.creditCardTransactionService = creditCardTransactionService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var cashierId = await GetAuthenticatedUserIdAsync();
            var today = DateTime.Today;

            var model = new CashierHomeViewModel
            {
               TodayTransactions = await transactionService.GetTransactionsByCashierAndDateAsync(cashierId, today),
               TodayPayments = await transactionService.GetPaymentsCountByCashierAndDateAsync(cashierId, today), 
               TodayDeposits = await transactionService.GetDepositsCountByCashierAndDateAsync(cashierId, today),
               TodayWithdrawals = await transactionService.GetWithdrawalsCountByCashierAndDateAsync(cashierId, today)
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
