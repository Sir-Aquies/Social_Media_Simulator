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
				return NotFound();
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
				ViewBag.ErrorMessage = "User not found";
				return View(userModel);
			}
		}

		public async Task<IActionResult> Appearance()
		{
			UserModel userModel = await userManager.GetUserAsync(HttpContext.User);

			if (userModel == null)
			{
				return NotFound();
			}

			return View(userModel);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Appearance(bool ShowImages)
		{
			UserModel userModel = await userManager.GetUserAsync(HttpContext.User);

			if (userModel == null)
			{
				return NotFound();
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

			return View(userModel);
		}

		public async Task<IActionResult> Security()
		{
			UserModel userModel = await userManager.GetUserAsync(HttpContext.User);

			if (userModel == null)
			{
				return NotFound();
			}

			if (TempData["ErrorMessage"] != null)
			{
				ViewBag.ErrorMessage = TempData["ErrorMessage"].ToString();
			}

			if (TempData["Message"] != null)
			{
				ViewBag.Message = TempData["Message"].ToString();
			}

			return View(userModel);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Security(string newPassword, string oldPassword)
		{
			UserModel userModel = await userManager.GetUserAsync(HttpContext.User);

			if (string.IsNullOrEmpty(oldPassword))
			{
				ViewBag.ErrorMessage = "Old Password is invalid";
				return View(userModel);
			}

			if (!string.IsNullOrEmpty(newPassword))
			{
				userModel.PasswordHash = passwordHasher.HashPassword(userModel, newPassword);

				IdentityResult result = await userManager.UpdateAsync(userModel);

				if (result.Succeeded)
				{
					ViewBag.Message = "Password successfully updated.";
					return View(userModel);
				}
				else
				{
					ViewBag.ErrorMessage = "Sorry something went worng.";
					return View(userModel);
				}
			}

			return View(userModel);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ChangeEmail(string newEmail)
		{
			UserModel userModel = await userManager.GetUserAsync(HttpContext.User);

			if (userModel == null)
			{
				return NotFound();
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