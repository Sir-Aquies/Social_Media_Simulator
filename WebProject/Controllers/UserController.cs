﻿#nullable disable
using Microsoft.AspNetCore.Mvc;
using WebProject.Models;
using WebProject.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Authorization;

namespace WebProject.Controllers
{
	[Authorize]
	public class UserController : Controller
	{
		private readonly WebProjectContext _Models;
		private readonly ILogger<UserController> _Logger;
		private readonly UserManager<UserModel> userManager;

		public UserController(WebProjectContext Models, UserManager<UserModel> manager, ILogger<UserController> logger)
		{
			_Models = Models;
			userManager = manager;
			_Logger = logger;
		}

		public async Task<IActionResult> UserPage(string UserName)
		{
			UserModel userModel = await userManager.GetUserAsync(HttpContext.User);

			userModel.LikedPost = await (from post in _Models.Posts where post.UsersLikes.Contains(userModel) select post).AsNoTracking().ToListAsync();
			userModel.LikedComments = await (from com in _Models.Comments where com.UsersLikes.Contains(userModel) select com).AsNoTracking().ToListAsync();

			if (userModel == null)
			{
				return RedirectToAction("Login", "Account");
			}

			UserModel page = new();

			if (!string.IsNullOrEmpty(UserName))
			{
				page = await userManager.FindByNameAsync(UserName);

				if (page != null)
				{
					page.Posts = await _Models.Posts.Include(p => p.Comments).ThenInclude(c => c.User).Where(p => p.UserId == page.Id).AsNoTracking().ToListAsync();
					foreach (var post in page.Posts)
					{
						post.User = page;
					}
				}
				else
				{
					//TODO - set up a user not found view.
					return NotFound();
				}

			}
			else
			{
				page = userModel;
				page.Posts = await _Models.Posts.Include(p => p.Comments).ThenInclude(c => c.User).Where(p => p.UserId == page.Id).AsNoTracking().ToListAsync();
				foreach (var post in page.Posts)
				{
					post.User = page;
				}
			}

			DynamicUser dynamic = new()
			{
				User = userModel,
				PageUser = page
			};

			if (TempData["ErrorMessage"] != null)
			{
				ViewBag.ErrorMessage = TempData["ErrorMessage"].ToString();
			}

			if (TempData["Message"] != null)
			{
				ViewBag.Message = TempData["Message"].ToString();
			}

			return View(dynamic);
		}

		public async Task<IActionResult> AllUsers()
		{
			UserModel userModel = await userManager.GetUserAsync(HttpContext.User);

			return View(userModel);
		}

		public async Task<IActionResult> GetUsers()
		{
			List<UserModel> users = await userManager.Users.ToListAsync();
			return PartialView("UsersList", users);
		}
	}
}