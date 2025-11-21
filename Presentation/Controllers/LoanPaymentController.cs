using Application.Dtos.Loan;
using Application.Interfaces;
using Application.ViewModels.Loan;
using AutoMapper;
using Domain.Common.Enums;
using Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [Authorize(Roles = "Cashier")]
    public class LoanPaymentController : Controller
    {
        private readonly ILoanService loanService;
        private readonly ILoanPaymentService loanPaymentService;
        private readonly ISavingsAccountService savingsAccountService;
        private readonly IUserAccountService userAccountService;
        private readonly IMapper mapper;

        public LoanPaymentController(ILoanService loanService, ILoanPaymentService loanPaymentService, ISavingsAccountService savingsAccountService,
            IUserAccountService userAccountService, IMapper mapper)
        {
            this.loanService = loanService;
            this.loanPaymentService = loanPaymentService;
            this.savingsAccountService = savingsAccountService;
            this.userAccountService = userAccountService;
            this.mapper = mapper;
        }

        public IActionResult Index()
        {
            var vm = new LoanPaymentViewModel
            {
                OriginAccountNumber = "",
                Amount = 0,
                LoanNumber = "",
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Index(LoanPaymentViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var accountId = await savingsAccountService.GetAccountIdByAccountNumberAsync(vm.OriginAccountNumber);
            if (accountId == null)
            {
                ModelState.AddModelError("", "La cuenta de origen no existe.");
                return View(vm);
            }

            var originAccount = await savingsAccountService.GetAccountSummaryAsync(accountId.Value);
            if (originAccount == null || !originAccount.IsActive)
            {
                ModelState.AddModelError("", "La cuenta de origen está inactiva.");
                return View(vm);
            }

            var loanId = await loanService.GetLoanByNumberAsync(vm.LoanNumber);

            if (loanId == null)
            {
                ModelState.AddModelError("", "El préstamo no existe.");
                return View(vm);
            }

            var loan = await loanService.GetLoanByNumberAsync(vm.LoanNumber);

            if (loan == null || loan.Status == LoanStatus.Completed)
            {
                ModelState.AddModelError("", "El préstamo no existe o está completado.");
                return View(vm);
            }


            if (originAccount.Balance < vm.Amount)
            {
                ModelState.AddModelError("", "Fondos insuficientes en la cuenta de origen.");
                return View(vm);
            }

            if (loan.UserId != originAccount.UserId)
            {
                ModelState.AddModelError("", "La cuenta origen no pertenece al titular del préstamo.");
                return View(vm);
            }

            if (vm.Amount <= 0)
            {
                ModelState.AddModelError("", "El monto a pagar debe ser mayor a cero.");
                return View(vm);
            }

            var user = await userAccountService.GetUserById(loan.UserId.ToString());

            if (user == null)
            {
                ModelState.AddModelError("", "El usuario asociado al préstamo no existe.");
                return View(vm);
            }

            var confirmVm = mapper.Map<LoanPaymentConfirmationViewModel>(loan);

            confirmVm.UserId = loan.UserId;
            confirmVm.HolderName = user.Name;
            confirmVm.HolderLastName = user.LastName;
            confirmVm.OriginAccountId = originAccount.Id;
            confirmVm.OriginAccountNumber = vm.OriginAccountNumber; 
            confirmVm.PaymentAmount = vm.Amount;
            confirmVm.TransactionDate = DateTime.UtcNow;

            return View("Confirm", confirmVm);

        }

        [HttpPost]
        public async Task<IActionResult> Confirm(LoanPaymentConfirmationViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var result = await loanPaymentService.RegisterPaymentAsync(new LoanPaymentDto
            {
                OriginAccountNumber = vm.OriginAccountNumber,
                Amount = vm.PaymentAmount,
                LoanNumber = vm.LoanNumber,
                UserId = vm.UserId
            });

            if (result == null)
            {
                TempData["ErrorMessage"] = "No se pudo registrar el pago. Datos inválidos o usuario no encontrado.";
                return RedirectToAction("Index");
            }

            TempData["SuccessMessage"] = "El pago del préstamo se ha confirmado correctamente.";
            return RedirectToAction("Index");
        }
    }

}
