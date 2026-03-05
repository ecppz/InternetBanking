using Application.Interfaces;
using Application.ViewModels.CreditCard;
using Application.ViewModels.HomeCustomerAccounts;
using Application.ViewModels.Loan;
using AutoMapper;
using Domain.Common.Enums;
using Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InternetBankingApp.Controllers.Customer
{
    [Authorize(Roles = "Customer")]
    public class CustomerController : Controller
    {
        private readonly ISavingsAccountService savingsAccountService;
        private readonly ITransactionService transactionlService;
        private readonly ILoanService loanService;
        private readonly ICreditCardService creditCardService;
        private readonly IUserAccountServiceForWebApp userAccountService;
        private readonly UserManager<UserAccount> userManager;
        private readonly IMapper mapper;

        public CustomerController(ISavingsAccountService savingsAccountService, ICreditCardService creditCardService, ITransactionService transactionlService,
            ILoanService loanService, IUserAccountServiceForWebApp userAccountService, UserManager<UserAccount> userManager, IMapper mapper)
        {
            this.savingsAccountService = savingsAccountService;
            this.creditCardService = creditCardService;
            this.loanService = loanService;
            this.transactionlService = transactionlService;
            this.userManager = userManager;
            this.userAccountService = userAccountService;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userSession = await userManager.GetUserAsync(User);
            var userId = Guid.Parse(userSession.Id);

            // cuentas
            var accounts = await savingsAccountService.GetActiveByUserIdAsync(userId);
            var orderedAccounts = accounts
                .OrderByDescending(a => a.IsPrimary)
                .ThenByDescending(a => a.Balance)
                .Select(a => new AccountSummaryViewModel
                {
                    Id = a.Id,
                    AccountNumber = a.AccountNumber,
                    Balance = a.Balance,
                    IsPrimary = a.IsPrimary
                }).ToList();

            // préstamo activo
            var loans = await loanService.GetActiveLoansByUserIdAsync(userId);
            var loan = loans.FirstOrDefault(l => l.Status != LoanStatus.Completed);

            LoanSummaryViewModel? loanVm = null;
            if (loan != null)
            {
                // 👉 aquí obtienes el usuario dueño del préstamo
                var user = await userAccountService.GetUserById(userId.ToString());

                loanVm = new LoanSummaryViewModel
                {
                    Id = loan.Id,
                    LoanNumber = loan.LoanNumber,
                    Amount = loan.Amount,
                    TermMonths = loan.TermMonths,
                    AnnualInterestRate = loan.AnnualInterestRate,
                    TotalInstallments = loan.TotalInstallments,
                    PaidInstallments = loan.PaidInstallments,
                    PendingAmount = loan.PendingAmount,
                    Status = loan.Status,
                    CustomerFullName = $"{user.Name} {user.LastName}",
                    DocumentNumber = user.DocumentNumber
                };
            }

            // tarjeta
            var card = (await creditCardService.GetActiveCardsByUserIdAsync(userId)).FirstOrDefault();
            CreditCardSummaryViewModel? cardVm = null;
            if (card != null)
            {
                cardVm = new CreditCardSummaryViewModel
                {
                    Id = card.Id,
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

        public async Task<IActionResult> LoanDetails(Guid loanId)
        {
            var loanDto = await loanService.GetLoanDetailsAsync(loanId);
            if (loanDto == null)
                return NotFound();

            var loanVm = mapper.Map<LoanDetailsViewModel>(loanDto);

            return View(loanVm);
        }

        public async Task<IActionResult> CreditCardDetails(Guid cardId)
        {
            var cardDto = await creditCardService.GetCardDetailsAsync(cardId);
            if (cardDto == null)
                return NotFound();

            var cardVm = mapper.Map<CreditCardDetailsViewModel>(cardDto);

            return View(cardVm);
        }

    }
}
