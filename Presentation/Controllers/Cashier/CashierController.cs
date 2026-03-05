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
        private readonly ITransactionService transactionService;
        private readonly UserManager<UserAccount> userManager;
        private readonly IMapper mapper;
        public CashierController(ITransactionService transactionService, UserManager<UserAccount> userManager, IMapper mapper)
        {
            this.transactionService = transactionService;
            this.userManager = userManager;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            UserAccount? userSession = await userManager.GetUserAsync(User);

            if (userSession == null)
                return RedirectToRoute(new { controller = "AccessDenied", action = "Index" });

            var roles = await userManager.GetRolesAsync(userSession);
            if (!roles.Contains(Roles.Cashier.ToString()))
                return RedirectToRoute(new { controller = "AccessDenied", action = "Index" });

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
