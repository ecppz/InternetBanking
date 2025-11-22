using Application.Dtos.CreditCardTransaction;
using Application.Dtos.Loan;
using Application.Dtos.SavingsAccount;
using Application.Dtos.Transaction;
using Application.Interfaces;
using Application.Services;
using Application.ViewModels.CreditCardTransaction;
using Application.ViewModels.ExpressTransaction;
using Application.ViewModels.Loan;
using Application.ViewModels.TransactionBeneficiaryTransfer;
using AutoMapper;
using Domain.Common.Enums;
using Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using System.Security.Principal;
namespace Presentation.Controllers
{
    [Authorize(Roles = "Customer")]
    public class TransactionCustomerController : Controller
    {
        private readonly IMapper mapper;
        private readonly ITransactionService transactionService;
        private readonly ISavingsAccountService savingsAccountService;
        private readonly IUserAccountService userAccountService;
        private readonly IBeneficiaryService beneficiaryService;
        private readonly UserManager<UserAccount> userManager;
        private readonly ICreditCardService creditCardService;
        private readonly ILoanService loanService;
        private readonly ICreditCardTransactionService creditCardTransactionService;
        private readonly ILoanPaymentService loanPaymentService;
        public TransactionCustomerController(
               IMapper mapper,
               ITransactionService transactionService,
               ISavingsAccountService savingsAccountService,
               IUserAccountService userAccountService, IBeneficiaryService beneficiaryService,
               UserManager<UserAccount> userManager,
               ICreditCardService creditCardService,
               ICreditCardTransactionService creditCardTransactionService,
               ILoanService loanService,
               ILoanPaymentService loanPaymentService)
        {
            this.mapper = mapper;
            this.transactionService = transactionService;
            this.savingsAccountService = savingsAccountService;
            this.userAccountService = userAccountService;
            this.beneficiaryService = beneficiaryService;
            this.userManager = userManager;
            this.creditCardService = creditCardService;
            this.creditCardTransactionService = creditCardTransactionService;
            this.loanService = loanService;
            this.loanPaymentService = loanPaymentService;

        }

