using Application.Interfaces;
using Application.Services;
using Application.ViewModels.AdminDashboard;
using AutoMapper;
using Domain.Common.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    public class AdminHomeController : Controller
    {
        private readonly ISavingsAccountService savingsAccountService;
        private readonly ITransactionService transactionService;
        private readonly IUserAccountService userAccountService;
        private readonly IMapper mapper;
        private readonly ILoanService loanService;
        private readonly ICreditCardService creditCardService;
        private readonly ICreditCardTransactionService creditCardTransactionService;

        // Constructor: recibe las dependencias necesarias mediante inyección.
        public AdminHomeController(
            ISavingsAccountService savingsAccountService,
            IUserAccountService userAccountService,
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

        // Acción principal del Dashboard (GET).
        // Carga los indicadores y los envía a la vista.
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var allTransactions = await transactionService.GetAllTransactionsAsync();
            var (totalPayments, todayPayments) = await creditCardTransactionService.GetPaymentsIndicatorsAsync();

            var model = new AdminDashboardViewModel
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
