using Application.Dtos.Transfer;
using Application.Interfaces;
using Application.Services;
using Application.ViewModels.InternalTransfer;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    public class InternalTransferController : Controller
    {
        private readonly IInternalTransferService _service;
        private readonly IUserAccountService _userAccountService;
        private readonly ISavingsAccountService _savingsAccountService;

        public InternalTransferController(IInternalTransferService service, IUserAccountService userAccountService, ISavingsAccountService savingsAccountService)
        {
            _service = service;
            _userAccountService = userAccountService;
            _savingsAccountService = savingsAccountService;
        }

        // GET: /InternalTransfer
        public async Task<IActionResult> Index()
        {
            var userId = await GetUserIdAsync();
            var accounts = await _savingsAccountService.GetAllByUserIdOrderedAsync(userId);

            var viewModel = new InternalTransferViewModel
            {
                UserAccounts = accounts
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(InternalTransferViewModel model)
        {
            var userId = await GetUserIdAsync();

            if (!ModelState.IsValid)
            {
                model.UserAccounts = await _savingsAccountService.GetAllByUserIdOrderedAsync(userId);
                model.TransferResult = new InternalTransferResultDto
                {
                    Success = false,
                    Message = "Datos inválidos. Verifica los campos e intenta nuevamente."
                };
                return View(model);
            }

            var result = await _service.TransferAsync(userId, model.TransferRequest);

            model.UserAccounts = await _savingsAccountService.GetAllByUserIdOrderedAsync(userId);
            model.TransferResult = result;

            return View(model);
        }

        // Este método debe obtener el ID del usuario autenticado
        private async Task<Guid> GetUserIdAsync()
        {
            if (User?.Identity?.IsAuthenticated != true)
                return Guid.Empty; // Usuario no autenticado

            var userName = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(userName))
                return Guid.Empty; // Nombre no disponible

            var user = await _userAccountService.GetUserByUserName(userName);
            if (user == null || string.IsNullOrWhiteSpace(user.Id))
                return Guid.Empty; // No se pudo obtener el ID

            if (!Guid.TryParse(user.Id, out var userId))
                return Guid.Empty; // ID inválido

            return userId;
        }



    }
}
