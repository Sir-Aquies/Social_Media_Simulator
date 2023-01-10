#nullable disable
using Microsoft.AspNetCore.Mvc;
using WebProject.Models;
using WebProject.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Hosting;

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

		//TODO - With the likes algorithm then create a tendency page with the most likes and commented post.
		//TODO - finnish learning angular and add it to this project and make the respective improvements.
		public async Task<IActionResult> SearchUser(string UserName)
		{
			UserModel userModel = await userManager.GetUserAsync(HttpContext.User);

			if (!string.IsNullOrEmpty(UserName))
			{
				return RedirectToAction("UserPage", new { UserName });
			}

			return RedirectToAction("UserPage", new { userModel.UserName });
		}
		
		public async Task<IActionResult> UserPage(string UserName)
		{
			UserModel user = await userManager.GetUserAsync(HttpContext.User);

			if (user == null)
			{
				return RedirectToAction("Login", "Account");
			}

			UserModel pageUser = new();

			if (!string.IsNullOrEmpty(UserName) && user.UserName != UserName)
			{
				pageUser = await userManager.FindByNameAsync(UserName);

				if (pageUser != null)
				{
					pageUser.Posts = await GetPosts(pageUser);
				}
				else
				{
					//TODO - set up a user not found view.
					return NotFound();
				}

			}
			else
			{
				pageUser = user;
				pageUser.Posts = await GetPosts(pageUser);
			}

			user.LikedPost = GetPostsLiked(pageUser.Posts, user.Id);

			user.LikedComments = GetCommentsLiked(pageUser.Posts, user.Id);


			DynamicUser dynamic = new()
			{
				User = user,
				PageUser = pageUser
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

		public async Task<IActionResult> CompletePost(string UserName, int PostId)
		{
			UserModel user = await userManager.GetUserAsync(HttpContext.User);

			if (user == null)
			{
				return RedirectToAction("Login", "Account");
			}

			UserModel pageUser = new();

			if (!string.IsNullOrEmpty(UserName) && user.UserName != UserName)
			{
				pageUser = await userManager.FindByNameAsync(UserName);

				if (pageUser != null)
				{
					PostModel post = await _Models.Posts.Include(p => p.UsersLikes).AsNoTracking().FirstOrDefaultAsync(p => p.Id == PostId);
					post.User = pageUser;
					post.Comments = await LoadComments(post);
					pageUser.Posts = new List<PostModel> { post };
				}
				else
				{
					//TODO - set up a user not found view.
					return NotFound();
				}

			}
			else
			{
				pageUser = user;
				PostModel post = await _Models.Posts.Include(p => p.UsersLikes).AsNoTracking().FirstOrDefaultAsync(p => p.Id == PostId);
				post.User = pageUser;
				post.Comments = await LoadComments(post);
				pageUser.Posts = new List<PostModel> { post };
			}

			user.LikedPost = GetPostsLiked(pageUser.Posts, user.Id);

			user.LikedComments = GetCommentsLiked(pageUser.Posts, user.Id);

			DynamicUser dynamic = new()
			{
				User = user,
				PageUser = pageUser
			};

			return View(dynamic);
		}

		private List<PostModel> GetPostsLiked(IList<PostModel> posts, string userId)
		{
			List<PostModel> postsLiked = new();

			foreach (PostModel p in posts)
			{
				foreach (UserModel u in p.UsersLikes)
				{
					if (u.Id == userId)
					{
						postsLiked.Add(p);
					}
				}
			}

			return postsLiked;
		}

		private List<CommentModel> GetCommentsLiked(IList<PostModel> posts, string userId)
		{
			List<CommentModel> commentsLiked = new();

			foreach (PostModel p in posts)
			{
				foreach (CommentModel c in p.Comments)
				{
					foreach (UserModel u in c.UsersLikes)
					{
						if (u.Id == userId)
						{
							commentsLiked.Add(c);
						}
					}
				}
			}

			return commentsLiked;
		}

		public async Task<List<PostModel>> GetPosts(UserModel user)
		{
			List<PostModel> output = new();

			foreach (PostModel p in _Models.Posts.AsNoTracking())
			{
				PostModel post = p;

				if (post.UserId == user.Id)
				{
					post = await _Models.Posts.Include(p => p.UsersLikes).AsNoTracking().FirstOrDefaultAsync(p => p.Id == post.Id);
					post.User = user;
					post.Comments = await LoadComments(post);
					output.Add(post);
				}
			}

			return output;
		}

		public async Task<List<CommentModel>> LoadComments(PostModel post)
		{
			List<CommentModel> output = new();

			foreach (CommentModel c in _Models.Comments.AsNoTracking())
			{
				CommentModel comment = c;

				if (comment.PostId == post.Id)
				{
					comment = await _Models.Comments.Include(c => c.User).Include(c => c.UsersLikes).FirstOrDefaultAsync(c => c.Id == comment.Id);
					comment.Post = post;
					output.Add(comment);
				}
			}

			return output;
		}

		public async Task<IActionResult> AllUsers()
		{
			UserModel userModel = await userManager.GetUserAsync(HttpContext.User);

			return View(userModel);
		}
	}
}
