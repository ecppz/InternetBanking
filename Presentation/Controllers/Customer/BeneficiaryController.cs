using Application.Dtos.Beneficiary;
using Application.Interfaces;
using Application.ViewModels.Beneficiary;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternetBankingApp.Controllers.Customer
{

    [Authorize(Roles = "Customer")]
    public class BeneficiaryController : Controller
    {
        private readonly IUserAccountServiceForWebApp _userAccountService;
        private readonly IBeneficiaryService _beneficiaryService;
        private readonly IMapper _mapper;

        public BeneficiaryController(IMapper mapper, IBeneficiaryService beneficiaryService, IUserAccountServiceForWebApp userAccountService)
        {
            _mapper = mapper;
            _beneficiaryService = beneficiaryService;
            _userAccountService = userAccountService;
        }

        private async Task<Guid?> GetCurrentUserIdAsync()
        {
            var user = await _userAccountService.GetUserByUserName(User.Identity?.Name ?? "");
            if (user is null || !user.IsActive)
                return null;

            return Guid.Parse(user.Id);
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {

            var user = await _userAccountService.GetUserByUserName(User.Identity?.Name ?? "");
            if (user is null || !user.IsActive)
                return Unauthorized();

            var userId = Guid.Parse(user.Id);

            var dtos = await _beneficiaryService.GetByOwnerUserIdAsync(userId);
            var viewModels = _mapper.Map<List<BeneficiaryViewModel>>(dtos);

            var model = new BeneficiaryListViewModel
            {
                Beneficiaries = viewModels,
                NewBeneficiary = new CreateBeneficiaryViewModel()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Add(BeneficiaryListViewModel model)
        {
            var userId = await GetCurrentUserIdAsync();
            if (userId is null)
                return Unauthorized();

            var subModel = model.NewBeneficiary;
            if (!TryValidateModel(subModel))
            {
                TempData["Error"] = "Debe ingresar un número de cuenta válido.";
                return RedirectToAction(nameof(Index));
            }

            var dto = new CreateBeneficiaryDto
            {
                BeneficiaryAccountNumber = subModel.BeneficiaryAccountNumber
            };

            var result = await _beneficiaryService.AddAsync(userId.Value, dto);

            if (!result.Success)
            {
                TempData["Error"] = result.ErrorMessage;
                return RedirectToAction(nameof(Index));
            }

            var user = await _userAccountService.GetUserById(result.Beneficiary.BeneficiaryUserId.ToString());
            if (user != null)
            {
                TempData["Success"] = $"Beneficiario {user.Name} {user.LastName} agregado correctamente.";
            }
            else
            {
                TempData["Success"] = "Beneficiario agregado correctamente.";
            }

            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = await GetCurrentUserIdAsync();
            if (userId is null)
                return Unauthorized();

            var success = await _beneficiaryService.DeleteAsync(id, userId.Value);

            if (!success)
            {
                TempData["Error"] = "No se pudo eliminar el beneficiario. Verifique que le pertenece.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = "Beneficiario eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }


    }
}
