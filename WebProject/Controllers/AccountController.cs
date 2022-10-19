#nullable disable
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using WebProject.Models;
using WebProject.Data;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;

namespace WebProject.Controllers
{
	[Authorize]
	public class AccountController : Controller
	{
		private readonly UserManager<UserModel> userManager;
		private readonly SignInManager<UserModel> signInManager;

		public AccountController(UserManager<UserModel> manager, SignInManager<UserModel> sign)
		{
			userManager = manager;
			signInManager = sign;
		}

		[AllowAnonymous]
		public async Task<IActionResult> Login(string returnUrl)
		{
			if (User.Identity.IsAuthenticated)
			{
				UserModel userModel = await userManager.GetUserAsync(HttpContext.User);
				return RedirectToActionPermanent("UserPage", "User", new { userModel.UserName });
			}

			LoginModel login = new LoginModel();
			login.ReturnUrl = returnUrl;
			return View(login);
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(LoginModel login)
		{
			if (ModelState.IsValid)
			{
				UserModel user = await userManager.FindByNameAsync(login.UserName);

				if (user != null)
				{
					await signInManager.SignOutAsync();
					Microsoft.AspNetCore.Identity.SignInResult result = await signInManager.PasswordSignInAsync(user, login.Password, false, false);

					if (result.Succeeded)
					{
						return Redirect(login.ReturnUrl ?? $"/{user.UserName}");
					}
				}

				ModelState.AddModelError(nameof(login.UserName), "Login Failed: Invalid User Name or password");
			}
			return View(login);
		}

		[AllowAnonymous]
		public async Task<IActionResult> Register()
		{
			if (User.Identity.IsAuthenticated)
			{
				UserModel userModel = await userManager.GetUserAsync(HttpContext.User);
				return RedirectToActionPermanent("UserPage", "User", new { userModel.UserName });
			}

			return View();
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Register(RegisterModel create)
		{
			if (ModelState.IsValid)
			{
				UserModel user = new UserModel()
				{
					Email = create.Email,
					UserName = create.UserName,
					DateofBirth = create.DateofBirth,
				};

				IdentityResult result = await userManager.CreateAsync(user, create.Password);

				if (result.Succeeded)
				{
					return RedirectToAction("Login", "Account");
				}
				else
				{
					foreach (var error in result.Errors)
					{
						ModelState.AddModelError("", error.Description);
					}
				}
			}

			return View();
		}

		//Passwords must be at least 6 characters.
		//Passwords must have at least one non alphanumeric character.
		//Passwords must have at least one digit('0'-'9').
		//Passwords must have at least one uppercase('A'-'Z').

		public async Task<IActionResult> Logout()
		{
			await signInManager.SignOutAsync();
			return RedirectToAction("Login", "Account");
		}
	}
}
