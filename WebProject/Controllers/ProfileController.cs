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
	public class ProfileController : Controller
	{
		private readonly WebProjectContext _Models;
		private readonly ILogger<ProfileController> _Logger;
		private readonly UserManager<UserModel> userManager;

		public ProfileController(WebProjectContext Models, UserManager<UserModel> manager, ILogger<ProfileController> logger)
		{
			_Models = Models;
			_Logger = logger;
			userManager = manager;
		}

		public async Task<IActionResult> Index()
		{
			UserModel userModel = await userManager.GetUserAsync(HttpContext.User);

			if (userModel == null)
			{
				return NotFound();
			}

            userModel.Posts = await _Models.Posts.Where(u => u.UserId == userModel.Id).ToListAsync();

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
		public async Task<IActionResult> Index(string Content, IFormFile Media)
		{
			UserModel userModel = await userManager.GetUserAsync(HttpContext.User);
			PostModel post = new PostModel();

			if (!string.IsNullOrEmpty(Content))
			{
				post.Content = Content;
			}

			if (Media != null)
			{
				post.Media = Convert.ToBase64String(await GetBytes(Media));
			}

			if (!string.IsNullOrEmpty(post.Content) || post.Media != null)
			{
				post.UserId = userModel.Id;
				_Models.Posts.Add(post);
				await _Models.SaveChangesAsync();
			}

			return View(userModel);
		}

		[HttpPost]
		public async Task<IActionResult> EditPost(int? PostId, string Content, IFormFile Media, string DeleteMedia)
		{
			PostModel post = new PostModel();
			UserModel userModel = await userManager.GetUserAsync(HttpContext.User);

			if (Media != null)
			{
				post.Media = Convert.ToBase64String(await GetBytes(Media));
			}
			else if (DeleteMedia == "true" && Media == null)
			{
				post.Media = null;
			}

			if (!string.IsNullOrEmpty(Content) && Content != post.Content)
			{
				post.Content = Content;
			}

			try
			{
				post.IsEdited = true;
				post.UserId = userModel.Id;
				_Models.Posts.Update(post);
				await _Models.SaveChangesAsync();
				TempData["Message"] = "Post successfully updated.";
			} 
			catch
			{
				TempData["ErrorMessage"] = "Sorry, something went wrong";
				return RedirectToAction("Index");
			}

			return RedirectToAction("Index");
		}

		[HttpPost]
		public async Task<IActionResult> LookforPost(int? PostId)
		{
			if (PostId == null)
			{
				return View();
			}

			PostModel postModel = await _Models.Posts.FirstOrDefaultAsync(us => us.Id == PostId);

			if (postModel == null)
			{
				return View();
			}

			return PartialView("PartialEditPost", postModel);
		}

		[HttpGet]
		public IActionResult LookForCreatePost() => PartialView("CreatePost");

		public async Task<IActionResult> DeletePost(int? PostId)
		{
			if (PostId == null)
			{
				return NotFound();
			}

			PostModel postModel = await _Models.Posts.FirstOrDefaultAsync(us => us.Id == PostId);

			if (postModel != null)
			{
				_Models.Remove(postModel);
				await _Models.SaveChangesAsync();
			}

			return RedirectToAction("Index");
		}

		private async Task<byte[]> GetBytes(IFormFile formFile)
		{
			await using var memoryStream = new MemoryStream();
			await formFile.CopyToAsync(memoryStream);
			return memoryStream.ToArray();
		}
	}
}
