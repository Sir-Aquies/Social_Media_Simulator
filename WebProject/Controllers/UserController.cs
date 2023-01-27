#nullable disable
using Microsoft.AspNetCore.Mvc;
using WebProject.Models;
using WebProject.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using WebProject.Services;

namespace WebProject.Controllers
{
	[Authorize]
	public class UserController : Controller
	{
		private readonly WebProjectContext _Models;
		private readonly ILogger<UserController> _Logger;
		private readonly UserManager<UserModel> _UserManager;

		private readonly ITendency _Tendency;

		public UserController(WebProjectContext Models, UserManager<UserModel> userManager, ILogger<UserController> logger, ITendency tendency)
		{
			_Models = Models;
			_UserManager = userManager;
			_Logger = logger;
			_Tendency = tendency;
		}

		//TODO - With the likes algorithm then create a tendency page with the most likes and commented post

		public async Task<IActionResult> SearchUser(string userName)
		{
			UserModel userModel = await _UserManager.GetUserAsync(HttpContext.User);

			return !string.IsNullOrEmpty(userName) ? RedirectToAction("UserPage", new { userName }) : RedirectToAction("UserPage", new { userModel.UserName });
		}

		//TODO - search bar for posts maybe?.
		//TODO - add a way to filter post (most/least likes, most/least commentsm, etc).
		public async Task<IActionResult> UserPage(string userName)
		{
			UserModel user = await _UserManager.GetUserAsync(HttpContext.User);

			if (user == null) 
				return RedirectToAction("Login", "Account");

			UserModel pageUser = (!string.IsNullOrEmpty(user.UserName) && userName != user.UserName)
									? await _UserManager.FindByNameAsync(userName) : user;

			if (pageUser == null)
			{
				ViewBag.unexistingUserName = userName;
				return View(new DynamicUser { User = user, PageUser = pageUser });
			}

			int loadFirstPost = 10;
			pageUser.Posts = await GetPosts(pageUser, 0, loadFirstPost);
			ViewData["startFromPost"] = loadFirstPost.ToString();

			List<int> likes = await _Models.Database
				.SqlQueryRaw<int>("SELECT SUM(Likes) FROM Posts WHERE UserId = {0}", pageUser.Id).ToListAsync();
			TempData["TotalLikes"] = likes.First();

			List<int> posts = await _Models.Database
				.SqlQueryRaw<int>("SELECT COUNT(Id) FROM Posts WHERE UserId = {0}", pageUser.Id).ToListAsync();
			TempData["TotalPosts"] = posts.First();

			return View(new DynamicUser { User = user, PageUser = pageUser });
		}

		public async Task<IActionResult> MediaPosts(string userName)
		{
			UserModel user = await _UserManager.GetUserAsync(HttpContext.User);

			if (user == null) 
				return RedirectToAction("Login", "Account");

			UserModel pageUser = (!string.IsNullOrEmpty(user.UserName) && userName != user.UserName) 
									? await _UserManager.FindByNameAsync(userName) : user;

			if (pageUser == null) 
				return NotFound();

			int loadFirstPost = 10;
			pageUser.Posts = await GetPosts(pageUser, 0, loadFirstPost, true);
			ViewData["startFromPost"] = loadFirstPost.ToString();

			List<int> likes = await _Models.Database.SqlQueryRaw<int>("SELECT SUM(Likes) FROM Posts WHERE UserId = {0}", pageUser.Id).ToListAsync();
			TempData["TotalLikes"] = likes.First();

			List<int> posts = await _Models.Database.SqlQueryRaw<int>("SELECT COUNT(Id) FROM Posts WHERE UserId = {0}", pageUser.Id).ToListAsync();
			TempData["TotalPosts"] = posts.First();

			return View(new DynamicUser { User = user, PageUser = pageUser });
		}

		public async Task<IActionResult> CompletePost(string userName, int postId)
		{
			UserModel user = await _UserManager.GetUserAsync(HttpContext.User);

			if (user == null) 
				return RedirectToAction("Login", "Account");

			UserModel pageUser = (!string.IsNullOrEmpty(user.UserName) && userName != user.UserName)
									? await _UserManager.FindByNameAsync(userName) : user;

			if (pageUser == null) 
				return View("UserPage", new DynamicUser { User = user, PageUser = pageUser });

			PostModel post = await _Models.Posts.FromSqlRaw($"Select * from Posts where Id = {postId}").AsNoTracking().FirstOrDefaultAsync();

			if (post.UserId != pageUser.Id)
			{
				post = null;
			}

			if (post != null)
			{
				post.UsersLikes = await GetPostLikesSelective(post.Id);
				post.User = pageUser;
				post.Comments = await LoadComments(post);
			}

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
			UserModel user = await _UserManager.GetUserAsync(HttpContext.User);

			if (user == null) 
				return RedirectToAction("Login", "Account");

			UserModel pageUser = (!string.IsNullOrEmpty(user.UserName) && userName != user.UserName)
				? await _UserManager.FindByNameAsync(userName) : user; 

			if (pageUser == null) 
				return NotFound();

			int loadFirstPost = 10;
			pageUser.LikedPost = await GetLikedPost(pageUser.Id, 0, loadFirstPost);
			ViewData["startFromPost"] = loadFirstPost.ToString();

			List<int> likes = await _Models.Database.SqlQueryRaw<int>("SELECT SUM(Likes) FROM Posts WHERE UserId = {0}", pageUser.Id).ToListAsync();
			TempData["TotalLikes"] = likes.First();

			List<int> posts = await _Models.Database.SqlQueryRaw<int>("SELECT COUNT(Id) FROM Posts WHERE UserId = {0}", pageUser.Id).ToListAsync();
			TempData["TotalPosts"] = posts.First();

			return View(new DynamicUser { User = user, PageUser = pageUser});
		}

