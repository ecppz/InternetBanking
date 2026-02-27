
using Microsoft.AspNetCore.Authorization;
using Application.Interfaces;
using Application.ViewModels.AdminDashboard;
using AutoMapper;
using Domain.Common.Enums;
using Microsoft.AspNetCore.Mvc;

namespace InternetBankingApp.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ISavingsAccountService savingsAccountService;
        private readonly ITransactionService transactionService;
        private readonly IUserAccountServiceForWebApp userAccountService;
        private readonly IMapper mapper;
        private readonly ILoanService loanService;
        private readonly ICreditCardService creditCardService;
        private readonly ICreditCardTransactionService creditCardTransactionService;

        // Constructor: recibe las dependencias necesarias mediante inyección.
        public AdminController(
            ISavingsAccountService savingsAccountService,
            IUserAccountServiceForWebApp userAccountService,
            ITransactionService transactionService,
            IMapper mapper,ILoanService loanService, ICreditCardService creditCardService, ICreditCardTransactionService creditCardTransactionService)
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
            var allTransactions = await transactionService.GetAllTransactionsAsync();
            var (totalPayments, todayPayments) = await transactionService.GetLoanAndCreditCardPaymentsAsync();

            var model = new AdminHomeViewModel
            {
                // Indicadores de transacciones
                TotalTransactions = allTransactions.Count,
                TodayTransactions = allTransactions.Count(t => t.Date.Date == DateTime.Today),

                // Indicadores de pagos (inyectados desde el servicio)
                TotalPayments = totalPayments,
                TodayPayments = todayPayments,

                // Indicadores de clientes
                ActiveClients = (await userAccountService.GetAllCustomersAsync()).Count(u => u.IsActive),
                InactiveClients = (await userAccountService.GetAllCustomersAsync()).Count(u => !u.IsActive),

                TotalSavingsAccounts = (await savingsAccountService.GetAllSavingsAccountsAsync())
                    .Count(sa => sa.Status == SavingsAccountStatus.Activa),

                // Placeholders para indicadores pendientes
                ActiveLoans = await loanService.GetActiveLoansCountAsync(),
                ActiveCreditCards = await creditCardService.GetActiveCreditCardsCountAsync(),
                AverageDebtPerClient = (await loanService.GetAverageDebtPerClientAsync()).ToString("C")
            };

            // Retorna la vista con el modelo cargado
            return View(model);
        }


    }
}
