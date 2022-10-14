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
	public class PostController : Controller
	{
		private readonly WebProjectContext _Models;
		private readonly UserManager<UserModel> userManager;

		public PostController(WebProjectContext Models, UserManager<UserModel> manager)
		{
			_Models = Models;
			userManager = manager;
		}

		[HttpPost]
		public IActionResult LookForCreatePost() => PartialView("CreatePost");

		[HttpPost]
		public async Task<IActionResult> CreatePost(string Content, IFormFile Media)
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
				post.PostDate = DateTime.Now;
				post.UserId = userModel.Id;
				_Models.Posts.Add(post);
				await _Models.SaveChangesAsync();
			}

			return RedirectToAction("UserPage", "User", new { userModel.UserName });
		}

		[HttpPost]
		public async Task<IActionResult> EditPost(int PostId, string Content, IFormFile Media, string DeleteMedia)
		{
			PostModel post = await _Models.Posts.FirstOrDefaultAsync(p => p.Id == PostId);
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

			if (!string.IsNullOrEmpty(post.Content) || post.Media != null)
			{
				post.IsEdited = true;
				post.EditedDate = DateTime.Now;

				_Models.Posts.Update(post);
				await _Models.SaveChangesAsync();
				TempData["Message"] = "Post successfully updated.";

				return RedirectToAction("UserPage", "User", new { userModel.UserName });
			}
			else
			{
				TempData["ErrorMessage"] = "Sorry, something went wrong";
			}

			return RedirectToAction("UserPage", "User", new { userModel.UserName });
		}

		[HttpPost]
		public async Task<IActionResult> LookforPost(int PostId)
		{
			PostModel postModel = await _Models.Posts.AsNoTracking().FirstOrDefaultAsync(us => us.Id == PostId);

			if (postModel == null)
			{
				return NotFound();
			}

			return PartialView("EditPost", postModel);
		}

		[HttpPost]
		public async Task<IActionResult> DeletePost(int PostId)
		{
			UserModel userModel = await userManager.GetUserAsync(HttpContext.User);
			PostModel postModel = await _Models.Posts.Include(p => p.Comments).ThenInclude(c => c.UsersLikes).Include(p => p.UsersLikes).FirstOrDefaultAsync(us => us.Id == PostId);

			if (postModel != null)
			{
				postModel.UsersLikes.Clear();
				foreach (CommentModel comment in postModel.Comments)
				{
					comment.UsersLikes.Clear();
				}

				_Models.Remove(postModel);
				await _Models.SaveChangesAsync();
			}

			return RedirectToAction("UserPage", "User", new { userModel.UserName });
		}

		[HttpPost]
		public async Task<string> LikePost(int PostId)
		{
			UserModel userModel = await userManager.GetUserAsync(HttpContext.User);

			if (userModel != null && PostId != 0)
			{
				PostModel post = await _Models.Posts.Include(p => p.UsersLikes).FirstOrDefaultAsync(p => p.Id == PostId);

				if (post.UsersLikes.Contains(userModel))
				{
					post.Likes--;
					post.UsersLikes.Remove(userModel);
					await _Models.SaveChangesAsync();
					return "-";
				}
				else
				{
					post.Likes++;
					post.UsersLikes.Add(userModel);
					await _Models.SaveChangesAsync();
					return "+";
				}

			}

			return "0";
		}

		[HttpPost]
		public IActionResult LookForCreateComment(int PostId, string UserName)
		{
			TempData["PostId"] = PostId;
			TempData["UserName"] = UserName;

			return PartialView("CreateComment");
		}

		[HttpPost]
		public async Task<IActionResult> CreateComment(string Content)
		{
			UserModel userModel = await userManager.GetUserAsync(HttpContext.User);

			string UserName = "";
			int PostId = 0;
			bool PostIdBool = false;

			if (!string.IsNullOrEmpty(TempData["PostId"]?.ToString()) && !string.IsNullOrEmpty(TempData["UserName"]?.ToString()))
			{
				UserName = TempData["UserName"]?.ToString() ?? "empty";
				PostIdBool = int.TryParse(TempData["PostId"]?.ToString(), out PostId);
			}

			if (!string.IsNullOrEmpty(Content) && PostIdBool && !string.IsNullOrEmpty(UserName))
			{
				CommentModel comment = new()
				{
					Content = Content,
					UserId = userModel.Id,
					PostId = PostId,
					Date = DateTime.Now,
				};

				_Models.Add(comment);
				await _Models.SaveChangesAsync();

				return RedirectToAction("UserPage", "User", new { UserName });
			}

			return RedirectToAction("UserPage", "User", new { userModel.UserName });
		}

		public async Task<IActionResult> DeleteComment(int CommentId)
		{
			UserModel userModel = await userManager.GetUserAsync(HttpContext.User);
			CommentModel comment = await _Models.Comments.Include(c => c.UsersLikes)
					.Include(c => c.Post).ThenInclude(p => p.User).FirstOrDefaultAsync(c => c.Id == CommentId);

			if (comment != null)
			{
				comment.UsersLikes.Clear();
				_Models.Comments.Remove(comment);
				await _Models.SaveChangesAsync();
			}

			return RedirectToAction("UserPage", "User", new { UserName = comment.Post.User.UserName });
		}

		[HttpPost]
		public async Task<string> LikeComment(int CommentId)
		{
			UserModel userModel = await userManager.GetUserAsync(HttpContext.User);

			if (userModel != null && CommentId != 0)
			{
				CommentModel comment = await _Models.Comments.Include(p => p.UsersLikes).FirstOrDefaultAsync(p => p.Id == CommentId);

				if (comment.UsersLikes.Contains(userModel))
				{
					comment.Likes--;
					comment.UsersLikes.Remove(userModel);
					await _Models.SaveChangesAsync();
					return "-";
				}
				else
				{
					comment.Likes++;
					comment.UsersLikes.Add(userModel);
					await _Models.SaveChangesAsync();
					return "+";
				}

			}

			return "0";
		}

		private async Task<byte[]> GetBytes(IFormFile formFile)
		{
			await using var memoryStream = new MemoryStream();
			await formFile.CopyToAsync(memoryStream);
			return memoryStream.ToArray();
		}
	}
}
