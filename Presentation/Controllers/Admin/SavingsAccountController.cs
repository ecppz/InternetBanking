using Application.Dtos.SavingsAccount;
using Application.Interfaces;
using Application.ViewModels.SavingsAccount;
using AutoMapper;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternetBankingApp.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class SavingsAccountController : Controller
    {
        private readonly ISavingsAccountService savingsAccountService;
        private readonly IUserAccountServiceForWebApp userAccountService;
        private readonly IMapper mapper;

        public SavingsAccountController(ISavingsAccountService savingsAccountService, IUserAccountServiceForWebApp userAccountService, IMapper mapper)
        {
            this.savingsAccountService = savingsAccountService;
            this.userAccountService = userAccountService;
            this.mapper = mapper;
        }

        // GET: /SavingsAccount/List
        public async Task<IActionResult> List(string? documentNumber, bool? isActive, bool? isPrimary, int page = 1)
        {
            const int pageSize = 10;
            var accounts = await savingsAccountService.GetFilteredAccountsAsync(documentNumber, isActive, isPrimary, page, pageSize);

            var userIds = accounts.Select(a => a.UserId).Distinct().ToList();
            var users = await userAccountService.GetUsersByIds(userIds);

            foreach (var account in accounts)
            {
                var user = users.FirstOrDefault(u => u.Id == account.UserId.ToString());
                account.OwnerFullName = user != null ? $"{user.Name} {user.LastName}".Trim() : "Desconocido";
                account.DocumentNumber = user?.DocumentNumber ?? "N/D";
            }

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

            var user = await userAccountService.GetUserById(account.UserId.ToString());
            if (user == null) return NotFound();

            var viewModel = new SavingsAccountDetailViewModel
            {
                Account = account,
                OwnerFullName = $"{user.Name?.Trim()} {user.LastName?.Trim()}".Trim(),
                DocumentNumber = user.DocumentNumber
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmCancel(CancelSavingsAccountViewModel viewModel)
        {
            var userId = await savingsAccountService.CancelSecondaryAccountAsync(viewModel.AccountId);
            if (userId == null) return BadRequest();

            await userAccountService.SetUserActiveStatus(userId.Value.ToString(), true);

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
