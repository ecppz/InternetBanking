using Application.Dtos.Loan;
using Application.Interfaces;
using Application.ViewModels.HomeCustomerAccounts;
using AutoMapper;
using Domain.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternetBankingApp.Controllers.Customer
{
    [Authorize(Roles = "Customer")]
    public class CustomerController : Controller
    {
        private readonly ISavingsAccountService savingsAccountService;
        private readonly ITransactionService transactionlService;
        private readonly IUserAccountService userAccountService;
        private readonly ILoanService loanService;
        private readonly ICreditCardService creditCardService;
        private readonly IMapper mapper;

        public CustomerController(ISavingsAccountService savingsAccountService, ICreditCardService creditCardService, IUserAccountService userAccountService, ITransactionService transactionlService,
            ILoanService loanService, IMapper mapper)
        {
            this.savingsAccountService = savingsAccountService;
            this.transactionlService = transactionlService;
            this.loanService = loanService;
            this.creditCardService = creditCardService;
            this.mapper = mapper;
            this.userAccountService = userAccountService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = await GetAuthenticatedUserIdAsync();

            // cuentas
            var accounts = await savingsAccountService.GetActiveByUserIdAsync(userId);
            var orderedAccounts = accounts
                .OrderByDescending(a => a.IsPrimary) // principal primero
                .ThenByDescending(a => a.Balance)    // secundarias por balance
                .Select(a => new AccountSummaryViewModel
                {
                    Id = a.Id,
                    AccountNumber = a.AccountNumber,
                    Balance = a.Balance,
                    IsPrimary = a.IsPrimary
                }).ToList();

            // préstamo activo (solo uno)
            var loan = (await loanService.GetActiveLoansByUserIdAsync(userId)).FirstOrDefault();
            LoanDisplayDto? loanVm = null;
            if (loan != null)
            {
                // 👉 aquí usamos loan.LoanNumber en vez de loanNumber
                var loanDto = await loanService.GetLoanByNumberAsync(loan.LoanNumber);

                loanVm = new LoanDisplayDto
                {
                    Id = loanDto.LoanId,
                    LoanNumber = loanDto.LoanNumber,
                    CustomerFullName = loanDto.CustomerFullName,
                    Amount = loanDto.Amount,
                    TermMonths = loanDto.TermMonths,
                    AnnualInterestRate = loanDto.AnnualInterestRate,
                    TotalInstallments = loanDto.InstallmentsDetails.Count,
                    PaidInstallments = loanDto.InstallmentsDetails.Count(i => i.Status == InstallmentStatus.Paid),
                    PendingAmount = loanDto.InstallmentsDetails.Where(i => i.Status != InstallmentStatus.Paid).Sum(i => i.Amount),
                    Status = loanDto.Status,
                    // quita CreatedAt si tu DTO no lo tiene
                };
            }

            // tarjeta activa (solo una)
            var card = (await creditCardService.GetActiveCardsByUserIdAsync(userId)).FirstOrDefault();
            CreditCardSummaryViewModel? cardVm = null;
            if (card != null)
            {
                cardVm = new CreditCardSummaryViewModel
                {
                    CardNumber = card.CardNumber,
                    CreditLimit = card.CreditLimit,
                    ExpirationDate = card.ExpirationDate,
                    Debt = card.CurrentDebt
                };
            }

            var model = new CustomerHomeViewModel
            {
                Accounts = orderedAccounts,
                Loan = loanVm,
                CreditCard = cardVm
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

        public async Task<IActionResult> LoanDetails(string loanNumber)
        {
            var loan = await loanService.GetLoanByNumberAsync(loanNumber);
            if (loan == null)
            {
                TempData["ErrorMessage"] = "El préstamo no existe.";
                return RedirectToAction("Index");
            }

            return View(loan);
        }



    }
}
