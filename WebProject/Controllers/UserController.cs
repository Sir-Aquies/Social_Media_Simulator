#nullable disable
using Microsoft.AspNetCore.Mvc;
using WebProject.Models;
using WebProject.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using WebProject.Services;
using Microsoft.Extensions.Hosting;

namespace WebProject.Controllers
{
	[Authorize]
	public class UserController : Controller
	{
		private readonly WebProjectContext _Models;
		private readonly ILogger<UserController> _Logger;
		private readonly UserManager<UserModel> _UserManager;
		private readonly ITendency _Tendency;

		private readonly int inicialAmountPostsToLoad = 10;
		private readonly int showCommentsPerPost = 3;

		public UserController(WebProjectContext models, UserManager<UserModel> userManager, ILogger<UserController> logger, ITendency tendency)
		{
			_Models = models;
			_UserManager = userManager;
			_Logger = logger;
			_Tendency = tendency;
		}

		//TODO - With the likes algorithm then create a tendency page with the most likes and commented post
		//Eliot09   9a0a92b8-2c82-4199-9972-d1731a300f0c
		//lazycat381   fbf54244-51e6-4f57-badb-c75ff5742cb4
		public async Task<IActionResult> SearchUser(string userName)
		{
			UserModel loggedUser = await _UserManager.GetUserAsync(HttpContext.User);

			return !string.IsNullOrEmpty(userName) ? 
				RedirectToAction("UserPage", new { userName }) : 
				RedirectToAction("UserPage", new { loggedUser.UserName });
		}

		//TODO - delete DynamicUser.
		//"Home page" that shows all the posts of a user (pageUser).
		public async Task<IActionResult> UserPage(string userName)
		{
			UserModel loggedUser = await _UserManager.GetUserAsync(HttpContext.User);

			if (loggedUser == null) 
				return RedirectToAction("Login", "Account");

			UserModel pageUser = (!string.IsNullOrEmpty(userName) && userName != loggedUser.UserName)
									? await _UserManager.FindByNameAsync(userName) : loggedUser;

			if (pageUser == null)
			{
				ViewBag.unexistingUserName = userName;
				return View(new DynamicUser { User = loggedUser, PageUser = pageUser });
			}

			//Load the first posts with the comments from the database, from 0 to inicialAmountPostsToLoad.
			pageUser.Posts = await GetPosts(pageUser, 0, inicialAmountPostsToLoad);
			//Use to pass the number to a javascript method.
			ViewData["startFromPost"] = inicialAmountPostsToLoad.ToString();

			await LoadPageUserStats(pageUser.Id);

			return View(new DynamicUser { User = loggedUser, PageUser = pageUser });
		}

		//Page that shows all the posts with media (images).
		public async Task<IActionResult> MediaPosts(string userName)
		{
			UserModel loggedUser = await _UserManager.GetUserAsync(HttpContext.User);

			if (loggedUser == null) 
				return RedirectToAction("Login", "Account");

			UserModel pageUser = (!string.IsNullOrEmpty(userName) && userName != loggedUser.UserName) 
									? await _UserManager.FindByNameAsync(userName) : loggedUser;

			if (pageUser == null)
			{
				ViewBag.unexistingUserName = userName;
				return View(new DynamicUser { User = loggedUser, PageUser = pageUser });
			}

			//Load the first posts with the comments from the database, from 0 to inicialAmountPostsToLoad.
			pageUser.Posts = await GetPosts(pageUser, 0, inicialAmountPostsToLoad, true);
			//Use to pass the number to a javascript method.
			ViewData["startFromPost"] = inicialAmountPostsToLoad.ToString();

			await LoadPageUserStats(pageUser.Id);

			return View(new DynamicUser { User = loggedUser, PageUser = pageUser });
		}

