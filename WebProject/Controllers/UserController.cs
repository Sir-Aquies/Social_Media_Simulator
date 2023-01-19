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
using Microsoft.Identity.Client;

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
		public async Task<IActionResult> SearchUser(string userName)
		{
			UserModel userModel = await userManager.GetUserAsync(HttpContext.User);

			return !string.IsNullOrEmpty(userName) ? RedirectToAction("UserPage", new { userName }) : RedirectToAction("UserPage", new { userModel.UserName });
		}

		//TODO - add a way to filter post (most/least likes, most/least commentsm, etc).
		public async Task<IActionResult> UserPage(string userName)
		{
			UserModel user = await userManager.GetUserAsync(HttpContext.User);

			if (user == null) return RedirectToAction("Login", "Account");

			UserModel pageUser = (!string.IsNullOrEmpty(user.UserName) && userName != user.UserName)
									? await userManager.FindByNameAsync(userName) : user;

			if (pageUser == null) return NotFound();

			pageUser.Posts = await GetPosts(pageUser);

			List<int> likes = await _Models.Database.SqlQueryRaw<int>("SELECT SUM(Likes) FROM Posts WHERE UserId = {0}", pageUser.Id).ToListAsync();
			TempData["TotalLikes"] = likes.First();

			List<int> posts = await _Models.Database.SqlQueryRaw<int>("SELECT COUNT(Id) FROM Posts WHERE UserId = {0}", pageUser.Id).ToListAsync();
			TempData["TotalPosts"] = posts.First();

			return View(new DynamicUser { User = user, PageUser = pageUser });
		}

		public async Task<IActionResult> MediaPosts(string userName)
		{
			UserModel user = await userManager.GetUserAsync(HttpContext.User);

			if (user == null) return RedirectToAction("Login", "Account");

			UserModel pageUser = (!string.IsNullOrEmpty(user.UserName) && userName != user.UserName) 
									? await userManager.FindByNameAsync(userName) : user;

			if (pageUser == null) return NotFound();

			pageUser.Posts = await GetPosts(pageUser, true);

			List<int> likes = await _Models.Database.SqlQueryRaw<int>("SELECT SUM(Likes) FROM Posts WHERE UserId = {0}", pageUser.Id).ToListAsync();
			TempData["TotalLikes"] = likes.First();

			List<int> posts = await _Models.Database.SqlQueryRaw<int>("SELECT COUNT(Id) FROM Posts WHERE UserId = {0}", pageUser.Id).ToListAsync();
			TempData["TotalPosts"] = posts.First();

			return View(new DynamicUser { User = user, PageUser = pageUser });
		}

		public async Task<IActionResult> CompletePost(string userName, int postId)
		{
			UserModel user = await userManager.GetUserAsync(HttpContext.User);

			if (user == null) return RedirectToAction("Login", "Account");

			UserModel pageUser = (!string.IsNullOrEmpty(user.UserName) && userName != user.UserName)
									? await userManager.FindByNameAsync(userName) : user;

			//TODO - set up a user not found page.
			if (pageUser == null) return NotFound();

			PostModel post = await _Models.Posts.FromSqlRaw($"Select * from Posts where Id = {postId}").AsNoTracking().FirstOrDefaultAsync();
			post.UsersLikes = await GetPostLikesSelective(post.Id);
			post.User = pageUser;
			post.Comments = await LoadComments(post);
			pageUser.Posts = new List<PostModel> { post };

			List<int> likes = await _Models.Database.SqlQueryRaw<int>("SELECT SUM(Likes) FROM Posts WHERE UserId = {0}", pageUser.Id).ToListAsync();
			TempData["TotalLikes"] = likes.First();

			List<int> posts = await _Models.Database.SqlQueryRaw<int>("SELECT COUNT(Id) FROM Posts WHERE UserId = {0}", pageUser.Id).ToListAsync();
			TempData["TotalPosts"] = posts.First();

			return View(new DynamicUser { User = user, PageUser = pageUser });
		}

		//TODO - think where to put the liked comments.
		public async Task<IActionResult> LikedPosts(string userName)
		{
			UserModel user = await userManager.GetUserAsync(HttpContext.User);

			if (user == null) return RedirectToAction("Login", "Account");

			UserModel pageUser = (!string.IsNullOrEmpty(user.UserName) && userName != user.UserName)
				? await userManager.FindByNameAsync(userName) : user; 

			if (pageUser == null) return NotFound();

			pageUser.LikedPost = await GetLikedPost(pageUser.Id);

			List<int> likes = await _Models.Database.SqlQueryRaw<int>("SELECT SUM(Likes) FROM Posts WHERE UserId = {0}", pageUser.Id).ToListAsync();
			TempData["TotalLikes"] = likes.First();

			List<int> posts = await _Models.Database.SqlQueryRaw<int>("SELECT COUNT(Id) FROM Posts WHERE UserId = {0}", pageUser.Id).ToListAsync();
			TempData["TotalPosts"] = posts.First();

			return View(new DynamicUser { User = user, PageUser = pageUser});
		}

		public async Task<IActionResult> CommentedPosts(string userName)
		{
			UserModel user = await userManager.GetUserAsync(HttpContext.User);

			if (user == null) return RedirectToAction("Login", "Account");

			UserModel pageUser = (!string.IsNullOrEmpty(user.UserName) && userName != user.UserName)
				? await userManager.FindByNameAsync(userName) : user;

			if (pageUser == null) return NotFound();

			pageUser.Posts = await _Models.Posts
						.FromSqlRaw("Select * from Posts where Id in (Select PostId from Comments where UserId = {0})", pageUser.Id)
						.AsNoTracking().ToListAsync();

			pageUser.Comments = await _Models.Comments
				.FromSqlRaw("Select * from Comments where UserId = {0}", pageUser.Id)
				.AsNoTracking().ToListAsync();

			foreach (CommentModel comment in pageUser.Comments)
			{
				comment.UsersLikes = await GetCommentLikesSelective(comment.Id);
				comment.User = pageUser;

				foreach (PostModel post in pageUser.Posts)
				{
					post.UsersLikes = await GetPostLikesSelective(post.Id);
					post.User = await _Models.Users
						.Select(u => new UserModel { Id = u.Id, UserName = u.UserName, ProfilePicture = u.ProfilePicture })
						.Where(u => u.Id == post.UserId).AsNoTracking().FirstOrDefaultAsync();
					post.Comments = new List<CommentModel>();

					if (comment.PostId == post.Id)
					{
						post.Comments.Add(comment);
						comment.Post = post;
					}
				}
			}

			List<int> likes = await _Models.Database.SqlQueryRaw<int>("SELECT SUM(Likes) FROM Posts WHERE UserId = {0}", pageUser.Id).ToListAsync();
			TempData["TotalLikes"] = likes.First();

			List<int> posts = await _Models.Database.SqlQueryRaw<int>("SELECT COUNT(Id) FROM Posts WHERE UserId = {0}", pageUser.Id).ToListAsync();
			TempData["TotalPosts"] = posts.First();


			return View(new DynamicUser { User = user, PageUser = pageUser });
		}

		//Eliot09 Id 9a0a92b8-2c82-4199-9972-d1731a300f0c
		private async Task<List<PostModel>> GetLikedPost(string userId)
		{
			List<PostModel> posts = await _Models.Posts
				.FromSqlRaw("Select * from Posts Where Id in (Select PostId from PostLikes where UserId = {0})", userId)
				.AsNoTracking().ToListAsync();

			foreach (PostModel post in posts)
			{
				post.UsersLikes = await GetPostLikesSelective(post.Id);
				post.User = await _Models.Users
				.Select(u => new UserModel { Id = u.Id, UserName = u.UserName, ProfilePicture = u.ProfilePicture })
				.Where(u => u.Id == post.UserId).AsNoTracking().FirstOrDefaultAsync();
				post.Comments = await LoadComments(post);
			}

			return posts;
		}

		//TODO - Load only the first posts and as the user scroll down fetch more.
		private async Task<List<PostModel>> GetPosts(UserModel user, bool onlyMedia = false)
		{
			string sql = onlyMedia ? "SELECT * FROM Posts WHERE UserId = {0} AND Media IS NOT NULL;" : "Select * from Posts where UserId = {0}";
			List<PostModel> posts = await _Models.Posts.FromSqlRaw(sql, user.Id).AsNoTracking().ToListAsync();

			foreach (PostModel post in posts)
			{
				post.UsersLikes = await GetPostLikesSelective(post.Id);
				post.Comments = await LoadComments(post);
				post.User = user;
			}

			return posts;
		}

		private async Task<List<CommentModel>> LoadComments(PostModel post)
		{
			List<CommentModel> comments = await _Models.Comments.FromSqlRaw("Select * from Comments where PostId = {0}", post.Id).AsNoTracking().ToListAsync();

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

		private async Task<List<UserModel>> GetPostLikesSelective(int postId)
		{
			List<UserModel> users = new();

			string[] userLikes = await _Models.Database.SqlQueryRaw<string>("Select UserId from PostLikes where PostId = {0}", postId).AsNoTracking().ToArrayAsync();

			for (int i = 0; i < userLikes.Length; i++)
			{
				users.Add(new UserModel { Id = userLikes[i] });
			}

			return users;
		}

		private async Task<List<UserModel>> GetCommentLikesSelective(int commentId)
		{
			List<UserModel> users = new();

			string[] userLikes = await _Models.Database.SqlQueryRaw<string>("Select UserId from CommentLikes where CommentId = {0}", commentId).AsNoTracking().ToArrayAsync();

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

		//if (TempData["ErrorMessage"] != null)
		//{
		//	ViewBag.ErrorMessage = TempData["ErrorMessage"].ToString();
		//}

		//if (TempData["Message"] != null)
		//{
		//	ViewBag.Message = TempData["Message"].ToString();
		//}
	}
}
