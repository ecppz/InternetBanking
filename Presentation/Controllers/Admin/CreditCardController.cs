using Application.Dtos.CreditCard;
using Application.Dtos.Email;
using Application.Dtos.User;
using Application.Interfaces;
using Application.ViewModels.CreditCard;
using AutoMapper;
using Domain.Common.Enums;
using Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InternetBankingApp.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class CreditCardController : Controller
    {
        private readonly ICreditCardService creditCardService;
        private readonly IUserAccountServiceForWebApp userAccountService;
        private readonly UserManager<UserAccount> userManager;
        private readonly IEmailService emailService;
        private readonly IMapper mapper;

        public CreditCardController(ICreditCardService creditCardService, IUserAccountServiceForWebApp userAccountService, UserManager<UserAccount> userManager,
             IEmailService emailService, IMapper mapper)
        {
            this.creditCardService = creditCardService;
            this.userAccountService = userAccountService;
            this.userManager = userManager;
            this.emailService = emailService;
            this.mapper = mapper;
        }
        public async Task<IActionResult> Index(string? documentNumber, string? statusFilter)
        {
            var allUsers = await userAccountService.GetAllActiveUsers();
            var dtos = await creditCardService.GetAllDisplayAsync(allUsers, documentNumber, statusFilter);

            ViewBag.CurrentFilter = documentNumber;
            ViewBag.StatusFilter = statusFilter;

            var vms = mapper.Map<List<CreditCardDisplayViewModel>>(dtos);
            return View(vms);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var card = await creditCardService.GetCardDetailsAsync(id);
            if (card == null)
            {
                return NotFound();
            }

            var vm = mapper.Map<CreditCardDetailsViewModel>(card);
            return View(vm);
        }

        public async Task<IActionResult> EligibleCustomersForCreditCard(string? documentNumber)
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

            var eligibleCustomers = await creditCardService.GetEligibleCustomersForCreditCard(customers);

            if (!string.IsNullOrWhiteSpace(documentNumber))
            {
                eligibleCustomers = eligibleCustomers
                    .Where(c => c.DocumentNumber.Contains(documentNumber))
                    .ToList();

                ViewData["CurrentFilter"] = documentNumber;
            }

            var avgDebt = await creditCardService.GetAverageDebtAsync();
            ViewData["AverageDebt"] = avgDebt;

            var vm = mapper.Map<List<EligibleCustomerForCreditCardViewModel>>(eligibleCustomers);
            return View(vm);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(EditCreditCardViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var dto = new EditCreditCardDto
            {
                CardId = vm.CardId,
                NewLimit = vm.NewLimit
            };

            var user = await userAccountService.GetUserById(vm.UserId.ToString());
            var result = await creditCardService.UpdateCreditLimitAsync(dto, user);

            if (!result)
            {
                ViewBag.ErrorMessage = "El nuevo límite no puede ser inferior a la deuda actual.";
                return View(vm);
            }

            var card = await creditCardService.GetById(dto.CardId);

            if (user != null && card != null)
            {
                await emailService.SendAsync(new EmailRequestDto
                {
                    To = user.Email,
                    Subject = "Actualización de límite de tarjeta de crédito",
                    HtmlBody = $@"
                <p>Estimado {user.Name},</p>
                <p>El límite de su tarjeta de crédito terminada en <b>{card.CardNumber[^4..]}</b> ha sido actualizado.</p>
                <ul>
                    <li><b>Nuevo límite aprobado:</b> {card.CreditLimit:C}</li>
                    <li><b>Deuda actual:</b> {card.CurrentDebt:C}</li>
                    <li><b>Fecha de expiración:</b> {card.ExpirationDate:MM/yy}</li>
                </ul>
                <p>Gracias por confiar en nosotros.</p>"
                });
            }

            return RedirectToAction("Index");
        }



        public async Task<IActionResult> AssignCreditCard(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                TempData["Error"] = "Debes seleccionar un cliente antes de continuar";
                return RedirectToAction("EligibleCustomers");
            }

            var user = await userAccountService.GetUserById(userId.ToString());

            if (user == null || !user.IsActive)
            {
                TempData["Error"] = "El cliente seleccionado no es válido o está inactivo";
                return RedirectToAction("EligibleCustomers");
            }

            var adminUser = await userManager.GetUserAsync(User);

            var vm = new AssignCreditCardViewModel
            {
                UserId = userId,
                DocumentNumber = user.DocumentNumber,
                FullName = $"{user.Name} {user.LastName}",
                Email = user.Email,
                CreditLimit = 0,
                AdminUserId = Guid.Parse(adminUser.Id)
            };

            var avgDebt = await creditCardService.GetAverageDebtAsync();
            ViewData["AverageDebt"] = avgDebt;

            return View("AssignCreditCard", vm);
        }
        [HttpPost]
        public async Task<IActionResult> AssignCreditCard(AssignCreditCardViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var dto = mapper.Map<AssignCreditCardDto>(vm);
            var response = await creditCardService.AssignCardAsync(dto);

            if (!response.Success)
            {
                ViewBag.ErrorMessage = response.Message;
                return View(vm);
            }

            var user = await userAccountService.GetUserById(dto.UserId.ToString());
            if (user != null)
            {
                await emailService.SendAsync(new EmailRequestDto
                {
                    To = user.Email,
                    Subject = "Asignación de tarjeta de crédito",
                    HtmlBody = $@"
                <p>Estimado {user.Name},</p>
                <p>Se le ha asignado una nueva tarjeta de crédito.</p>
                <ul>
                    <li><b>Número de tarjeta:</b> terminada en {response.CardNumber}</li>
                    <li><b>Límite aprobado:</b> {response.CreditLimit:C}</li>
                    <li><b>Deuda actual:</b> {response.CurrentDebt:C}</li>
                    <li><b>Fecha de expiración:</b> {response.ExpirationDate:MM/yy}</li>
                </ul>
                <p>Gracias por confiar en nosotros.</p>"
                });
            }

            return RedirectToAction("Index");
        }


        public async Task<IActionResult> Edit(Guid id)
        {
            var card = await creditCardService.GetById(id);
            if (card == null)
            {
                return NotFound();
            }

            var vm = new EditCreditCardViewModel
            {
                CardId = card.Id,
                NewLimit = card.CreditLimit
            };
            return View(vm);
        }


        public async Task<IActionResult> CancelCreditCard(Guid id)
        {
            var card = await creditCardService.GetById(id);
            if (card == null)
            {
                return NotFound();
            }

            if (card.Status == CreditCardStatus.Cancelled || card.Status == CreditCardStatus.Expired)
            {
                return BadRequest("No se puede editar una tarjeta cancelada o expirada.");
            }

            var vm = mapper.Map<CancelCreditCardViewModel>(card);
            ViewBag.LastDigits = vm.CardLastDigits;
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> CancelCreditCard(CancelCreditCardViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var dto = mapper.Map<CancelCreditCardDto>(vm);
            var result = await creditCardService.CancelCardAsync(dto);

            if (!result)
            {
                TempData["ErrorMessage"] = "Para cancelar esta tarjeta, el cliente debe saldar la totalidad de la deuda pendiente.";
                return RedirectToAction("Index");
            }

            var card = await creditCardService.GetById(dto.CardId);
            var user = await userAccountService.GetUserById(dto.UserId.ToString());

            if (user != null && card != null)
            {
                await emailService.SendAsync(new EmailRequestDto
                {
                    To = user.Email,
                    Subject = "Cancelación de tarjeta de crédito",
                    HtmlBody = $@"
                <p>Estimado {user.Name},</p>
                <p>Su tarjeta de crédito terminada en <b>{card.CardNumber[^4..]}</b> ha sido cancelada.</p>
                <p>A partir de este momento no podrá realizar consumos ni pagos con dicha tarjeta.</p>
                <p>Gracias por confiar en nosotros.</p>"
                });
            }

            TempData["SuccessMessage"] = $"La tarjeta terminada en {vm.CardLastDigits} ha sido cancelada correctamente.";
            return RedirectToAction("Index");
        }


    }
}