		//Get posts from the database using a range and can also fetch the posts where media is not null.
		private async Task<List<PostModel>> GetPosts(UserModel user, int startFromRow, int amountOfRows, bool onlyMedia = false)
		{
			//The only different is the "AND Media IS NOT NULL" on the first string.
			string sql = onlyMedia ? 
				"SELECT * FROM Posts WHERE UserId = {0} AND Media IS NOT NULL ORDER BY PostDate DESC OFFSET {1} ROWS FETCH NEXT {2} ROWS ONLY;"
				: "SELECT * FROM Posts WHERE UserId = {0} ORDER BY PostDate DESC OFFSET {1} ROWS FETCH NEXT {2} ROWS ONLY;";

			List<PostModel> posts = await _Models.Posts
				.FromSqlRaw(sql, user.Id, startFromRow, amountOfRows)
				.AsNoTracking().ToListAsync();

			posts = await FillPostsProperties(posts, user);

			return posts;
		}

		//Use to fetch more posts in UserPage as the user scrolls down.
		public async Task<IActionResult> LoadMorePosts(string userId, int startFromRow, int amountOfRows, bool onlyMedia = false)
		{
			UserModel pageUser = await _UserManager.FindByIdAsync(userId);

			if (pageUser == null)
				return NotFound("User not found.");

			List<PostModel> posts = await GetPosts(pageUser, startFromRow, amountOfRows, onlyMedia);

			if (posts == null)
				return NotFound("Posts not found.");

			//If no posts are return from the database.
			//NoContent stops future calls.

			if (posts.Count == 0)
				return NoContent();

			await LoadPostListInfo();

			return PartialView("PostList", posts);
		}

		//Page to show one post with all its comments.
		public async Task<IActionResult> CompletePost(string userName, int postId)
		{
			UserModel loggedUser = await _UserManager.GetUserAsync(HttpContext.User);

			if (loggedUser == null) 
				return RedirectToAction("Login", "Account");

			UserModel pageUser = (!string.IsNullOrEmpty(userName) && userName != loggedUser.UserName)
									? await _UserManager.FindByNameAsync(userName) : loggedUser;

			if (pageUser == null)
			{
				ViewBag.unexistingUserName = userName;
				return View("UserPage", new DynamicUser { User = loggedUser, PageUser = pageUser });
			}

			PostModel post = await _Models.Posts.FromSqlRaw("SELECT * FROM Posts WHERE Id = {0} AND UserId = {1}", postId, pageUser.Id).AsNoTracking().FirstOrDefaultAsync();

			if (post != null)
			{
				post.UsersLikes = await GetPostLikesSelective(post.Id);
				post.User = pageUser;
				post.Comments = await LoadComments(post);
			}

			pageUser.Posts = new List<PostModel> { post };

			await LoadPageUserStats(pageUser.Id);

			return View(new DynamicUser { User = loggedUser, PageUser = pageUser });
		}

		//Loads and shows all the posts the page user has liked.
		public async Task<IActionResult> LikedPostsAndComments(string userName)
		{
			UserModel loggedUser = await _UserManager.GetUserAsync(HttpContext.User);

			if (loggedUser == null) 
				return RedirectToAction("Login", "Account");

			UserModel pageUser = (!string.IsNullOrEmpty(userName) && userName != loggedUser.UserName)
				? await _UserManager.FindByNameAsync(userName) : loggedUser; 

			if (pageUser == null)
			{
				ViewBag.unexistingUserName = userName;
				return View("UserPage", new DynamicUser { User = loggedUser, PageUser = pageUser });
			}

			//Load the first posts with the comments from the databas (from 0 to inicialAmountPostsToLoad).
			pageUser.LikedPost = await GetLikedPost(pageUser.Id, 0, inicialAmountPostsToLoad);
			//Use to pass the number to a javascript method.
			ViewData["startFromPost"] = inicialAmountPostsToLoad;
			ViewData["inicialAmountPostsToLoad"] = inicialAmountPostsToLoad;

			await LoadPageUserStats(pageUser.Id);

			return View(new DynamicUser { User = loggedUser, PageUser = pageUser});
		}

		private async Task<List<PostModel>> GetLikedPost(string userId, int startFromRow, int amountOfRows)
		{
			List<PostModel> posts = await _Models.Posts
				.FromSqlRaw("SELECT * FROM Posts WHERE Id IN " +
				"(SELECT PostId FROM PostLikes WHERE UserId = {0}) " +
				"ORDER BY Id DESC OFFSET {1} ROWS FETCH NEXT {2} ROWS ONLY", userId, startFromRow, amountOfRows)
				.AsNoTracking().ToListAsync();

			posts = await FillPostsProperties(posts);

			return posts;
		}

