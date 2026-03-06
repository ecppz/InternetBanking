using Application.Dtos.CreditCardTransaction.CashAdvance;
using Application.Dtos.Email;
using Application.Interfaces;
using Application.ViewModels.CreditCardTransaction.CashAdvance;
using AutoMapper;
using Domain.Common.Enums;
using Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InternetBankingApp.Controllers.Customer
{
    [Authorize(Roles = "Customer")]
    public class CashAdvanceController : Controller
    {
        private readonly ICreditCardService creditCardService;
        private readonly ICashAdvanceService cashAdvanceService;
        private readonly ISavingsAccountService savingsAccountService;
        private readonly IUserAccountServiceForWebApp userAccountService;
        private readonly UserManager<UserAccount> userManager;
        private readonly IEmailService emailService;
        private readonly IMapper mapper;

        public CashAdvanceController(ICreditCardService creditCardService, ICashAdvanceService cashAdvanceService, ISavingsAccountService savingsAccountService,
            IUserAccountServiceForWebApp userAccountService, UserManager<UserAccount> userManager, IEmailService emailService,  IMapper mapper)
        {
            this.creditCardService = creditCardService;
            this.cashAdvanceService = cashAdvanceService;
            this.savingsAccountService = savingsAccountService;
            this.userAccountService = userAccountService;
            this.userManager = userManager;
            this.emailService = emailService;
            this.mapper = mapper;
        }
        public async Task<IActionResult> Index()
        {
            var userSession = await userManager.GetUserAsync(User);
            if (userSession == null)
            {
                return RedirectToRoute(new { controller = "AccessDenied", action = "Index" });
            }

            var userId = Guid.Parse(userSession.Id);

            var cards = await creditCardService.GetActiveCardsByUserIdAsync(userId);
            var accounts = await savingsAccountService.GetActiveAccountsByUserIdAsync(userId);

            ViewBag.CreditCards = new SelectList(cards, "Id", "CardNumber");
            ViewBag.SavingsAccounts = new SelectList(accounts, "Id", "AccountNumber");

            return View(new CashAdvanceViewModel());
        }


        [HttpPost]
        public async Task<IActionResult> Index(CashAdvanceViewModel vm)
        {
            var userSession = await userManager.GetUserAsync(User);
            if (userSession == null)
            {
                return RedirectToRoute(new { controller = "AccessDenied", action = "Index" });
            }

            var userId = Guid.Parse(userSession.Id);

            ViewBag.CreditCards = new SelectList(await creditCardService.GetActiveCardsByUserIdAsync(userId),
                                       "Id", "CardNumber"
            );
            ViewBag.SavingsAccounts = new SelectList(await savingsAccountService.GetActiveAccountsByUserIdAsync(userId),
                                       "Id", "AccountNumber"
            );

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var card = await creditCardService.GetCardDetailsAsync(vm.CreditCardId);
            if (card == null || card.Status != CreditCardStatus.Active)
            {
                ModelState.AddModelError("", "La tarjeta de crédito está inactiva o no existe.");
                return View(vm);
            }

            var account = await savingsAccountService.GetAccountSummaryAsync(vm.SavingsAccountId);
            if (account == null || !account.IsActive)
            {
                ModelState.AddModelError("", "La cuenta de ahorro destino está inactiva o no existe.");
                return View(vm);
            }

            if (card.UserId != account.UserId)
            {
                ModelState.AddModelError("", "La tarjeta y la cuenta destino no pertenecen al mismo titular.");
                return View(vm);
            }

            if (vm.Amount <= 0)
            {
                ModelState.AddModelError("", "El monto del avance debe ser mayor a cero.");
                return View(vm);
            }

            var availableCredit = card.CreditLimit - card.CurrentDebt;
            if (vm.Amount > availableCredit)
            {   
                ModelState.AddModelError("", "El monto excede el crédito disponible.");
                return View(vm);
            }

            var user = await userAccountService.GetUserById(card.UserId.ToString());

            if (user == null)
            {
                ModelState.AddModelError("", "El usuario asociado no existe.");
                return View(vm);
            }

            var confirmVm = new CashAdvanceConfirmationViewModel
            {
                UserId = card.UserId,
                CreditCardId = card.CreditCardId,
                SavingsAccountId = account.Id,
                CreditCardNumber = card.CardNumber,
                SavingsAccountNumber = account.AccountNumber,
                HolderName = user.Name,
                HolderLastName = user.LastName,
                AdvanceAmount = vm.Amount,
                InterestApplied = vm.Amount * 0.0625m,
                TransactionDate = DateTime.UtcNow,
            };

            return View("Confirm", confirmVm);
        }
        [HttpPost]
        public async Task<IActionResult> Confirm(CashAdvanceConfirmationViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var dto = new CashAdvanceDto
            {
                CreditCardId = vm.CreditCardId,
                SavingsAccountId = vm.SavingsAccountId,
                Amount = vm.AdvanceAmount,
                Date = DateTime.UtcNow,
                UserId = vm.UserId
            };

            var result = await cashAdvanceService.RegisterCashAdvanceAsync(dto);

            if (result == null)
            {
                TempData["ErrorMessage"] = "No se pudo registrar el avance de efectivo.";
                return View(vm);
            }

            var card = await creditCardService.GetById(result.CreditCardId);
            var account = await savingsAccountService.GetById(result.SavingsAccountId);

            var user = await userAccountService.GetUserById(result.UserId.ToString());

            if (user != null && card != null && account != null)
            {
                var interest = result.Amount * 0.0625m;

                await emailService.SendAsync(new EmailRequestDto
                {
                    To = user.Email,
                    Subject = $"Avance de efectivo registrado - ****{card.CardNumber.Substring(card.CardNumber.Length - 4)}",
                    HtmlBody = $@"
                <p>Estimado {user.Name},</p>
                <p>Hemos registrado correctamente su avance de efectivo con los siguientes detalles:</p>
                <ul>
                    <li><b>Número de tarjeta:</b> ****{card.CardNumber.Substring(card.CardNumber.Length - 4)}</li>
                    <li><b>Monto del avance:</b> {result.Amount:C}</li>
                    <li><b>Cuenta destino:</b> ****{account.AccountNumber.Substring(account.AccountNumber.Length - 4)}</li>
                    <li><b>Fecha de transacción:</b> {DateTime.UtcNow:dd/MM/yyyy HH:mm}</li>
                    <li><b>Interés aplicado:</b> {interest:C}</li>
                </ul>
                <p>Gracias por confiar en nosotros para sus operaciones financieras.</p>"
                });
            }

            TempData["SuccessMessage"] = "El avance de efectivo se ha confirmado correctamente.";
            return RedirectToAction("Index", "CustomerHome");
        }

    }
}