		public async Task<IActionResult> CommentedPosts(string userName)
		{
			UserModel user = await _UserManager.GetUserAsync(HttpContext.User);

			if (user == null) 
				return RedirectToAction("Login", "Account");

			UserModel pageUser = (!string.IsNullOrEmpty(user.UserName) && userName != user.UserName)
				? await _UserManager.FindByNameAsync(userName) : user;

			if (pageUser == null) 
				return NotFound();

			int loadFirstPost = 10;
			pageUser.Posts = await GetCommentedPosts(pageUser, 0, loadFirstPost);
			ViewData["startFromPost"] = loadFirstPost.ToString();

			List<int> likes = await _Models.Database.SqlQueryRaw<int>("SELECT SUM(Likes) FROM Posts WHERE UserId = {0}", pageUser.Id).ToListAsync();
			TempData["TotalLikes"] = likes.First();

			List<int> posts = await _Models.Database.SqlQueryRaw<int>("SELECT COUNT(Id) FROM Posts WHERE UserId = {0}", pageUser.Id).ToListAsync();
			TempData["TotalPosts"] = posts.First();

			return View(new DynamicUser { User = user, PageUser = pageUser });
		}

		private async Task<List<PostModel>> GetCommentedPosts(UserModel user, int from, int amount)
		{
			List<PostModel> posts = new();
			List<CommentModel> comments = await _Models.Comments
				.FromSqlRaw("SELECT * FROM Comments WHERE UserId = {0} " +
				"ORDER BY Date DESC OFFSET {1} ROWS FETCH NEXT {2} ROWS ONLY;", user.Id, from, amount)
				.AsNoTracking().ToListAsync();

			foreach (CommentModel comment in comments)
			{
				comment.UsersLikes = await GetCommentLikesSelective(comment.Id);
				comment.User = user;

				PostModel post = await _Models.Posts
						.FromSqlRaw("SELECT * FROM Posts WHERE Id = {0}", comment.PostId)
						.AsNoTracking().FirstOrDefaultAsync();

				List<CommentModel> samePostComments = await _Models.Comments
					.FromSqlRaw("SELECT * FROM Comments WHERE PostId = {0} AND UserId = {1} AND Date < {2}", post.Id, user.Id, comment.Date)
					.AsNoTracking().ToListAsync();

				List<CommentModel> alreadyAddedComments = await _Models.Comments
					.FromSqlRaw("SELECT * FROM Comments WHERE PostId = {0} AND UserId = {1} AND Date > {2}", post.Id, user.Id, comment.Date)
					.AsNoTracking().ToListAsync();

				if (alreadyAddedComments.Count > 0) 
					continue;

				foreach (CommentModel c in samePostComments)
				{
					c.UsersLikes = await GetCommentLikesSelective(c.Id);
					c.User = user;
					comments.Remove(c);
				}

				post.Comments = samePostComments;

				post.Comments.Add(comment);

				post.UsersLikes = await GetPostLikesSelective(post.Id);

				post.User = await _Models.Users
						.Select(u => new UserModel { Id = u.Id, UserName = u.UserName, ProfilePicture = u.ProfilePicture })
						.Where(u => u.Id == post.UserId).AsNoTracking().FirstOrDefaultAsync();

				posts.Add(post);
			}

			return posts;
		}

		public async Task<IActionResult> LoadMorePostsComments(string userId, int from, int amount)
		{
			UserModel user = await _UserManager.FindByIdAsync(userId);
			UserModel logedUser = await _UserManager.GetUserAsync(HttpContext.User);

			List<PostModel>  posts = await GetCommentedPosts(user, from, amount);

			if (posts == null)
				return NotFound("Posts not found.");

			if (posts.Count == 0)
			{
				return NoContent();
			}

			ViewData["commentsAmount"] = 3;
			ViewData["UserId"] = logedUser.Id;
			ViewBag.blur = logedUser.ShowImages ? "1" : "0";

			return PartialView("PostList", posts);
		}

