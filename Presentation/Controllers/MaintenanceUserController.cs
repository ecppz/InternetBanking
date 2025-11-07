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


     [Authorize(Roles = "Admin")] 
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
            var ids = await userAccountService.GetAllUserIdsAsync();

            var dtos = await userAccountService.GetUsersByIds(ids); // ✅ carga todos los usuarios en una sola llamada

            var vms = mapper.Map<List<UserViewModel>>(dtos);

            return View(vms);
        }
        public async Task<IActionResult> Create()
        {
            // Validación de sesión y rol (puedes activarla luego)
            // UserAccount? userSession = await userManager.GetUserAsync(User);
            // if (userSession != null)
            //     return RedirectToRoute(new { controller = "Home", action = "Index" });

            // var roles = await userManager.GetRolesAsync(userSession);
            // if (!roles.Contains("Admin"))
            //     return RedirectToRoute(new { controller = "AccessDenied", action = "Index" });

            // Inicialización segura del ViewModel
            var vm = new SaveUserViewModel
            {
                Id = "",
                Name = "",
                LastName = "",
                UserName = "",
                Email = "",
                DocumentNumber = "",
                Password = "",
                ConfirmPassword = "",
                Role = Roles.None.ToString(),
                CurrentBalance = 0
            };

            return View("Save", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SaveUserViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View("Save", vm);
            }

            if (!Enum.TryParse<Roles>(vm.Role, out var parsedRole) || !Enum.IsDefined(typeof(Roles), parsedRole) || parsedRole == Roles.None)
            {
                ModelState.AddModelError("Role", "Debes seleccionar un tipo de usuario válido.");
                return View("Save", vm);
            }

            if (parsedRole == Roles.Customer && (!vm.CurrentBalance.HasValue || vm.CurrentBalance < 0))
            {
                ModelState.AddModelError("CurrentBalance", "El monto inicial debe ser mayor o igual a cero.");
                return View("Save", vm);
            }

            string origin = $"{Request.Scheme}://{Request.Host}";

            vm.DocumentNumber = vm.DocumentNumber?.Replace("-", "").Trim();
            var dto = mapper.Map<SaveUserDto>(vm);
            dto.Role = parsedRole.ToString();

            UserResponseDto result = await userAccountService.RegisterUserAsync(dto, origin);

            if (result.HasError)
            {
                ViewBag.HasError = true;
                ViewBag.Errors = result.Errors;
                return View("Save", vm);
            }

            // Si el rol es cliente y quieres guardar el monto inicial, puedes hacerlo aquí
            // Ejemplo: await userAccountService.EditUserAsync(dto, origin, true);

            TempData["Success"] = $"Usuario creado correctamente. Se ha enviado un correo de confirmación a {result.Email}.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            string message = await userAccountService.ConfirmAccountAsync(userId, token);
            return View("ConfirmEmail", model: message); // explícito
        }


        // GET: Editar usuario
        public async Task<IActionResult> Edit(string id)
        {
            ViewBag.EditMode = true;

            var dto = await userAccountService.GetUserById(id);
            if (dto == null)
            {
                TempData["Error"] = "El usuario no existe.";
                return RedirectToAction("Index");
            }

            var vm = mapper.Map<UpdateUserViewModel>(dto);
            return View("Edit", vm);
        }

        // POST: Guardar cambios del usuario
        [HttpPost]
        public async Task<IActionResult> Edit(UpdateUserViewModel vm)
        {
            ViewBag.EditMode = true;

            if (!ModelState.IsValid)
            {
                return View("Edit", vm);
            }

            // Validación de duplicados
            var existingUser = await userAccountService.GetUserById(vm.Id);
            if (existingUser == null)
            {
                TempData["Error"] = "El usuario no existe.";
                return RedirectToAction("Index");
            }

            // Normalizar cédula
            vm.DocumentNumber = vm.DocumentNumber?.Replace("-", "").Trim();

            // Validar duplicados si cambió el email, username o cédula
            if (existingUser.Email != vm.Email)
            {
                var emailTaken = await userAccountService.GetUserByEmail(vm.Email);
                if (emailTaken != null)
                {
                    ModelState.AddModelError("Email", "Este correo ya está en uso.");
                    return View("Edit", vm);
                }
            }

            if (existingUser.UserName != vm.UserName)
            {
                var userNameTaken = await userAccountService.GetUserByUserName(vm.UserName);
                if (userNameTaken != null)
                {
                    ModelState.AddModelError("UserName", "Este nombre de usuario ya está en uso.");
                    return View("Edit", vm);
                }
            }

            if (existingUser.DocumentNumber != vm.DocumentNumber)
            {
                var cedulaRepetida = await userAccountService.GetUserById(vm.DocumentNumber);
                if (cedulaRepetida != null && cedulaRepetida.Id != vm.Id)
                {
                    ModelState.AddModelError("DocumentNumber", "Esta cédula ya está registrada.");
                    return View("Edit", vm);
                }
            }

            // Mapear y guardar
            var dto = mapper.Map<SaveUserDto>(vm);
            var origin = $"{Request.Scheme}://{Request.Host}";
            await userAccountService.EditUser(dto, origin: origin, isCreated: false);

            TempData["Success"] = "Usuario actualizado correctamente.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Activate(string id)
        {
            var success = await userAccountService.SetUserActiveStatus(id, true);
            if (!success)
            {
                TempData["Error"] = "No se pudo activar el usuario.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Deactivate(string id)
        {
            var success = await userAccountService.SetUserActiveStatus(id, false);
            if (!success)
            {
                TempData["Error"] = "No se pudo desactivar el usuario.";
            }
            return RedirectToAction("Index");
        }


    }

}
