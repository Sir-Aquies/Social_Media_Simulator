#nullable disable
using Microsoft.AspNetCore.Mvc;
using WebProject.Models;
using WebProject.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Hosting;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Diagnostics;

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

		//TODO - add a way to filter post (most/least likes, most/least commentsm, etc).
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

		public async Task<IActionResult> CompletePost(string userName, int postId)
		{
			UserModel user = await userManager.GetUserAsync(HttpContext.User);

			if (user == null)
			{
				return RedirectToAction("Login", "Account");
			}

			UserModel pageUser = new();
			if (!string.IsNullOrEmpty(userName) && user.UserName != userName)
			{
				pageUser = await userManager.FindByNameAsync(userName);

				if (pageUser != null)
				{
					PostModel post = await _Models.Posts.FromSqlRaw($"Select * from Posts where Id = {postId}").AsNoTracking().FirstOrDefaultAsync();
					post.UsersLikes = await GetPostLikesSelective(post.Id);
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
				PostModel post = await _Models.Posts.FromSqlRaw($"Select * from Posts where Id = {postId}").AsNoTracking().FirstOrDefaultAsync();
				post.UsersLikes = await GetPostLikesSelective(post.Id);
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
			List<PostModel> output = new();

			foreach (PostModel p in posts)
			{
				foreach (UserModel u in p.UsersLikes)
				{
					if (u.Id == userId)
					{
						output.Add(p);
					}
				}
			}

			return output;
		}

		private List<CommentModel> GetCommentsLiked(IList<PostModel> posts, string userId)
		{
			List<CommentModel> output = new();

			foreach (PostModel p in posts)
			{
				foreach (CommentModel c in p.Comments)
				{
					foreach (UserModel u in c.UsersLikes)
					{
						if (u.Id == userId)
						{
							output.Add(c);
						}
					}
				}
			}

			return output;
		}
		//TODO - Load only the first posts and as the user scroll down fetch more.
		public async Task<List<PostModel>> GetPosts(UserModel user)
		{
			List<PostModel> posts = await _Models.Posts.FromSqlRaw($"Select * from Posts where UserId = '{user.Id}'").AsNoTracking().ToListAsync();

			foreach (PostModel post in posts)
			{
				post.UsersLikes = await GetPostLikesSelective(post.Id);
				post.Comments = await LoadComments(post);
				post.User = user;
			}

			return posts;
		}

		//TODO - show who like the post.
		public async Task<List<CommentModel>> LoadComments(PostModel post)
		{
			List<CommentModel> comments = await _Models.Comments.FromSqlRaw($"Select * from Comments where PostId = {post.Id}").AsNoTracking().ToListAsync();

			foreach (CommentModel comment in comments)
			{
				comment.UsersLikes = await GetCommentLikesSelective(comment.Id);
				comment.User = await _Models.Users
				.Select(u => new UserModel { Id = u.Id, UserName = u.UserName, ProfilePicture = u.ProfilePicture })
				.Where(u => u.Id == comment.UserId).AsNoTracking().FirstOrDefaultAsync();
				comment.Post = post;
			}

			return comments;
		}

		public async Task<List<UserModel>> GetPostLikesSelective(int postId)
		{
			List<UserModel> users = new();

			string[] userLikes = await _Models.Database.SqlQueryRaw<string>($"Select UserId from PostLikes where PostId = { postId }").ToArrayAsync();

			for (int i = 0; i < userLikes.Length; i++)
			{
				users.Add(new UserModel { Id = userLikes[i] });
			}

			return users;
		}

		public async Task<List<UserModel>> GetCommentLikesSelective(int commentId)
		{
			List<UserModel> users = new();

			string[] userLikes =await _Models.Database.SqlQueryRaw<string>($"Select UserId from CommentLikes where CommentId = { commentId }").ToArrayAsync();

			for (int i = 0; i < userLikes.Length; i++)
			{
				users.Add(new UserModel { Id = userLikes[i] });
			}

			return users;
		}

		public async Task<IActionResult> AllUsers()
		{
			UserModel userModel = await userManager.GetUserAsync(HttpContext.User);

			return View(userModel);
		}
	}
}