		public async Task<IActionResult> LoadMorePostsLikes(string userId, int startFromRow, int amountOfRows)
		{
			List<PostModel> posts = await GetLikedPost(userId, startFromRow, amountOfRows);

			if (posts == null)
				return NotFound("Posts not found.");

			if (posts.Count == 0)
				return NoContent();

			await LoadPostListInfo();

			return PartialView("PostList", posts);
		}

		private async Task<List<PostModel>> GetLikedCommentsPost(string userId, int startFromRow, int amountOfRows)
		{
			List<PostModel> posts = new();

			List<CommentModel> comments = await _Models.Comments
				.FromSqlRaw("SELECT * FROM Comments WHERE Id IN " +
				"(SELECT CommentId FROM CommentLikes WHERE UserId = {0}) " +
				"ORDER BY Id DESC OFFSET {1} ROWS FETCH NEXT {2} ROWS ONLY;", userId, startFromRow, amountOfRows)
				.AsNoTracking().ToListAsync();

			foreach (CommentModel comment in comments)
			{
				comment.UsersLikes = await GetCommentLikesSelective(comment.Id);
				comment.User = await _Models.Users
				.Select(u => new UserModel { Id = u.Id, UserName = u.UserName, ProfilePicture = u.ProfilePicture })
				.Where(u => u.Id == comment.UserId).AsNoTracking().FirstOrDefaultAsync();

				comment.Post = await _Models.Posts.FromSqlRaw("SELECT * FROM Posts WHERE Id = {0}", comment.PostId).AsNoTracking().FirstOrDefaultAsync();
				comment.Post.User = await _Models.Users
					.Select(u => new UserModel { Id = u.Id, UserName = u.UserName, ProfilePicture = u.ProfilePicture })
					.Where(u => u.Id == comment.Post.UserId).AsNoTracking().FirstOrDefaultAsync();
				comment.Post.UsersLikes = await GetPostLikesSelective(comment.Post.Id);
				comment.Post.Comments = new List<CommentModel>() { comment };

				posts.Add(comment.Post);
			}

			return posts;
		}

		public async Task<IActionResult> LoadMoreLikedComments(string userId, int startFromRow, int amountOfRows)
		{
			List<PostModel> posts = await GetLikedCommentsPost(userId, startFromRow, amountOfRows);

			if (posts == null)
				return NotFound("Posts not found.");

			if (posts.Count == 0)
			{
				return NoContent();
			}

			await LoadPostListInfo();

			return PartialView("PostList", posts);
		}

		//Loads and shows all the posts the page user has commented.
		public async Task<IActionResult> CommentedPosts(string userName)
		{
			UserModel loggedUser = await _UserManager.GetUserAsync(HttpContext.User);

			if (loggedUser == null) 
				return RedirectToAction("Login", "Account");

			UserModel pageUser = (!string.IsNullOrEmpty(userName) && userName != loggedUser.UserName)
				? await _UserManager.FindByNameAsync(userName) : loggedUser;

			if (pageUser == null)
			{
				ViewBag.unexistingUserName = userName;
				return View("UserPage", new DynamicUser { User = loggedUser, PageUser = pageUser });
			}

			//Load the first posts with the comments from the database, from 0 to inicialAmountPostsToLoad.
			pageUser.Posts = await GetCommentedPosts(pageUser, 0, inicialAmountPostsToLoad);
			//Use to pass the number to a javascript method.
			ViewData["startFromPost"] = inicialAmountPostsToLoad.ToString();

			await LoadPageUserStats(pageUser.Id);

			return View(new DynamicUser { User = loggedUser, PageUser = pageUser });
		}

