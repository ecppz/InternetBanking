using Application.Dtos.Email;
using Application.Dtos.Transaction;
using Application.Interfaces;
using Application.ViewModels.Transaction;
using AutoMapper;
using Domain.Common.Enums;
using Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Globalization;

namespace InternetBankingApp.Controllers.Cashier
{
    [Authorize(Roles = "Cashier")]
    public class TransactionController : Controller
    {
        private readonly ITransactionService transactionService;
        private readonly ISavingsAccountService savingsAccountService;
        private readonly IUserAccountServiceForWebApp userAccountService;
        private readonly UserManager<UserAccount> userManager;
        private readonly IEmailService emailService;
        private readonly IMapper mapper;

        public TransactionController(IMapper mapper, UserManager<UserAccount> userManager, ITransactionService transactionService, ISavingsAccountService savingsAccountService, 
            IUserAccountServiceForWebApp userAccountService, IEmailService emailService)
        {
            this.mapper = mapper;
            this.userManager = userManager;
            this.transactionService = transactionService;
            this.savingsAccountService = savingsAccountService;
            this.userAccountService = userAccountService;
            this.emailService = emailService;
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
            UserAccount? userSession = await userManager.GetUserAsync(User);

            if (userSession == null)
                return RedirectToRoute(new { controller = "AccessDenied", action = "Index" });

            var roles = await userManager.GetRolesAsync(userSession);
            if (!roles.Contains(Roles.Cashier.ToString()))
                return RedirectToRoute(new { controller = "AccessDenied", action = "Index" });

            if (!TempData.ContainsKey("TransferData"))
                return RedirectToAction("TransferToThirdParty");

            var model = JsonConvert.DeserializeObject<ThirdPartyTransferViewModel>((string)TempData["TransferData"]!);
            var cashierId = Guid.Parse(userSession.Id);

            var result = await transactionService.ExecuteThirdPartyTransferAsync(
                model.OriginAccountNumber,
                model.DestinationAccountNumber,
                model.Amount,
                cashierId
            );

            if (result == null)
            {
                TempData["ErrorMessage"] = "La transacción fue rechazada por el sistema.";
                return RedirectToAction("TransferToThirdParty");
            }

            // 👉 Correos
            var originAccount = await savingsAccountService.GetByAccountNumberAsync(result.Origin);
            var destinationAccount = await savingsAccountService.GetByAccountNumberAsync(result.Beneficiary);

            var originUser = await userAccountService.GetUserById(originAccount.UserId.ToString());
            var destinationUser = await userAccountService.GetUserById(destinationAccount.UserId.ToString());

            if (originUser != null && destinationUser != null)
            {
                var formattedAmount = result.Amount.ToString("C", new CultureInfo("es-DO"));
                var formattedDate = result.Date.ToString("dd/MM/yyyy h:mm tt", new CultureInfo("es-DO"));

                var last4Dest = destinationAccount.AccountNumber[^4..];
                var last4Origin = originAccount.AccountNumber[^4..];

                await emailService.SendAsync(new EmailRequestDto
                {
                    To = originUser.Email,
                    Subject = $"Transacción realizada a la cuenta {last4Dest}",
                    HtmlBody = $@"<p>Se ha enviado {formattedAmount} a la cuenta destino {last4Dest}.</p>
                          <p>Fecha y hora: {formattedDate}</p>"
                });

                await emailService.SendAsync(new EmailRequestDto
                {
                    To = destinationUser.Email,
                    Subject = $"Transacción enviada desde la cuenta {last4Origin}",
                    HtmlBody = $@"<p>Ha recibido un depósito de {formattedAmount} desde la cuenta {last4Origin}.</p>
                          <p>Fecha y hora: {formattedDate}</p>"
                });
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

            // 👉 aquí resolvemos el usuario
            var user = await userAccountService.GetUserById(confirmation.DestinationUserId.ToString());

            var viewModel = new DepositConfirmationViewModel
            {
                DestinationAccountNumber = confirmation.DestinationAccountNumber,
                DestinationOwnerFullName = user != null ? $"{user.Name?.Trim()} {user.LastName?.Trim()}".Trim() : "Desconocido",
                Amount = confirmation.Amount
            };

            return View("ConfirmDeposit", viewModel);
        }



        // POST: /Transaction/ConfirmDeposit
        [HttpPost]
        public async Task<IActionResult> ConfirmDeposit(DepositConfirmationViewModel model)
        {
            UserAccount? userSession = await userManager.GetUserAsync(User);

            if (userSession == null)
                return RedirectToRoute(new { controller = "AccessDenied", action = "Index" });

            var roles = await userManager.GetRolesAsync(userSession);
            if (!roles.Contains(Roles.Cashier.ToString()))
                return RedirectToRoute(new { controller = "AccessDenied", action = "Index" });

            var dto = new DepositRequestDto
            {
                DestinationAccountNumber = model.DestinationAccountNumber,
                Amount = model.Amount
            };

            var cashierId = Guid.Parse(userSession.Id);
            var result = await transactionService.ExecuteDepositAsync(dto, cashierId);

            if (result == null)
            {
                TempData["Error"] = "No se pudo completar el depósito.";
                return RedirectToAction("Deposit");
            }

            // 👉 Aquí resolvemos usuario y enviamos correo
            var destinationAccount = await savingsAccountService.GetByAccountNumberAsync(dto.DestinationAccountNumber);
            var user = await userAccountService.GetUserById(destinationAccount.UserId.ToString());

            if (user != null)
            {
                var formattedAmount = dto.Amount.ToString("C", new CultureInfo("es-DO"));
                var formattedDate = result.Date.ToString("dd/MM/yyyy h:mm tt", new CultureInfo("es-DO"));
                var last4 = destinationAccount.AccountNumber[^4..];

                await emailService.SendAsync(new EmailRequestDto
                {
                    To = user.Email,
                    Subject = $"Depósito realizado a su cuenta {last4}",
                    HtmlBody = $@"
                    <div style='font-family:Arial,sans-serif;color:#333'>
                        <h2 style='color:#28B463'>Depósito Recibido</h2>
                        <p>Se ha depositado <strong>{formattedAmount}</strong> en su cuenta <strong>{last4}</strong>.</p>
                        <p>Fecha y hora: <strong>{formattedDate}</strong></p>
                        <p style='margin-top:20px'>Gracias por confiar en nosotros.</p>
                    </div>"
                });
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

            // 👉 aquí resolvemos el usuario
            var user = await userAccountService.GetUserById(confirmation.OriginUserId.ToString());

            var viewModel = new WithdrawalConfirmationViewModel
            {
                OriginAccountNumber = confirmation.OriginAccountNumber,
                OriginOwnerFullName = user != null ? $"{user.Name?.Trim()} {user.LastName?.Trim()}".Trim() : "Desconocido",
                Amount = confirmation.Amount
            };

            return View("ConfirmWithdrawal", viewModel);
        }


        // POST: /Transaction/ConfirmWithdrawal
        [HttpPost]
        public async Task<IActionResult> ConfirmWithdrawal(WithdrawalConfirmationViewModel model)
        {
            UserAccount? userSession = await userManager.GetUserAsync(User);

            if (userSession == null)
                return RedirectToRoute(new { controller = "AccessDenied", action = "Index" });

            var roles = await userManager.GetRolesAsync(userSession);
            if (!roles.Contains(Roles.Cashier.ToString()))
                return RedirectToRoute(new { controller = "AccessDenied", action = "Index" });

            var dto = new WithdrawalRequestDto
            {
                OriginAccountNumber = model.OriginAccountNumber,
                Amount = model.Amount
            };

            var cashierId = Guid.Parse(userSession.Id);
            var result = await transactionService.ExecuteWithdrawalAsync(dto, cashierId);

            if (result == null)
            {
                TempData["Error"] = "No se pudo completar el retiro.";
                return RedirectToAction("Withdrawal");
            }

            // 👉 Aquí resolvemos usuario y enviamos correo
            var originAccount = await savingsAccountService.GetByAccountNumberAsync(dto.OriginAccountNumber);
            var user = await userAccountService.GetUserById(originAccount.UserId.ToString());

            if (user != null)
            {
                var formattedAmount = dto.Amount.ToString("C", new CultureInfo("es-DO"));
                var formattedDate = result.Date.ToString("dd/MM/yyyy h:mm tt", new CultureInfo("es-DO"));
                var last4 = originAccount.AccountNumber[^4..];

                await emailService.SendAsync(new EmailRequestDto
                {
                    To = user.Email,
                    Subject = $"Retiro realizado a su cuenta {last4}",
                    HtmlBody = $@"
                <div style='font-family:Arial,sans-serif;color:#333'>
                    <h2 style='color:#C0392B'>Retiro Procesado</h2>
                    <p>Se ha retirado <strong>{formattedAmount}</strong> de su cuenta <strong>{last4}</strong>.</p>
                    <p>Fecha y hora: <strong>{formattedDate}</strong></p>
                    <p style='margin-top:20px'>Gracias por confiar en nosotros.</p>
                </div>"
                });
            }

            TempData["Success"] = "Retiro realizado exitosamente.";
            return RedirectToAction("Withdrawal");
        }


    }
}