		//Eliot09   9a0a92b8-2c82-4199-9972-d1731a300f0c
		//lazycat381   fbf54244-51e6-4f57-badb-c75ff5742cb4
		private async Task<List<PostModel>> GetLikedPost(string userId, int from, int amount)
		{
			List<PostModel> posts = await _Models.Posts
				.FromSqlRaw("SELECT * FROM Posts WHERE Id IN " +
				"(SELECT PostId FROM PostLikes WHERE UserId = {0}) " +
				"ORDER BY PostDate DESC OFFSET {1} ROWS FETCH NEXT {2} ROWS ONLY", userId, from, amount)
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

		public async Task<IActionResult> LoadMorePostsLikes(string userId, int from, int amount)
		{
			List<PostModel> posts = await GetLikedPost(userId, from, amount);
			UserModel logedUser = await _UserManager.GetUserAsync(HttpContext.User);

			if (posts == null)
				return NotFound("Posts not found.");

			if (posts.Count == 0)
			{
				return NoContent();
			}

			ViewData["commentsAmount"] = 3;
			ViewData["UserId"] = logedUser.Id;
			ViewBag.blur = logedUser.ShowImages ? "1" : "0";

			return PartialView("PostList", posts);
		}

		private async Task<List<PostModel>> GetPosts(UserModel user, int from, int amount, bool onlyMedia = false)
		{
			string sql = onlyMedia ? "SELECT * FROM Posts WHERE UserId = {0} AND Media IS NOT NULL ORDER BY PostDate DESC OFFSET {1} ROWS FETCH NEXT {2} ROWS ONLY;"
				: "SELECT * FROM Posts WHERE UserId = {0} ORDER BY PostDate DESC OFFSET {1} ROWS FETCH NEXT {2} ROWS ONLY;";
			List<PostModel> posts = await _Models.Posts.FromSqlRaw(sql, user.Id, from, amount).AsNoTracking().ToListAsync();

			foreach (PostModel post in posts)
			{
				post.UsersLikes = await GetPostLikesSelective(post.Id);
				post.Comments = await LoadComments(post);
				post.User = user;
			}

			return posts;
		}

		public async Task<IActionResult> LoadMorePosts(string userId, int from, int amount, bool onlyMedia = false)
		{
			UserModel user = await _UserManager.FindByIdAsync(userId);
			UserModel logedUser = await _UserManager.GetUserAsync(HttpContext.User);

			if (user == null) 
				return NotFound("User not found.");

			List<PostModel> posts = await GetPosts(user, from, amount, onlyMedia);

			if (posts == null)
				return NotFound("Posts not found.");

			if (posts.Count == 0)
			{
				return NoContent();
			}

			ViewData["commentsAmount"] = 3;
			ViewData["UserId"] = logedUser.Id;
			ViewBag.blur = logedUser.ShowImages ? "1" : "0";

			return PartialView("PostList", posts);
		}

		private async Task<List<CommentModel>> LoadComments(PostModel post)
		{
			List<CommentModel> comments = await _Models.Comments
				.FromSqlRaw("SELECT * FROM Comments WHERE PostId = {0}", post.Id)
				.AsNoTracking().ToListAsync();

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

			string[] userLikes = await _Models.Database
				.SqlQueryRaw<string>("SELECT UserId FROM PostLikes WHERE PostId = {0}", postId)
				.AsNoTracking().ToArrayAsync();

			for (int i = 0; i < userLikes.Length; i++)
			{
				users.Add(new UserModel { Id = userLikes[i] });
			}

			return users;
		}

		private async Task<List<UserModel>> GetCommentLikesSelective(int commentId)
		{
			List<UserModel> users = new();

			string[] userLikes = await _Models.Database
				.SqlQueryRaw<string>("Select UserId from CommentLikes where CommentId = {0}", commentId)
				.AsNoTracking().ToArrayAsync();

			for (int i = 0; i < userLikes.Length; i++)
			{
				users.Add(new UserModel { Id = userLikes[i] });
			}

			return users;
		}

		public async Task<IActionResult> AllUsers()
		{
			UserModel userModel = await _UserManager.GetUserAsync(HttpContext.User);

			return View(userModel);
		}

		public async Task<IActionResult> LookUsersByUserName(string userName)
		{
			List<UserModel> users = await _Models.Users
				.FromSqlRaw("SELECT * FROM AspNetUsers WHERE UserName LIKE {0};", userName + "%").AsNoTracking().ToListAsync();

			return PartialView("UsersList", users);
		}

		public async Task<IActionResult> GetRandomUsersView(int amount)
		{
			string sql = String.Format("SELECT TOP {0} * FROM AspNetUsers ORDER BY NEWID();", amount);

			List<UserModel> users = await _Models.Users.FromSqlRaw(sql).AsNoTracking().ToListAsync();

			return PartialView("UsersList", users);
		}

		public IActionResult UsersWithMostLikesView()
		{
			List<UserModel> users = _Tendency.UsersWithMostLikes;

			return PartialView("UsersList", users);
		}
		public IActionResult UsersWithMostPostsView()
		{
			List<UserModel> users = _Tendency.UsersWithMostPost;

			return PartialView("UsersList", users);
		}
	}
}
