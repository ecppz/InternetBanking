using Application.Dtos.Transaction;
using Application.Interfaces;
using Application.Services;
using Application.ViewModels.ExpressTransaction;
using Application.ViewModels.TransactionBeneficiaryTransfer;
using AutoMapper;
using Domain.Common.Enums;
using Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;




namespace Presentation.Controllers
{
    public class TransactionCustomerController : Controller
    {
        private readonly IMapper mapper;
        private readonly ITransactionService transactionService;
        private readonly ISavingsAccountService savingsAccountService;
        private readonly IUserAccountService _userAccountService;
        private readonly IBeneficiaryService _beneficiaryService;


        public TransactionCustomerController(
               IMapper mapper,
               ITransactionService transactionService,
               ISavingsAccountService savingsAccountService,
               IUserAccountService userAccountService, IBeneficiaryService beneficiaryService)
        {
            this.mapper = mapper;
            this.transactionService = transactionService;
            this.savingsAccountService = savingsAccountService;
            _userAccountService = userAccountService;
            _beneficiaryService = beneficiaryService;

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

            var user = await _userAccountService.GetUserByUserName(userName);

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

            var beneficiaries = (await _beneficiaryService.GetByOwnerUserIdAsync(userId))
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
            var beneficiary = await _beneficiaryService.GetByAccountNumberAndOwnerAsync(userId, model.BeneficiaryAccountNumber);
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

            var beneficiaries = await _beneficiaryService.GetByOwnerUserIdAsync(userId);

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

    }
}