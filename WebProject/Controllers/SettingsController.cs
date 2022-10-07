#nullable disable
using Microsoft.AspNetCore.Mvc;
using WebProject.Models;
using WebProject.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace WebProject.Controllers
{
	[Authorize]
	public class SettingsController : Controller
	{
		private readonly WebProjectContext _Models;
		private readonly UserManager<UserModel> userManager;
		private readonly IPasswordHasher<UserModel> passwordHasher;

		public SettingsController(WebProjectContext Models, IPasswordHasher<UserModel> passwordHshr, UserManager<UserModel> manager)
		{
			_Models = Models;
			userManager = manager;
			passwordHasher = passwordHshr;
		}

		public async Task<IActionResult> EditProfile()
		{
			UserModel userModel = await userManager.GetUserAsync(HttpContext.User);

			if (userModel == null)
			{
				return RedirectToAction("Logout", "Account");
			}

			return View(userModel);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditProfile(string Description, string Name, string UserName, IFormFile ProfilePicture)
		{
			UserModel userModel = await userManager.GetUserAsync(HttpContext.User);

			if (userModel != null)
			{
				if (!string.IsNullOrEmpty(UserName))
				{
					userModel.UserName = UserName;
				}

				if (!string.IsNullOrEmpty(Name))
				{
					userModel.Name = Name;
				}

				if (!string.IsNullOrEmpty(UserName))
				{
					userModel.Description = Description;
				}

				if (ProfilePicture != null)
				{
					userModel.ProfilePicture = Convert.ToBase64String(await GetBytes(ProfilePicture));

				}

				IdentityResult result = await userManager.UpdateAsync(userModel);
				if (result.Succeeded)
				{
					ViewBag.Message = "Profile successfully updated.";
					return View(userModel);
				}
				else
				{
					ViewBag.ErrorMessage = "Sorry, something went wrong.";
					return View(userModel);
				}
			}
			else
			{
				return RedirectToAction("Logout", "Account");
			}
		}

		public async Task<IActionResult> Appearance()
		{
			UserModel userModel = await userManager.GetUserAsync(HttpContext.User);

			if (userModel == null)
			{
				return RedirectToAction("Logout", "Account");
			}

			return View(userModel);
		}

		[HttpPost]
		public async Task ShowImagesToggle()
		{
			UserModel userModel = await userManager.GetUserAsync(HttpContext.User);

			if (userModel == null)
			{
				return;
			}

			if (userModel.ShowImages)
			{
				userModel.ShowImages = false;
			}
			else if (!userModel.ShowImages)
			{
				userModel.ShowImages = true;
			}

			await userManager.UpdateAsync(userModel);
		}

		public async Task<IActionResult> Security()
		{
			PasswordUser passwordUser = new PasswordUser();
			passwordUser.User = await userManager.GetUserAsync(HttpContext.User);

			if (passwordUser.User == null)
			{
				return RedirectToAction("Logout", "Account");
			}

			if (TempData["ErrorMessage"] != null)
			{
				ViewBag.ErrorMessage = TempData["ErrorMessage"].ToString();
			}

			if (TempData["Message"] != null)
			{
				ViewBag.Message = TempData["Message"].ToString();
			}

			return View(passwordUser);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Security(PasswordUser passwordUser)
		{
			passwordUser.User = await userManager.GetUserAsync(HttpContext.User);

			if (passwordUser.User == null)
			{
				return RedirectToAction("Logout", "Account");
			}

			PasswordVerificationResult Pswrdresult = passwordHasher.VerifyHashedPassword(passwordUser.User, passwordUser.User.PasswordHash, passwordUser.OldPassword);

			if (Pswrdresult == PasswordVerificationResult.Success)
			{
				passwordUser.User.PasswordHash = passwordHasher.HashPassword(passwordUser.User, passwordUser.NewPassword);

				IdentityResult result = await userManager.UpdateAsync(passwordUser.User);

				if (result.Succeeded)
				{
					ViewBag.Message = "Password successfully updated.";
					return View(passwordUser);
				}
				else
				{
					ViewBag.ErrorMessage = "New password is invalid";
					return View(passwordUser);
				}
			}
			else
			{
				ViewBag.ErrorMessage = "Old Password is invalid.";
				return View(passwordUser);
			}
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ChangeEmail(string newEmail)
		{
			UserModel userModel = await userManager.GetUserAsync(HttpContext.User);

			if (userModel == null)
			{
				return RedirectToAction("Logout", "Account");
			}

			if (userModel.Email == newEmail)
			{
				TempData["ErrorMessage"] = "Email address is the same.";
				return RedirectToAction("Security");
			}

			if (!string.IsNullOrEmpty(newEmail))
			{
				userModel.Email = newEmail;
				IdentityResult result = await userManager.UpdateAsync(userModel);
				if (result.Succeeded)
				{
					TempData["Message"] = "Email address successfully updated.";
				}
				else
				{
					TempData["ErrorMessage"] = "Sorry something went worng.";
				}
			}

			return RedirectToAction("Security");
		}

		private async Task<byte[]> GetBytes(IFormFile formFile)
		{
			await using var memoryStream = new MemoryStream();
			await formFile.CopyToAsync(memoryStream);
			return memoryStream.ToArray();
		}
	}
}