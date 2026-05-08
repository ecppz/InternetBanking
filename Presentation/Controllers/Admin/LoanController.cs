using Application.Dtos.Loan;
using Application.Dtos.User;
using Application.Interfaces;
using Application.ViewModels.Loan;
using AutoMapper;
using Domain.Common.Enums;
using Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace InternetBankingApp.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class LoanController : Controller
    {
        private readonly ILoanService loanService;
        private readonly IUserAccountServiceForWebApp userAccountService;
        private readonly UserManager<UserAccount> userManager;
        private readonly IMapper mapper;

        public LoanController(ILoanService loanService, IUserAccountServiceForWebApp userAccountService, UserManager<UserAccount> userManager, IMapper mapper)
        {
            this.loanService = loanService;
            this.userAccountService = userAccountService;
            this.userManager = userManager;
            this.mapper = mapper;
        }

        public async Task<IActionResult> Index(string? documentNumber, string? statusFilter)
        {
            var allUsers = await userAccountService.GetAllActiveUsers();
            var dtos = await loanService.GetAllDisplayAsync(allUsers, documentNumber, statusFilter);

            ViewBag.CurrentFilter = documentNumber;
            ViewBag.StatusFilter = statusFilter;

            var vms = mapper.Map<List<LoanDisplayViewModel>>(dtos);
            return View(vms);
        }


        public IActionResult RiskWarning()
        {
            return View();
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var dto = await loanService.GetLoanDetailsAsync(id);
            if (dto == null)
            {
                return NotFound();
            }

            var loan = await loanService.GetById(id);
            var user = await userAccountService.GetUserById(loan.UserId.ToString());

            var vm = mapper.Map<LoanDetailsViewModel>(dto);

            vm.HolderName = user.Name;
            vm.HolderLastName = user.LastName;
            vm.DocumentNumber = user.DocumentNumber;
            vm.Email = user.Email;

            return View(vm);
        }

        public async Task<IActionResult> EligibleCustomersForLoan(string? documentNumber)
        {
            var allUsers = await userAccountService.GetAllActiveUsers();
            var customers = new List<UserDto>();

            foreach (var user in allUsers)
            {
                var roles = await userAccountService.GetUserRolesAsync(Guid.Parse(user.Id));
                if (roles.Contains(Roles.Customer.ToString()) && user.IsActive)
                {
                    customers.Add(user);
                }
            }

            var eligibleCustomers = await loanService.GetEligibleCustomersForLoan(customers);

            if (!string.IsNullOrWhiteSpace(documentNumber))
            {
                eligibleCustomers = eligibleCustomers
                    .Where(c => c.DocumentNumber.Contains(documentNumber))
                    .ToList();

                ViewData["CurrentFilter"] = documentNumber;
            }

            var avgDebt = await loanService.GetAverageDebtAsync(allUsers);
            ViewData["AverageDebt"] = avgDebt;

            var vms = mapper.Map<List<EligibleCustomerForLoanViewModel>>(eligibleCustomers);
            return View(vms);
        }


        public async Task<IActionResult> Edit(Guid id)
        {
            var dto = await loanService.GetById(id);
            if (dto == null)
            {
                return RedirectToAction("Index");
            }

            var vm = mapper.Map<EditLoanViewModel>(dto); 
            return View("Edit", vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditLoanViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var dto = mapper.Map<EditLoanDto>(vm);
            
            var user = await userAccountService.GetUserById(dto.UserId.ToString());

            var response = await loanService.UpdateInterestRateAsync(dto, user);

            if (!response.Success)
            {
                TempData["Error"] = "No se pudo actualizar la tasa de interés.";
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }


        public async Task<IActionResult> AssignLoan(Guid userId)
        {
            var allUsers = await userAccountService.GetAllActiveUsers();

            if (userId == Guid.Empty)
            {
                TempData["Error"] = "debes seleccionar un cliente antes de continuar";
                return RedirectToAction("EligibleCustomers");
            }

            var user = await userAccountService.GetUserById(userId.ToString());
            if (user == null || !user.IsActive)
            {
                TempData["Error"] = "el cliente seleccionado no es válido o está inactivo";
                return RedirectToAction("EligibleCustomers");
            }

            var vm = new AssignLoanViewModel
            {
                UserId = userId,
                DocumentNumber = user.DocumentNumber,
                FullName = $"{user.Name} {user.LastName}",
                Email = user.Email
            };

            var avgDebt = await loanService.GetAverageDebtAsync(allUsers);
            ViewData["AverageDebt"] = avgDebt;

            return View("AssignLoan", vm);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmLoanAssignment(AssignLoanViewModel vm)
        {
            var allUsers = await userAccountService.GetAllActiveUsers();

            if (!ModelState.IsValid)
            {
                ViewData["AverageDebt"] = await loanService.GetAverageDebtAsync(allUsers);
                return View("AssignLoan", vm);
            }

            var user = await userAccountService.GetUserById(vm.UserId.ToString());
            if (user == null || !user.IsActive)
            {
                TempData["Error"] = "El cliente seleccionado no es válido o está inactivo.";
                return RedirectToAction("EligibleCustomers");
            }

            var averageDebt = await loanService.GetAverageDebtAsync(allUsers);
            var currentDebt = user.CurrentBalance ?? 0m;

            var interest = vm.Amount * (vm.AnnualRate / 100) * (vm.Months / 12m);
            var totalNewLoan = vm.Amount + interest;
            var totalProjectedDebt = currentDebt + totalNewLoan;

            if (currentDebt > averageDebt)
            {
                TempData["RiskMessage"] = "Este cliente se considera de alto riesgo, ya que su deuda actual supera el promedio del sistema.";
                TempData["UserId"] = vm.UserId.ToString();
                TempData["Amount"] = vm.Amount.ToString(CultureInfo.InvariantCulture);
                TempData["Months"] = vm.Months.ToString();
                TempData["AnnualRate"] = vm.AnnualRate.ToString(CultureInfo.InvariantCulture);
                return RedirectToAction("RiskWarning");
            }

            if (totalProjectedDebt > averageDebt)
            {
                TempData["RiskMessage"] = "Asignar este préstamo convertirá al cliente en un cliente de alto riesgo, ya que su deuda superará el umbral promedio del sistema.";
                TempData["UserId"] = vm.UserId.ToString();
                TempData["Amount"] = vm.Amount.ToString(CultureInfo.InvariantCulture);
                TempData["Months"] = vm.Months.ToString();
                TempData["AnnualRate"] = vm.AnnualRate.ToString(CultureInfo.InvariantCulture);
                return RedirectToAction("RiskWarning");
            }

            var dto = new CreateLoanDto
            {
                UserId = vm.UserId,
                Amount = vm.Amount,
                TermMonths = vm.Months,
                AnnualInterestRate = vm.AnnualRate
            };

            var response = await loanService.CreateLoanAsync(dto, user);
            if (!response.Success)
            {
                TempData["Error"] = response.Message;
                return RedirectToAction("EligibleCustomers");
            }

            return RedirectToAction("Index");
        }   

        [HttpPost]
        public async Task<IActionResult> FinalizeLoanAssignment(string userId, string amount, string months, string annualRate)
        {
            var admin = await userManager.GetUserAsync(User);
            var user = await userAccountService.GetUserById(userId.ToString());

            var dto = new CreateLoanDto
            {
                UserId = Guid.Parse(userId),
                Amount = decimal.Parse(amount, CultureInfo.InvariantCulture),
                TermMonths = int.Parse(months),
                AnnualInterestRate = decimal.Parse(annualRate, CultureInfo.InvariantCulture),
            };

            var response = await loanService.CreateLoanAsync(dto, user);

            if (!response.Success)
            {
                TempData["Error"] = response.Message;
                return RedirectToAction("EligibleCustomers");
            }

            return RedirectToAction("Index");
        }
    }
}
