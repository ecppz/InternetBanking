using Application.Dtos.User;
using Application.Interfaces;
using Application.ViewModels.User;
using AutoMapper;
using Domain.Common.Enums;
using Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InternetBankingApp.Controllers.Auth
{
    public class LoginController : Controller
    {
        private readonly IUserAccountServiceForWebApp userAccountServiceForWebApp;
        private readonly IMapper _mapper;
        private readonly UserManager<UserAccount> _userManager;

        public LoginController(IUserAccountServiceForWebApp userAccountServiceForWebApp, IMapper mapper, UserManager<UserAccount> userManager)
        {
            this.userAccountServiceForWebApp = userAccountServiceForWebApp;
            _mapper = mapper;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userSession = await userAccountServiceForWebApp.GetUserByUserName(User.Identity?.Name ?? "");

            if (userSession != null && userSession.IsActive)
            {
                var role = userSession.Role;

                return role switch
                {
                    "Admin" => RedirectToRoute(new { controller = "Admin", action = "Index" }),
                    "Cashier" => RedirectToRoute(new { controller = "Cashier", action = "Index" }),
                    "Customer" => RedirectToRoute(new { controller = "Customer", action = "Index" }),
                   _ => RedirectToRoute(new { controller = "Login", action = "AccessDenied" }),
                };
            }

            return View(new LoginViewModel
            {
                UserName = string.Empty,
                Password = string.Empty
            });
        }

        [HttpPost]
        public async Task<IActionResult> Index(LoginViewModel vm)
        {
            if (vm == null)
            {
                return View(new LoginViewModel(){ UserName = "", Password = "" });
            }
            
            var userSession = await userAccountServiceForWebApp.GetUserByUserName(User.Identity?.Name ?? "");

            if (userSession != null && userSession.IsActive)
            {
                if (userSession.Role == Roles.Admin.ToString())
                    return RedirectToRoute(new { controller = "AdminHome", action = "Index" });

                if (userSession.Role == Roles.Cashier.ToString())
                    return RedirectToRoute(new { controller = "Cashier", action = "Index" });

                if (userSession.Role == Roles.Customer.ToString())
                    return RedirectToRoute(new { controller = "Customer", action = "Index" });
            }

            if (!ModelState.IsValid)
            {
                vm.Password = "";
                return View(vm);
            }

            var loginDto = new LoginDto
            {
                UserName = vm.UserName,
                Password = vm.Password
            };

            var response = await userAccountServiceForWebApp.AuthenticateAsync(loginDto);

            if (response != null && !response.HasError)
            {
                var role = response.Roles.FirstOrDefault();

                if (role == Roles.Admin.ToString())
                    return RedirectToRoute(new { controller = "Admin", action = "Index" });

                if (role == Roles.Cashier.ToString())
                    return RedirectToRoute(new { controller = "Cashier", action = "Index" });

                if (role == Roles.Customer.ToString())
                    return RedirectToRoute(new { controller = "Customer", action = "Index" });

                ModelState.AddModelError("userValidation", "Rol no reconocido.");
            }
            else
            {
                foreach (var error in response?.Errors ?? [])
                {
                    ModelState.AddModelError("userValidation", error);
                }
            }

            vm.Password = "";
            return View(vm);
        }

        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            UserResponseDto response = await userAccountServiceForWebApp.ConfirmAccountAsync(userId, token);
            return View("ConfirmEmail", response.Message);
        }


        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordRequestViewModel() { UserName = "" });
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequestViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            string origin = Request?.Headers?.Origin.ToString() ?? string.Empty;

            ForgotPasswordRequestDto dto = new() { UserName = vm.UserName, Origin = origin };

            UserResponseDto? returnUser = await userAccountServiceForWebApp.ForgotPasswordAsync(dto);

            if (returnUser.HasError)
            {
                ViewBag.HasError = true;
                ViewBag.Errors = returnUser.Errors;
                return View(vm);
            }

            return RedirectToRoute(new { controller = "Login", action = "Index" });
        }

        public IActionResult ResetPassword(string userId, string token)
        {
            return View(new ResetPasswordRequestViewModel() { Id = userId, Token = token, Password = "" });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequestViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            ResetPasswordRequestDto dto = new() { Id = vm.Id, Password = vm.Password, Token = vm.Token };

            UserResponseDto? returnUser = await userAccountServiceForWebApp.ResetPasswordAsync(dto);

            if (returnUser.HasError)
            {
                ViewBag.HasError = true;
                ViewBag.Errors = returnUser.Errors;
                return View(vm);
            }

            return RedirectToRoute(new { controller = "Login", action = "Index" });
        }


        public async Task<IActionResult> Logout()
        {
            await userAccountServiceForWebApp.SignOutAsync();
            return RedirectToRoute(new { controller = "Login", action = "Index" });
        }

        public async Task<IActionResult> AccessDenied()
        {
            UserAccount? userSession = await _userManager.GetUserAsync(User);

            if (userSession != null)
            {
                var user = await userAccountServiceForWebApp.GetUserByUserName(userSession.UserName ?? "");
                ViewBag.CurrentRol = user?.Role ?? "";
                return View();
            }

            return RedirectToRoute(new { controller = "Login", action = "Index" });
        }


    }
}
