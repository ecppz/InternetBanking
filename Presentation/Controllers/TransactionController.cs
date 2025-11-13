using Application.Dtos.Transaction;
using Application.Interfaces;
using Application.Services;
using Application.ViewModels.Transaction;
using AutoMapper;
using Domain.Common.Enums;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Presentation.Controllers
{
    public class TransactionController : Controller
    {
        private readonly IMapper mapper;
        private readonly ITransactionService transactionService;
        private readonly ISavingsAccountService savingsAccountService;

        public TransactionController(IMapper mapper, ITransactionService transactionService, ISavingsAccountService savingsAccountService)
        {
            this.mapper = mapper;
            this.transactionService = transactionService;
            this.savingsAccountService = savingsAccountService;
        }


        // GET: Formulario de transferencia
        [HttpGet]
        public IActionResult TransferToThirdParty()
        {
            return View(new ThirdPartyTransferViewModel());
        }

        // POST: Validación inicial y redirección a confirmación
        [HttpPost]
        public async Task<IActionResult> TransferToThirdParty(ThirdPartyTransferViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (!await transactionService.IsOriginAccountValidAsync(model.OriginAccountNumber))
            {
                model.ErrorMessage = "La cuenta origen no es válida o está inactiva.";
                return View(model);
            }

            if (!await transactionService.HasSufficientFundsAsync(model.OriginAccountNumber, model.Amount))
            {
                model.ErrorMessage = "Fondos insuficientes en la cuenta origen.";
                return View(model);
            }

            if (!await transactionService.IsDestinationAccountValidAsync(model.DestinationAccountNumber))
            {
                model.ErrorMessage = "La cuenta destino no es válida o está inactiva.";
                return View(model);
            }

            var confirmModel = await transactionService.PrepareTransferConfirmationAsync(
                model.OriginAccountNumber,
                model.DestinationAccountNumber,
                model.Amount
            );

            if (confirmModel == null)
            {
                model.ErrorMessage = "No se pudo preparar la confirmación.";
                return View(model);
            }

            TempData["TransferData"] = JsonConvert.SerializeObject(model);
            return View("ConfirmTransfer", confirmModel);
        }

        // POST: Confirmación de transferencia
        [HttpPost]
        public async Task<IActionResult> ConfirmTransfer()
        {
            if (!TempData.ContainsKey("TransferData"))
                return RedirectToAction("TransferToThirdParty");

            var model = JsonConvert.DeserializeObject<ThirdPartyTransferViewModel>((string)TempData["TransferData"]!);

            var success = await transactionService.ExecuteThirdPartyTransferAsync(
                model.OriginAccountNumber,
                model.DestinationAccountNumber,
                model.Amount
            );

            if (!success)
            {
                TempData["ErrorMessage"] = "La transacción fue rechazada por el sistema.";
                return RedirectToAction("TransferToThirdParty");
            }

            TempData["SuccessMessage"] = "La transacción fue realizada exitosamente.";
            return RedirectToAction("TransferToThirdParty");
        }


        // POST: Cancelación de la operación
        [HttpPost]
        public IActionResult CancelTransfer()
        {
            return RedirectToAction("TransferToThirdParty");
        }

        //Deposito y retiro:

        //Metodos para deposito:

        // GET: /Transaction/Deposit
        public IActionResult Deposit()
        {
            return View(new DepositFormViewModel());
        }

        // POST: /Transaction/Deposit
        [HttpPost]
        public async Task<IActionResult> Deposit(DepositFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (model.Amount <= 0)
            {
                ModelState.AddModelError(string.Empty, "El monto de la transacción debe ser mayor a cero.");
                return View(model);
            }

            var exists = await savingsAccountService.ExistsAccountNumberAsync(model.DestinationAccountNumber);
            if (!exists)
            {
                ModelState.AddModelError(string.Empty, "El número de cuenta ingresado no es válido.");
                return View(model);
            }

            var accountId = await savingsAccountService.GetAccountIdByAccountNumberAsync(model.DestinationAccountNumber);
            if (accountId == null)
            {
                ModelState.AddModelError(string.Empty, "No se pudo obtener la cuenta destino.");
                return View(model);
            }

            var summary = await savingsAccountService.GetAccountSummaryAsync(accountId.Value);
            if (summary == null || summary.Estado == SavingsAccountStatus.Cancelada || summary.Estado == SavingsAccountStatus.Bloqueada)
            {
                ModelState.AddModelError(string.Empty, "La cuenta de ahorro está inactiva y no permite transacciones.");
                return View(model);
            }

            var dto = new DepositRequestDto
            {
                DestinationAccountNumber = model.DestinationAccountNumber,
                Amount = model.Amount
            };

            var confirmation = await transactionService.ValidateDepositAsync(dto);
            if (confirmation == null)
            {
                ModelState.AddModelError(string.Empty, "No se pudo validar la cuenta destino.");
                return View(model);
            }

            var viewModel = new DepositConfirmationViewModel
            {
                DestinationAccountNumber = confirmation.DestinationAccountNumber,
                DestinationOwnerFullName = confirmation.DestinationOwnerFullName,
                Amount = confirmation.Amount
            };

            return View("ConfirmDeposit", viewModel);
        }


        // POST: /Transaction/ConfirmDeposit
        [HttpPost]
        public async Task<IActionResult> ConfirmDeposit(DepositConfirmationViewModel model)
        {
            var dto = new DepositRequestDto
            {
                DestinationAccountNumber = model.DestinationAccountNumber,
                Amount = model.Amount
            };

            var success = await transactionService.ExecuteDepositAsync(dto);
            if (!success)
            {
                TempData["Error"] = "No se pudo completar el depósito.";
                return RedirectToAction("Deposit");
            }

            TempData["Success"] = "Depósito realizado exitosamente.";
            return RedirectToAction("Deposit");
        }

        //Para deposito:

        // GET: /Transaction/Withdrawal
        public IActionResult Withdrawal()
        {
            return View(new WithdrawalFormViewModel());
        }

        // POST: /Transaction/Withdrawal
        [HttpPost]
        public async Task<IActionResult> Withdrawal(WithdrawalFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (model.Amount <= 0)
            {
                ModelState.AddModelError(string.Empty, "El monto de la transacción debe ser mayor a cero.");
                return View(model);
            }

            var exists = await savingsAccountService.ExistsAccountNumberAsync(model.OriginAccountNumber);
            if (!exists)
            {
                ModelState.AddModelError(string.Empty, "El número de cuenta ingresado no es válido.");
                return View(model);
            }

            var accountId = await savingsAccountService.GetAccountIdByAccountNumberAsync(model.OriginAccountNumber);
            if (accountId == null)
            {
                ModelState.AddModelError(string.Empty, "No se pudo obtener la cuenta origen.");
                return View(model);
            }

            var summary = await savingsAccountService.GetAccountSummaryAsync(accountId.Value);
            if (summary == null)
            {
                ModelState.AddModelError(string.Empty, "No se pudo obtener la cuenta origen.");
                return View(model);
            }

            if (summary.Estado == SavingsAccountStatus.Cancelada || summary.Estado == SavingsAccountStatus.Bloqueada)
            {
                ModelState.AddModelError(string.Empty, "La cuenta de ahorro está inactiva y no permite transacciones.");
                return View(model);
            }

            if (summary.Balance < model.Amount)
            {
                ModelState.AddModelError(string.Empty, "El monto excede el saldo disponible en la cuenta.");
                return View(model);
            }

            var dto = new WithdrawalRequestDto
            {
                OriginAccountNumber = model.OriginAccountNumber,
                Amount = model.Amount
            };

            var confirmation = await transactionService.ValidateWithdrawalAsync(dto);
            if (confirmation == null)
            {
                ModelState.AddModelError(string.Empty, "No se pudo validar la cuenta origen.");
                return View(model);
            }

            var viewModel = new WithdrawalConfirmationViewModel
            {
                OriginAccountNumber = confirmation.OriginAccountNumber,
                OriginOwnerFullName = confirmation.OriginOwnerFullName,
                Amount = confirmation.Amount
            };

            return View("ConfirmWithdrawal", viewModel);
        }

        // POST: /Transaction/ConfirmWithdrawal
        [HttpPost]
        public async Task<IActionResult> ConfirmWithdrawal(WithdrawalConfirmationViewModel model)
        {
            var dto = new WithdrawalRequestDto
            {
                OriginAccountNumber = model.OriginAccountNumber,
                Amount = model.Amount
            };

            var success = await transactionService.ExecuteWithdrawalAsync(dto);
            if (!success)
            {
                TempData["Error"] = "No se pudo completar el retiro.";
                return RedirectToAction("Withdrawal");
            }

            TempData["Success"] = "Retiro realizado exitosamente.";
            return RedirectToAction("Withdrawal");
        }

    }
}
