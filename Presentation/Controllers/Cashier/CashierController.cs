using Application.Interfaces;
using Application.ViewModels.Cashier;
using AutoMapper;
using Domain.Common.Enums;
using Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<UserAccount> userManager;

        // Constructor: recibe las dependencias necesarias mediante inyección.
        public CashierController(
            ISavingsAccountService savingsAccountService,
            IUserAccountService userAccountService,
            ITransactionService transactionService,
            UserManager<UserAccount> userManager,
        IMapper mapper, ILoanService loanService, ICreditCardService creditCardService, ICreditCardTransactionService creditCardTransactionService)
        {
            this.savingsAccountService = savingsAccountService;
            this.userManager = userManager;
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

            UserAccount? userSession = await userManager.GetUserAsync(User);

            if (userSession == null)
            {
                return RedirectToRoute(new { controller = "AccessDenied", action = "Index" });
            }

            var roles = await userManager.GetRolesAsync(userSession);

            if (!roles.Contains(Roles.Cashier.ToString()))
            {
                return RedirectToRoute(new { controller = "AccessDenied", action = "Index" });
            }

            var cashierId = Guid.Parse(userSession.Id);
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

    }
}
