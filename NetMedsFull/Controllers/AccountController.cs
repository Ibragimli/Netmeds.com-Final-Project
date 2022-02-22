﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NetMedsFull.Models;
using NetMedsFull.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static NetMedsFull.Services.EmailServices;

namespace NetMedsFull.Controllers
{
    public class AccountController : Controller
    {
        private readonly DataContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailService _emailService;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(DataContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEmailService emailService, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _roleManager = roleManager;
        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(MemberLoginViewModel user)
        {
            var UserExists = await _userManager.FindByNameAsync(user.Username);

            if (!UserExists.IsAdmin)
            {
                if (UserExists == null)
                {
                    ModelState.AddModelError("", "Username or Password is incorrect!");
                    return View();
                }
                var result = await _signInManager.PasswordSignInAsync(UserExists, user.Password, false, false);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", "Username or Password is incorrect!");
                    return View();
                }
                return RedirectToAction("index", "home");
            }

            ModelState.AddModelError("", "Username or Password is incorrect!");
            return View();
        }
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(MemberRegisterViewModel user)
        {

            var userExistEmail = await _userManager.FindByEmailAsync(user.Email);
            if (userExistEmail != null)
            {
                ModelState.AddModelError("Email", "Email is Already!");
            }
            var userExistUsername = await _userManager.FindByNameAsync(user.Username);
            if (userExistUsername != null)
            {
                ModelState.AddModelError("Username", "Username is Already!");
            }
            if (!ModelState.IsValid)
            {
                return View();
            }


            AppUser newUser = new AppUser
            {
                UserName = user.Username,
                Email = user.Email,
                PhoneNumber = user.Phone,
                IsAdmin = false,
                FullName = user.Fullname,
            };

            var result = await _userManager.CreateAsync(newUser, user.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View();
            }
            await _userManager.AddToRoleAsync(newUser, "Member");
            await _signInManager.PasswordSignInAsync(newUser, user.Password, false, false);

            return RedirectToAction("index", "home");
        }
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotViewModel forgotVM)
        {
            AppUser user = await _userManager.FindByEmailAsync(forgotVM.Email);
            if (user == null)
            {
                ModelState.AddModelError("email", "User not found!");
            }
            if (!ModelState.IsValid)
            {
                return View();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var url = Url.Action("resetpassword", "account", new { email = forgotVM.Email, token = token }, Request.Scheme);

            _emailService.Send(forgotVM.Email, "ChangePassword", url);
            return RedirectToAction("login", "account");
        }
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel resetPasswordVM)
        {
            AppUser user = await _userManager.FindByEmailAsync(resetPasswordVM.Email);
            if (user == null || !(await _userManager.VerifyUserTokenAsync(user, _userManager.Options.Tokens.PasswordResetTokenProvider, "resetpassword", resetPasswordVM.Token)))
            {
                return RedirectToAction("login", "account");
            }
            return View(resetPasswordVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ResetPasswordViewModel resetPasswordVM)
        {
            if (string.IsNullOrWhiteSpace(resetPasswordVM.Password) || resetPasswordVM.Password.Length > 25)
                ModelState.AddModelError("Password", "Password is required and must be less than 25 character");
            if (!ModelState.IsValid)
            {
                return View("ResetPassword", resetPasswordVM);
            }
            AppUser user = await _userManager.FindByEmailAsync(resetPasswordVM.Email);
            if (user == null)
            {
                return RedirectToAction("login", "account");
            }
            var result = await _userManager.ResetPasswordAsync(user, resetPasswordVM.Token, resetPasswordVM.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View("ResetPassword", resetPasswordVM);
            }
            return RedirectToAction("login", "account");
        }

        public IActionResult Profile()
        {
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("login", "account");
        }

        //public async Task<IActionResult> CreateRole()
        //{
        //    var role1 = await _roleManager.CreateAsync(new IdentityRole("SuperAdmin"));
        //    var role2 = await _roleManager.CreateAsync(new IdentityRole("Admin"));
        //    var role3 = await _roleManager.CreateAsync(new IdentityRole("Member"));

        //    AppUser SuperAdmin = new AppUser { FullName = "Super Admin", UserName = "SuperAdmin" };
        //    var admin = await _userManager.CreateAsync(SuperAdmin, "Admin123");
        //    var resultRole = await _userManager.AddToRoleAsync(SuperAdmin, "SuperAdmin");

        //    return Ok(resultRole);
        //}


    }
}
