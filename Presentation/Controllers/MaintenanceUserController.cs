using Application.Dtos.User;
using Application.Interfaces;
using Application.ViewModels.User;
using AutoMapper;
using Domain.Common.Enums;
using Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{

    // le toy METIENDO MANO A ESTO!! 


 //   [Authorize(Roles = "Admin")] 
    public class MaintenanceUserController : Controller
    {
        private readonly IUserAccountService userAccountService;
        private readonly UserManager<UserAccount> userManager;
        private readonly IMapper mapper;

        public MaintenanceUserController(IUserAccountService userAccountService, UserManager<UserAccount> userManager, IMapper mapper)
        {
            this.userAccountService = userAccountService;
            this.userManager = userManager;
            this.mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var dtos = await userAccountService.GetAllUserIdsAsync();
            var vms = mapper.Map<List<UserViewModel>>(dtos);
            return View(vms);
        }

        public async Task<IActionResult> Create()
        {
            //UserAccount? userSession = await userManager.GetUserAsync(User);

            //if (userSession != null)
            //{
            //    return RedirectToRoute(new { controller = "Home", action = "Index" });
            //}

            //var roles = await userManager.GetRolesAsync(userSession);

            //if (!roles.Contains("Admin"))
            //{
            //    return RedirectToRoute(new { controller = "AccessDenied", action = "Index" });
            //}

            //pa pode proba !! no me quite na de eso

            return View("Save", new SaveUserViewModel
            {
                Id = "",
                Name = "",
                LastName = "",
                UserName = "",
                Email = "",
                DocumentNumber = "",
                Password = "",
                ConfirmPassword = "",
                Role = Roles.None,
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(SaveUserViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View("Save", vm);
            }

            var user = new UserAccount
            {
                Name = vm.Name,
                LastName = vm.LastName,
                UserName = vm.UserName,
                Email = vm.Email,
                DocumentNumber = vm.DocumentNumber,
                EmailConfirmed = false,
            };

            var result = await userManager.CreateAsync(user, vm.Password);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Error al crear el usuario");
                return View("Save", vm);
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            string response = await userAccountService.ConfirmAccountAsync(userId, token);
            return View("ConfirmEmail", response);
        }


        public async Task<IActionResult> Edit(string id)
        {
            ViewBag.EditMode = true;
            var dto = await userAccountService.GetUserById(id);
            if (dto == null)
            {
                return RedirectToAction("Index");
            }
          
            var vm = mapper.Map<SaveUserViewModel>(dto);
            return View("Save", vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SaveUserViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.EditMode = true;
                return View("Save", vm);
            }

            var dto = mapper.Map<SaveUserDto>(vm);
            await userAccountService.GetUserById(dto.Id);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Activate(string id)
        {
            var dto = await userAccountService.GetUserById(id);
            if (dto == null) return RedirectToAction("Index");

            var vm = mapper.Map<ActivateUserViewModel>(dto);
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Activate(ActivateUserViewModel vm)
        {
            await userAccountService.ActivateUser(vm.Id);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Deactivate(string id)
        {
            var dto = await userAccountService.GetUserById(id);
            if (dto == null) return RedirectToAction("Index");

            var vm = mapper.Map<DeactivateUserViewModel>(dto);
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Deactivate(DeactivateUserViewModel vm)
        {
            await userAccountService.DeactivateUser(vm.Id);
            return RedirectToAction("Index");
        }
    }

}
