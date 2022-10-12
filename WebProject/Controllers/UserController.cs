#nullable disable
using Microsoft.AspNetCore.Mvc;
using WebProject.Models;
using WebProject.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.FileProviders;

namespace WebProject.Controllers
{
	public class UserController : Controller
	{
		private readonly WebProjectContext _Models;
		private readonly UserManager<UserModel> userManager;

		public UserController(WebProjectContext Models, UserManager<UserModel> manager)
		{
			_Models = Models;
			userManager = manager;
		}

		public async Task<IActionResult> UserPage(string UserName)
		{
			if (string.IsNullOrEmpty(UserName))
			{
				//TODO - create a not found page
				NotFound();
			}

			UserModel userModel = await userManager.GetUserAsync(HttpContext.User);

			if (userModel == null)
			{
				return RedirectToAction("Logout", "Account");
			}

			if (userModel.UserName == UserName)
			{
				return RedirectToAction("Index", "Profile");
			}

			UserModel page = await userManager.FindByNameAsync(UserName);

			if (page != null)
			{
				page.Posts = await _Models.Posts.Include(p => p.Comments).ThenInclude(c => c.User).Where(p => p.UserId == page.Id).AsNoTracking().ToListAsync();
				foreach(var post in page.Posts)
				{
					post.User = page;
				}
			}

			DynamicUser dynamic = new()
			{
				User = userModel,
				PageUser = page
			};

			return View(dynamic);
		}

		public async Task<IActionResult> AllUsers()
		{
			UserModel userModel = await userManager.GetUserAsync(HttpContext.User);

			if (userModel == null)
			{
				return RedirectToAction("Logout", "Account");
			}

			return View(userModel);
		}

		public async Task<IActionResult> GetUsers()
		{
			List<UserModel> users = await userManager.Users.ToListAsync();
			return PartialView("UsersList", users);
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

			if (userModel == null)
			{
				return RedirectToAction("Logout", "Account");
			}

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

				return RedirectToAction("UserPage", new { UserName });
			}

			return RedirectToAction("AllUsers");
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
	}
}
