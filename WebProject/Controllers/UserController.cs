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
				NotFound();
			}

			UserModel userModel = await userManager.GetUserAsync(HttpContext.User);

			if (userModel == null)
			{
				return RedirectToAction("Logout", "Account");
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
			else
			{
				NotFound();
				//TODO - create a not found page.
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
		public IActionResult LookForCreateComment(int PostId) => PartialView("CreateComment", PostId);

		[HttpPost]
		public async Task<IActionResult> CreateComment(string Content, int PostId)
		{
			UserModel userModel = await userManager.GetUserAsync(HttpContext.User);

			if (userModel == null)
			{
				return RedirectToAction("Logout", "Account");
			}

			if (!string.IsNullOrEmpty(Content) && PostId != 0)
			{
				CommentModel comment = new()
				{
					Content = Content,
					UserId = userModel.Id,
					PostId = PostId
				};

				_Models.Add(comment);
				await _Models.SaveChangesAsync();
			}

			return RedirectToAction("UserPage");
		}
	}
}
