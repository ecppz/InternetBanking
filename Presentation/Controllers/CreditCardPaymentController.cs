using Application.Dtos.CreditCardTransaction;
using Application.Interfaces;
using Application.ViewModels.CreditCardTransaction;
using AutoMapper;
using Domain.Common.Enums;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [Authorize(Roles = "Cashier")]
    public class CreditCardPaymentController : Controller
    {
        private readonly ICreditCardService creditCardService;
        private readonly ICreditCardTransactionService creditCardTransactionService;
        private readonly ISavingsAccountService savingsAccountService;
        private readonly IUserAccountService userAccountService;
        private readonly IMapper mapper;

        public CreditCardPaymentController(ICreditCardService creditCardService, ICreditCardTransactionService creditCardTransactionService, 
            ISavingsAccountService savingsAccountService, IUserAccountService userAccountService, IMapper mapper)
        {
            this.creditCardService = creditCardService;
            this.creditCardTransactionService = creditCardTransactionService;
            this.savingsAccountService = savingsAccountService;
            this.userAccountService = userAccountService;
            this.mapper = mapper;
        }

        public IActionResult Index()
        {
            var vm = new CreditCardPaymentViewModel
            {
                OriginAccountNumber = "",
                Amount = 0,
                CreditCardNumber = "",
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Index(CreditCardPaymentViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

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

            if (originAccount.Balance < vm.Amount)
            {
                ModelState.AddModelError("", "Fondos insuficientes en la cuenta de origen.");
                return View(vm);
            }

            var cardId = await creditCardService.GetCardIdByNumberAsync(vm.CreditCardNumber);
            if (cardId == null)
            {
                ModelState.AddModelError("", "La tarjeta de crédito no existe.");
                return View(vm);
            }

            var creditCard = await creditCardService.GetCardDetailsAsync(cardId.Value);

            if (creditCard == null || creditCard.Status != CreditCardStatus.Active)
            {
                ModelState.AddModelError("", "La tarjeta de crédito está inactiva.");
                return View(vm);
            }

            if (creditCard.UserId != originAccount.UserId)
            {
                ModelState.AddModelError("", "La cuenta origen no pertenece al titular de la tarjeta.");
                return View(vm);
            }

            if (vm.Amount <= 0)
            {
                ModelState.AddModelError("", "El monto a pagar debe ser mayor a cero.");
                return View(vm);
            }

            var user = await userAccountService.GetUserById(creditCard.UserId.ToString());

            if (user == null)
            {
                ModelState.AddModelError("", "El usuario asociado al préstamo no existe.");
                return View(vm);
            }

            var confirmVm = mapper.Map<CreditCardPaymentConfirmationViewModel>(creditCard);

            confirmVm.UserId = creditCard.UserId;
            confirmVm.HolderName = user.Name;
            confirmVm.HolderLastName = user.LastName;
            confirmVm.CreditCardNumber = vm.CreditCardNumber;
            confirmVm.OriginAccountNumber = vm.OriginAccountNumber;
            confirmVm.PaymentAmount = vm.Amount;
            confirmVm.TransactionDate = DateTime.UtcNow;

            return View("Confirm", confirmVm);
        }

        [HttpPost]
        public async Task<IActionResult> Confirm(CreditCardPaymentConfirmationViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var transactionDto = new CreditCardTransactionDto
            {
                CreditCardId = vm.CreditCardId,
                TransactionOrigin = vm.OriginAccountNumber,
                Amount = vm.PaymentAmount,
                Date = DateTime.UtcNow,
                Status = TransactionStatus.Approved
            };

            var result = await creditCardTransactionService.RegisterPaymentAsync(transactionDto);

            if (result == null)
            {
                TempData["ErrorMessage"] = "No se pudo registrar el pago.";
                return View(vm);
            }

            TempData["SuccessMessage"] = "El pago se ha confirmado correctamente.";
            return RedirectToAction("Index");
        }

    }
}
