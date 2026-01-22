using Application.Dtos.SavingsAccount;
using Application.Interfaces;
using Application.ViewModels.SavingsAccount;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternetBankingApp.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class SavingsAccountController : Controller
    {
        private readonly ISavingsAccountService savingsAccountService;
        private readonly IMapper mapper;

        public SavingsAccountController(ISavingsAccountService savingsAccountService, IMapper mapper)
        {
            this.savingsAccountService = savingsAccountService;
            this.mapper = mapper;
        }

        // GET: /SavingsAccount/List
        public async Task<IActionResult> List(string? documentNumber, bool? isActive, bool? isPrimary, int page = 1)
        {
            const int pageSize = 10;
            var accounts = await savingsAccountService.GetFilteredAccountsAsync(documentNumber, isActive, isPrimary, page, pageSize);

            var viewModel = new SavingsAccountListViewModel
            {
                Accounts = accounts,
                DocumentNumber = documentNumber,
                IsActive = isActive,
                IsPrimary = isPrimary,
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(accounts.Count / (double)pageSize)
            };

            return View(viewModel);
        }

        // GET: /SavingsAccount/Detail/{id}
        public async Task<IActionResult> Detail(Guid id)
        {
            var account = await savingsAccountService.GetAccountDetailAsync(id);
            if (account == null) return NotFound();

            var viewModel = new SavingsAccountDetailViewModel
            {
                Account = account
            };

            return View(viewModel);
        }

        // GET: /SavingsAccount/CreateSecondary
        public IActionResult CreateSecondary(Guid userId)
        {
            var viewModel = new CreateSavingsAccountViewModel
            {
                UserId = userId
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSecondary(CreateSavingsAccountViewModel viewModel)
        {
            if (!ModelState.IsValid) return View(viewModel);

            var dto = new CreateSavingsAccountDto
            {
                UserId = viewModel.UserId,
                InitialBalance = viewModel.InitialBalance,
                IsPrimary = false // ← blindado aquí
            };

            var success = await savingsAccountService.CreateSecondaryAccountAsync(dto);
            if (!success) return BadRequest();

            return RedirectToAction("List");
        }

        // POST: /SavingsAccount/Cancel/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmCancel(CancelSavingsAccountViewModel viewModel)
        {
            var success = await savingsAccountService.CancelSecondaryAccountAsync(viewModel.AccountId);
            if (!success) return BadRequest();

            return RedirectToAction("List");
        }

        // GET: /SavingsAccount/Cancel/{id}
        [HttpGet]
        public async Task<IActionResult> Cancel(Guid id)
        {
            var account = await savingsAccountService.GetAccountSummaryAsync(id);
            if (account == null || account.IsPrimary)
                return NotFound();

            var viewModel = new CancelSavingsAccountViewModel
            {
                AccountId = account.Id,
                AccountNumber = account.AccountNumber,
                Balance = account.Balance
            };

            return View(viewModel);
        }
    }
}