		private async Task<List<PostModel>> GetCommentedPosts(UserModel user, int startFromRow, int amountOfRows)
		{
			List<PostModel> posts = new();
			List<CommentModel> comments = await _Models.Comments
				.FromSqlRaw("SELECT * FROM Comments WHERE UserId = {0} " +
				"ORDER BY Date DESC OFFSET {1} ROWS FETCH NEXT {2} ROWS ONLY;", user.Id, startFromRow, amountOfRows)
				.AsNoTracking().ToListAsync();

			foreach (CommentModel comment in comments)
			{
				comment.UsersLikes = await GetCommentLikesSelective(comment.Id);
				comment.User = user;

				PostModel post = await _Models.Posts
						.FromSqlRaw("SELECT * FROM Posts WHERE Id = {0}", comment.PostId)
						.AsNoTracking().FirstOrDefaultAsync();

				//Fetch the comments who belong to the same post (the older comments).
				List<CommentModel> samePostComments = await _Models.Comments
					.FromSqlRaw("SELECT * FROM Comments WHERE PostId = {0} AND UserId = {1} AND Date < {2}", post.Id, user.Id, comment.Date)
					.AsNoTracking().ToListAsync();

				//Look for newer comments from the same post.
				List<CommentModel> alreadyAddedComments = await _Models.Comments
					.FromSqlRaw("SELECT * FROM Comments WHERE PostId = {0} AND UserId = {1} AND Date > {2}", post.Id, user.Id, comment.Date)
					.AsNoTracking().ToListAsync();

				//If there is any post that means the comment was already loaded.
				if (alreadyAddedComments.Count > 0) 
					continue;

				foreach (CommentModel c in samePostComments)
				{
					c.UsersLikes = await GetCommentLikesSelective(c.Id);
					c.User = user;
					//In the case that the comment was already fetch in comments.
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

		public async Task<IActionResult> LoadMorePostsComments(string userId, int startFromRow, int amountOfRows)
		{
			UserModel pageUser = await _UserManager.FindByIdAsync(userId);

			List<PostModel> posts = await GetCommentedPosts(pageUser, startFromRow, amountOfRows);

			if (posts == null)
				return NotFound("Posts not found.");

			if (posts.Count == 0)
				return NoContent();

			await LoadPostListInfo();

			return PartialView("PostList", posts);
		}

		//Loads the comments with its likes and user for a single post.
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

		//Gets the ids of the users who has liked a certain post.
		private async Task<List<UserModel>> GetPostLikesSelective(int postId)
		{
			List<UserModel> users = new();

			string[] userIds = await _Models.Database
				.SqlQueryRaw<string>("SELECT UserId FROM PostLikes WHERE PostId = {0}", postId)
				.AsNoTracking().ToArrayAsync();

			for (int i = 0; i < userIds.Length; i++)
			{
				users.Add(new UserModel { Id = userIds[i] });
			}

			return users;
		}

		//Gets the ids of the users who has liked a certain comment.
		private async Task<List<UserModel>> GetCommentLikesSelective(int commentId)
		{
			List<UserModel> users = new();

			string[] userIds = await _Models.Database
				.SqlQueryRaw<string>("Select UserId from CommentLikes where CommentId = {0}", commentId)
				.AsNoTracking().ToArrayAsync();

			for (int i = 0; i < userIds.Length; i++)
			{
				users.Add(new UserModel { Id = userIds[i] });
			}

			return users;
		}

		public async Task<List<PostModel>> OldFillPostsProperties(List<PostModel> posts)
		{
			foreach (PostModel post in posts)
			{
				post.Comments = await LoadComments(post);
				post.UsersLikes = await GetPostLikesSelective(post.Id);
				post.User = await _Models.Users
					.Select(u => new UserModel { Id = u.Id, UserName = u.UserName, ProfilePicture = u.ProfilePicture })
					.Where(u => u.Id == post.UserId).AsNoTracking().FirstOrDefaultAsync();
			}

			return posts;
		}

		public async Task<List<PostModel>> FillPostsProperties(List<PostModel> posts, UserModel postOwner = null)
		{
			List<UserModel> users = new();

			if (postOwner == null)
			{
				List<string> userIds = new();

				for (int i = 0; i < posts.Count; i++)
				{
					userIds.Add($"'{posts[i].UserId}'");
				}

				users = await _Models.Users
				.FromSqlRaw(string.Format("SELECT * FROM AspNetUsers WHERE Id IN({0});", string.Join(", ", userIds)))
				.AsNoTracking().ToListAsync();
			}

			foreach (PostModel post in posts)
			{
				post.UsersLikes = await GetPostLikesSelective(post.Id);

				if (postOwner == null)
				{
					for (int i = 0; i < users.Count; i++)
					{
						if (post.UserId == users[i].Id)
						{
							post.User = users[i];
							break;
						}
					}
				}
				else
				{
					post.User = postOwner;
				}
			}

			posts = await NewLoadComments(posts);

			return posts;
		}

		private async Task<List<PostModel>> NewLoadComments(List<PostModel> posts)
		{
			List<int> postIds = new();

			for (int i = 0; i < posts.Count; i++)
			{
				postIds.Add(posts[i].Id);
			}

			List<CommentModel> comments = await _Models.Comments
				.FromSqlRaw(string.Format("SELECT * FROM Comments WHERE PostId IN ({0})", string.Join(", ", postIds)))
				.AsNoTracking().ToListAsync();

			List<string> userIds = new();

			for (int i = 0; i < comments.Count; i++)
			{
				userIds.Add($"'{comments[i].UserId}'");
			}

			List<UserModel> users = await _Models.Users
				.FromSqlRaw(string.Format("SELECT * FROM AspNetUsers WHERE Id IN({0});", string.Join(", ", userIds)))
				.AsNoTracking().ToListAsync();

			foreach (CommentModel comment in comments)
			{
				comment.UsersLikes = await GetCommentLikesSelective(comment.Id);
				for (int i = 0; i < users.Count; i++)
				{
					if (comment.UserId == users[i].Id)
					{
						comment.User = users[i];
						break;
					}
				}
			}

			foreach (PostModel post in posts)
			{
				post.Comments = new List<CommentModel>();

				foreach (CommentModel comment in comments)
				{
					if (post.Id == comment.PostId)
					{
						post.Comments.Add(comment);
					}
				}
			}

			return posts;
		}

		public async Task LoadPostListInfo(UserModel loggedUser = null)
		{
			loggedUser ??= await _UserManager.GetUserAsync(HttpContext.User);

			//Lets the partial view know if the user liked a post or comment.
			ViewData["LoggedUserId"] = loggedUser.Id;
			//Lets the partial view know how many comments per post needs to show,
			//if no value is pass it shows all.
			ViewData["commentsAmount"] = showCommentsPerPost;
			//Lets the partial view know if it needs to blur the images.
			ViewBag.blur = loggedUser.ShowImages ? "1" : "0";
		}

		private async Task LoadPageUserStats(string userId)
		{
			//Total likes the page user has received in his/her posts.
			List<int> totalLikes = await _Models.Database
				.SqlQueryRaw<int>("SELECT SUM(Likes) FROM Posts WHERE UserId = {0}", userId).ToListAsync();
			TempData["TotalLikes"] = totalLikes.First();

			//Number of posts the page user has made.
			List<int> totalPosts = await _Models.Database
				.SqlQueryRaw<int>("SELECT COUNT(Id) FROM Posts WHERE UserId = {0}", userId).ToListAsync();
			TempData["TotalPosts"] = totalPosts.First();

			//Number of comments the page user has made.
			List<int> totalComments = await _Models.Database
				.SqlQueryRaw<int>("SELECT COUNT(Id) FROM Comments WHERE UserId = {0}", userId).ToListAsync();
			TempData["TotalComments"] = totalComments.First();
		}

		public async Task<IActionResult> SearchUsers()
		{
			UserModel loggedUser = await _UserManager.GetUserAsync(HttpContext.User);

			return View(loggedUser);
		}

		public async Task<IActionResult> LookUsersByUserName(string userName)
		{
			List<UserModel> users = await _Models.Users
				.FromSqlRaw("SELECT * FROM AspNetUsers WHERE UserName LIKE {0};", userName + "%")
				.AsNoTracking().ToListAsync();

			return PartialView("UsersList", users);
		}

		public async Task<IActionResult> GetRandomUsersView(int amountOfUsers)
		{
			List<UserModel> users = await _Models.Users.FromSqlRaw($"SELECT TOP { amountOfUsers } * FROM AspNetUsers ORDER BY NEWID();")
				.AsNoTracking().ToListAsync();

			return PartialView("UsersList", users);
		}

		public IActionResult UsersWithMostLikesView()
		{
			return PartialView("UsersList", _Tendency.UsersWithMostLikes);
		}

		public IActionResult UsersWithMostPostsView()
		{
			return PartialView("UsersList", _Tendency.UsersWithMostPost);
		}
	}
}