        [HttpGet]
        public async Task<IActionResult> Express()
        {
            var userId = await GetAuthenticatedUserIdAsync();
            var allAccounts = await savingsAccountService.GetAllByUserIdOrderedAsync(userId);

            var accounts = allAccounts
                .Where(a => a.Status == SavingsAccountStatus.Activa)
                .ToList(); // Filtrado correcto por estado

            var model = new ExpressTransactionFormViewModel
            {
                OriginAccounts = accounts.Select(a => new AccountOptionViewModelExpressTransaction
                {
                    Id = a.Id,
                    AccountNumber = a.AccountNumber
                }).ToList(),
                ShowAccountWarning = !accounts.Any()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Express(ExpressTransactionFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = await GetAuthenticatedUserIdAsync();

            // Obtener cuenta destino
            var destinationAccount = await savingsAccountService.GetByAccountNumberAsync(model.DestinationAccountNumber);
            if (destinationAccount == null || destinationAccount.Status != SavingsAccountStatus.Activa)
            {
                ModelState.AddModelError("", "La cuenta destino no es válida o está inactiva.");
                return View(model);
            }

            //  Validación: no puede transferirse a sí mismo
            if (destinationAccount.UserId == userId)
            {
                ModelState.AddModelError("", "No puedes transferirte a ti mismo. Usa la opción de Transferencias Internas.");
                return View(model);
            }

            //  Validación: no puede transferir a cuenta secundaria
            if (!destinationAccount.IsPrimary)
            {
                ModelState.AddModelError("", "No puedes transferir a una cuenta secundaria. Usa la opción de Transferencias Internas.");
                return View(model);
            }

            // Validar fondos
            var hasFunds = await transactionService.HasSufficientFundsAsync(model.OriginAccountNumber, model.Amount);
            if (!hasFunds)
            {
                ModelState.AddModelError("", "Fondos insuficientes en la cuenta de origen.");
                return View(model);
            }

            // Preparar confirmación
            var confirmation = await transactionService.PrepareTransferConfirmationAsync(
                model.OriginAccountNumber,
                model.DestinationAccountNumber,
                model.Amount
            );

            TempData["TransferData"] = JsonConvert.SerializeObject(confirmation);
            return View("ConfirmExpress", confirmation);
        }


        [HttpPost]
        public async Task<IActionResult> ConfirmExpress(string action)
        {
            if (action == "cancelar")
                return RedirectToAction("Index", "CustomerHome");

            var json = TempData["TransferData"] as string;
            if (string.IsNullOrEmpty(json))
                return RedirectToAction("Express");

            var model = JsonConvert.DeserializeObject<ConfirmThirdPartyTransferCustomerViewModel>(json);

            var success = await transactionService.ExecuteThirdPartyTransferAsync(
                model.OriginAccountNumber,
                model.DestinationAccountNumber,
                model.Amount
            );

            if (!success)
            {
                await transactionService.RegisterRejectedTransactionAsync(
                    model.OriginAccountNumber,
                    model.DestinationAccountNumber,
                    model.Amount,
                    "Error inesperado en la ejecución"
                );

                TempData["Error"] = "La transacción no pudo completarse.";
                return RedirectToAction("Express");
            }

            return RedirectToAction("Express");
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

        // Trasaccion para beneficiario registrado 

        [HttpGet]
        public async Task<IActionResult> BeneficiaryTransfer()
        {
            var userId = await GetAuthenticatedUserIdAsync();

            var beneficiaries = (await beneficiaryService.GetByOwnerUserIdAsync(userId))
              .Select(b => new SelectListItem
              {
                  Value = b.BeneficiaryAccountNumber,
                  Text = $"{b.Name} {b.LastName}"
              }).ToList();

            var model = new BeneficiaryTransferFormViewModel
            {
                Beneficiaries = beneficiaries,
                OriginAccounts = (await savingsAccountService.GetActiveByUserIdAsync(userId))
                    .Select(a => new AccountOptionViewModelBeneficiary
                    {
                        Id = a.Id,
                        AccountNumber = a.AccountNumber
                    }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> BeneficiaryTransfer(BeneficiaryTransferFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await RefillFormAsync(model);
                return View(model);
            }

            var originAccount = await savingsAccountService.GetAccountSummaryAsync(model.OriginAccountId);
            if (originAccount == null || originAccount.Balance < model.Amount)
            {
                ModelState.AddModelError("", "Fondos insuficientes en la cuenta de origen.");
                await RefillFormAsync(model);
                return View(model);
            }

            var userId = await GetAuthenticatedUserIdAsync();
            var beneficiary = await beneficiaryService.GetByAccountNumberAndOwnerAsync(userId, model.BeneficiaryAccountNumber);
            if (beneficiary == null)
            {
                ModelState.AddModelError("", "El beneficiario seleccionado no es válido.");
                await RefillFormAsync(model);
                return View(model);
            }

            var confirmation = new ConfirmBeneficiaryTransferViewModel
            {
                OriginAccountId = model.OriginAccountId,
                OriginAccountNumber = originAccount.AccountNumber,
                BeneficiaryAccountNumber = beneficiary.BeneficiaryAccountNumber,
                BeneficiaryFullName = $"{beneficiary.Name} {beneficiary.LastName}",
                Amount = model.Amount,
                Timestamp = DateTime.Now
            };

            TempData["BeneficiaryTransferData"] = JsonConvert.SerializeObject(confirmation);
            return View("ConfirmBeneficiaryTransfer", confirmation);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmBeneficiaryTransfer()
        {
            if (!TempData.ContainsKey("BeneficiaryTransferData"))
                return RedirectToAction("Home", "Customer");

            var confirmation = JsonConvert.DeserializeObject<ConfirmBeneficiaryTransferViewModel>(
                TempData["BeneficiaryTransferData"]!.ToString()!
            );

            var dto = new ExecuteBeneficiaryTransferDto
            {
                OriginAccountNumber = confirmation.OriginAccountNumber,
                BeneficiaryAccountNumber = confirmation.BeneficiaryAccountNumber,
                BeneficiaryFullName = confirmation.BeneficiaryFullName,
                Amount = confirmation.Amount,
                Timestamp = confirmation.Timestamp
            };

            var success = await transactionService.ExecuteBeneficiaryTransferAsync(dto);
            if (!success)
            {
                ModelState.AddModelError("", "Ocurrió un error al procesar la transacción.");
                return View("ConfirmBeneficiaryTransfer", confirmation);
            }

            return RedirectToAction("Home", "Customer");
        }

        private async Task RefillFormAsync(BeneficiaryTransferFormViewModel model)
        {
            var userId = await GetAuthenticatedUserIdAsync();

            var beneficiaries = await beneficiaryService.GetByOwnerUserIdAsync(userId);

            model.Beneficiaries = beneficiaries
                .Select(b => new SelectListItem
                {
                    Value = b.BeneficiaryAccountNumber,
                    Text = $"{b.Name} {b.LastName}"
                }).ToList();

            model.OriginAccounts = (await savingsAccountService.GetActiveByUserIdAsync(userId))
                .Select(a => new AccountOptionViewModelBeneficiary
                {
                    Id = a.Id,
                    AccountNumber = a.AccountNumber
                }).ToList();
        }

        [HttpGet]
        public IActionResult Transaction()
        {
            return View();
        }


        //=======================================================================================================

        public async Task<IActionResult> CreditCardPaymentTransaction()
        {
            var user = await userManager.GetUserAsync(User);
            var userId = Guid.Parse(user.Id);

            var cards = await creditCardService.GetActiveCardsByUserIdAsync(userId);
            var accounts = await savingsAccountService.GetActiveAccountsByUserIdAsync(userId);

            ViewBag.CreditCards = new SelectList(cards, "CardNumber", "CardNumber");
            ViewBag.SavingsAccounts = new SelectList(accounts, "AccountNumber", "AccountNumber");

            return View(new CreditCardPaymentViewModel
            {
                CreditCardNumber = "",
                AccountNumber = "",
                Amount = 0
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreditCardPaymentTransaction(CreditCardPaymentViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var user = await userManager.GetUserAsync(User);
                var userId = Guid.Parse(user.Id);

                var cards = await creditCardService.GetActiveCardsByUserIdAsync(userId);
                var accounts = await savingsAccountService.GetActiveAccountsByUserIdAsync(userId);

                ViewBag.CreditCards = new SelectList(cards, "CardNumber", "CardNumber");
                ViewBag.SavingsAccounts = new SelectList(accounts, "AccountNumber", "AccountNumber");

                return View(vm);
            }

            var accountId = await savingsAccountService.GetAccountIdByAccountNumberAsync(vm.AccountNumber);
            if (accountId == null)
            {
                ModelState.AddModelError("", "La cuenta de origen no existe.");
                var user = await userManager.GetUserAsync(User);
                var userId = Guid.Parse(user.Id);

                var cards = await creditCardService.GetActiveCardsByUserIdAsync(userId);
                var accounts = await savingsAccountService.GetActiveAccountsByUserIdAsync(userId);

                ViewBag.CreditCards = new SelectList(cards, "CardNumber", "CardNumber");
                ViewBag.SavingsAccounts = new SelectList(accounts, "AccountNumber", "AccountNumber");

                return View(vm);
            }

            var originAccount = await savingsAccountService.GetAccountSummaryAsync(accountId.Value);

            if (originAccount.Balance < vm.Amount)
            {
                ModelState.AddModelError("", "Fondos insuficientes en la cuenta de origen.");
                var user = await userManager.GetUserAsync(User);
                var userId = Guid.Parse(user.Id);

                var cards = await creditCardService.GetActiveCardsByUserIdAsync(userId);
                var accounts = await savingsAccountService.GetActiveAccountsByUserIdAsync(userId);

                ViewBag.CreditCards = new SelectList(cards, "CardNumber", "CardNumber");
                ViewBag.SavingsAccounts = new SelectList(accounts, "AccountNumber", "AccountNumber");

                return View(vm);
            }

            var cardId = await creditCardService.GetCardIdByNumberAsync(vm.CreditCardNumber);
            if (cardId == null)
            {
                ModelState.AddModelError("", "La tarjeta de crédito no existe.");
                var user = await userManager.GetUserAsync(User);
                var userId = Guid.Parse(user.Id);

                var cards = await creditCardService.GetActiveCardsByUserIdAsync(userId);
                var accounts = await savingsAccountService.GetActiveAccountsByUserIdAsync(userId);

                ViewBag.CreditCards = new SelectList(cards, "CardNumber", "CardNumber");
                ViewBag.SavingsAccounts = new SelectList(accounts, "AccountNumber", "AccountNumber");

                return View(vm);
            }

            var creditCard = await creditCardService.GetCardDetailsAsync(cardId.Value);
            if (creditCard == null || creditCard.Status != CreditCardStatus.Active)
            {
                ModelState.AddModelError("", "La tarjeta de crédito está inactiva.");
                var user = await userManager.GetUserAsync(User);
                var userId = Guid.Parse(user.Id);

                var cards = await creditCardService.GetActiveCardsByUserIdAsync(userId);
                var accounts = await savingsAccountService.GetActiveAccountsByUserIdAsync(userId);

                ViewBag.CreditCards = new SelectList(cards, "CardNumber", "CardNumber");
                ViewBag.SavingsAccounts = new SelectList(accounts, "AccountNumber", "AccountNumber");

                return View(vm);
            }

            var dto = new CreditCardTransactionDto
            {
                TransactionOrigin = originAccount.Id,
                Amount = vm.Amount,
                CreditCardId = cardId.Value,
                Date = DateTime.UtcNow,
                Status = TransactionStatus.Pending,
                Type = CreditCardTransactionType.Payment
            };

            var result = await creditCardTransactionService.RegisterPaymentAsync(dto);

            if (result == null)
            {
                TempData["ErrorMessage"] = "No se pudo procesar el pago a la tarjeta.";
                var user = await userManager.GetUserAsync(User);
                var userId = Guid.Parse(user.Id);

                var cards = await creditCardService.GetActiveCardsByUserIdAsync(userId);
                var accounts = await savingsAccountService.GetActiveAccountsByUserIdAsync(userId);

                ViewBag.CreditCards = new SelectList(cards, "CardNumber", "CardNumber");
                ViewBag.SavingsAccounts = new SelectList(accounts, "AccountNumber", "AccountNumber");

                return View(vm);
            }

            TempData["SuccessMessage"] = "El pago a la tarjeta se ha realizado correctamente.";
            return RedirectToAction("Index", "CustomerHome");
        }


        public async Task<IActionResult> LoanPaymentTransaction()
        {
            var user = await userManager.GetUserAsync(User);
            var userId = Guid.Parse(user.Id);

            var loans = await loanService.GetActiveLoansByUserIdAsync(userId);
            var accounts = await savingsAccountService.GetActiveAccountsByUserIdAsync(userId);

            ViewBag.Loans = new SelectList(loans, "LoanNumber", "LoanNumber");
            ViewBag.SavingsAccounts = new SelectList(accounts, "AccountNumber", "AccountNumber");

            return View(new LoanPaymentViewModel
            {
                OriginAccountNumber = "",
                Amount = 0,
                LoanNumber = ""
            });
        }
        [HttpPost]
        public async Task<IActionResult> LoanPaymentTransaction(LoanPaymentViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var user = await userManager.GetUserAsync(User);
                var userId = Guid.Parse(user.Id);

                var loans = await loanService.GetActiveLoansByUserIdAsync(userId);
                var accounts = await savingsAccountService.GetActiveAccountsByUserIdAsync(userId);

                ViewBag.Loans = new SelectList(loans, "LoanNumber", "LoanNumber");
                ViewBag.SavingsAccounts = new SelectList(accounts, "AccountNumber", "AccountNumber");

                return View(vm);
            }

            var accountId = await savingsAccountService.GetAccountIdByAccountNumberAsync(vm.OriginAccountNumber);
            if (accountId == null)
            {
                ModelState.AddModelError("", "La cuenta de origen no existe.");
                return View(vm);
            }

            var originAccount = await savingsAccountService.GetAccountSummaryAsync(accountId.Value);
            if (originAccount == null)
            {
                ModelState.AddModelError("", "No se pudo obtener la cuenta de origen.");
                return View(vm);
            }

            if (originAccount.Balance < vm.Amount)
            {
                ModelState.AddModelError("", "Fondos insuficientes en la cuenta de origen.");
                return View(vm);
            }

            var loan = await loanService.GetLoanByNumberAsync(vm.LoanNumber);
            if (loan == null || loan.Status == LoanStatus.Completed)
            {
                ModelState.AddModelError("", "El préstamo no existe o está completado.");
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

            var dto = new LoanPaymentDto
            {
                UserId = loan.UserId,
                OriginAccountNumber = originAccount.AccountNumber,
                Amount = vm.Amount,
                LoanNumber = loan.LoanNumber! 
            };
            var result = await loanPaymentService.RegisterPaymentAsync(dto);

            if (result == null)
            {
                TempData["ErrorMessage"] = "No se pudo procesar el pago al préstamo.";
                return View(vm);
            }

            TempData["SuccessMessage"] = "El pago al préstamo se ha realizado correctamente.";
            return RedirectToAction("Index", "CustomerHome");
        }

    }
}
