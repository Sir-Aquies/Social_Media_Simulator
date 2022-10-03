#nullable disable
using Microsoft.AspNetCore.Mvc;
using WebProject.Models;
using WebProject.Data;
using Microsoft.EntityFrameworkCore;

namespace WebProject.Controllers
{
	public class ProfileController : Controller
	{
		private readonly WebProjectContext _Models;
		private readonly ILogger<ProfileController> _Logger;

		public ProfileController(WebProjectContext Models, ILogger<ProfileController> logger)
		{
			_Models = Models;
			_Logger = logger;
		}

		public async Task<IActionResult> Index(int? UserId)
		{
			if (UserId == null)
			{
				return View(null);
			}

			UserModel userModel = await _Models.Users.Include(u => u.Posts).AsNoTracking().FirstOrDefaultAsync(us => us.Id == UserId);

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
		public async Task<IActionResult> Index(string Content, int UserId, IFormFile pic)
		{
			PostModel post = new PostModel();
			post.UserModelId = UserId;

			if (Content != null)
			{
				post.PostContent = Content;
			}

			if (pic != null)
			{
				post.Media = await GetBytes(pic);
				//var picbyte = await GetBytes(pic);
				//post.Media = Convert.ToBase64String(picbyte);
				//TODO - change the byte[] to nvarchar(MAX).
			}

			if (!ModelState.IsValid)
			{
				return RedirectToAction("UserPage", "Profile", new { UserId = UserId });
			}

			_Models.Posts.Add(post);
			await _Models.SaveChangesAsync();

			return RedirectToAction("Index", new { UserId });
		}

		[HttpPost]
		public async Task<IActionResult> EditPost(int? PostId, int UserId, string Content, IFormFile Media, string DeleteMedia)
		{
			PostModel postModel = await _Models.Posts.FirstOrDefaultAsync(us => us.Id == PostId);
			
			if (postModel != null)
			{
				if (Media != null)
				{
					postModel.Media = await GetBytes(Media);
				}
				else if (DeleteMedia == "true" && Media == null)
				{
					postModel.Media = null;
				}

				if (!string.IsNullOrEmpty(Content) && Content != postModel.PostContent)
				{
					postModel.PostContent = Content;
				}

				postModel.IsEdited = true;
				_Models.Attach(postModel).State = EntityState.Modified;
				await _Models.SaveChangesAsync();
				TempData["Message"] = "Post successfully updated.";
			}
			else
			{
				TempData["ErrorMessage"] = "Sorry, something went wrong";
				return RedirectToAction("Index", new { UserId = UserId });
			}

			return RedirectToAction("Index", new { UserId = UserId });
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

		public async Task<IActionResult> DeletePost(int? PostId, int? UserId)
		{
			if (PostId == null)
			{
				return NotFound();
			}

			PostModel postModel = await _Models.Posts.FirstOrDefaultAsync(us => us.Id == PostId);

			if (postModel != null)
			{
				//postModel.PostContent = null;
				//postModel.UserModelId = 0;
				//postModel.Media = null;
				//.Attach(postModel).State = EntityState.Modified;
				_Models.Remove(postModel);
				// TODO - make it so that i doesn't remove the post from the database.
				await _Models.SaveChangesAsync();
			}

			return RedirectToAction("Index", new { UserId = UserId });
		}

		private async Task<byte[]> GetBytes(IFormFile formFile)
		{
			await using var memoryStream = new MemoryStream();
			await formFile.CopyToAsync(memoryStream);
			return memoryStream.ToArray();
		}
	}
}
