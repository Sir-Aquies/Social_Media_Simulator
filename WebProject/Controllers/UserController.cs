#nullable disable
using Microsoft.AspNetCore.Mvc;
using WebProject.Models;
using WebProject.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using WebProject.Services;
using Newtonsoft.Json;

namespace WebProject.Controllers
{
	[Authorize]
	public class UserController : Controller
	{
		private readonly WebProjectContext _Models;
		private readonly ILogger<UserController> _Logger;
		private readonly UserManager<UserModel> _UserManager;
		private readonly ITendency _Tendency;
		private readonly ModelLogic _Logic;

		private readonly int inicialAmountPostsToLoad = 10;
		private readonly int showCommentsPerPost = 3;
		private const int AmountPostsPerLoad = 5;

		public UserController(WebProjectContext models, UserManager<UserModel> userManager, 
			ILogger<UserController> logger, ITendency tendency, ModelLogic modelLogic)
		{
			_Models = models;
			_UserManager = userManager;
			_Logger = logger;
			_Tendency = tendency;
			_Logic = modelLogic;
		}

		//var mostLikedUsers = _Models.Posts
		//	.GroupBy(p => p.UserId)
		//	.Select(g => new { UserId = g.Key, Total = g.Sum(p => p.Likes) })
		//	.OrderByDescending(x => x.Total)
		//	.Take(10)
		//	.ToList();

		//Eliot09  2e8e3796-7eb0-4bce-8201-df95004105d1
		//Loads all the following user's posts of the loggedUser. 
		public async Task<IActionResult> Home()
		{
			UserModel loggedUser = await _UserManager.GetUserAsync(HttpContext.User);

			if (loggedUser == null)
				return RedirectToAction("Login", "Account");

			loggedUser.Posts = await GetFollowingUsersPosts(loggedUser.Id, 0, inicialAmountPostsToLoad);

			//Use to pass the variables to a javascript methods (SetScrollEvent and SwitchTo...).
			ViewData["startFromPost"] = inicialAmountPostsToLoad;
			ViewData["PostsPerLoad"] = AmountPostsPerLoad;
			ViewData["profileTabName"] = "#following-posts";

			loggedUser.Followers = await _Logic.GetFollowers(loggedUser.Id);
			loggedUser.Following = await _Logic.GetFollowingUsers(loggedUser.Id);

			await LoadPageUserStats(loggedUser.Id);

			return View(loggedUser);
		}

		private async Task<List<PostModel>> GetFollowingUsersPosts(string userId, int startFromRow, int amountOfRows)
		{
			List<PostModel> posts = await _Models.Posts
				.FromSqlRaw("SELECT * FROM Posts WHERE UserId IN " +
				"(SELECT CreatorId FROM Followers WHERE FollowerId = {0}) " +
				"ORDER BY Date DESC OFFSET {1} ROWS FETCH NEXT {2} ROWS ONLY;", userId, startFromRow, amountOfRows)
				.AsNoTracking().ToListAsync();

			posts = await _Logic.FillPostsProperties(posts);

			return posts;
		}

		public async Task<IActionResult> LoadMoreFollowingUsersPosts(int startFromRow, int amountOfRows = AmountPostsPerLoad)
		{
			UserModel loggedUser = await _UserManager.GetUserAsync(HttpContext.User);

			if (loggedUser == null)
				return NotFound("User not found.");

			List<PostModel> posts = await GetFollowingUsersPosts(loggedUser.Id, startFromRow, amountOfRows);

			if (posts == null)
				return NotFound("Posts not found.");

			//If no posts are return from the database.
			//NoContent stops future calls.
			if (posts.Count == 0)
				return NoContent();

			await LoadPostListInfo(loggedUser);

			return PartialView("PostList", posts);
		}

		//PageUser profile page that shows all his/her posts.
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
				return View("Profile", pageUser);
			}

			//Load the first posts with the comments from the database, from 0 to inicialAmountPostsToLoad.
			pageUser.Posts = await GetPosts(pageUser, 0, inicialAmountPostsToLoad);

			//Use to pass the variables to a javascript methods (SetScrollEvent and SwitchTo...).
			ViewData["startFromPost"] = inicialAmountPostsToLoad;
			ViewData["PostsPerLoad"] = AmountPostsPerLoad;
			ViewData["profileTabName"] = "#pageuser-posts";

			pageUser.Followers = await _Logic.GetFollowers(pageUser.Id);
			pageUser.Following = await _Logic.GetFollowingUsers(pageUser.Id);

			await LoadPageUserStats(pageUser.Id);
			await LoadPostListInfo(loggedUser);

			return View("Profile", pageUser);
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
				return View("Profile", pageUser);
			}

			//Load the first posts with the comments from the database, from 0 to inicialAmountPostsToLoad.
			pageUser.Posts = await GetPosts(pageUser, 0, inicialAmountPostsToLoad, true);
			//Use to pass the variables to a javascript methods (SetScrollEvent and SwitchTo...).
			ViewData["startFromPost"] = inicialAmountPostsToLoad;
			ViewData["PostsPerLoad"] = AmountPostsPerLoad;
			ViewData["profileTabName"] = "#pageuser-media";

			pageUser.Followers = await _Logic.GetFollowers(pageUser.Id);
			pageUser.Following = await _Logic.GetFollowingUsers(pageUser.Id);

			await LoadPageUserStats(pageUser.Id);
			await LoadPostListInfo(loggedUser);

			return View("Profile", pageUser);
		}

		//Get posts from the database using a range and can also fetch the posts where media is not null.
		private async Task<List<PostModel>> GetPosts(UserModel user, int startFromRow, int amountOfRows, bool onlyMedia = false)
		{
			//The only different is the "AND Media IS NOT NULL" on the first string.
			string sql = onlyMedia ? 
				"SELECT * FROM Posts WHERE UserId = {0} AND Media IS NOT NULL ORDER BY Date DESC OFFSET {1} ROWS FETCH NEXT {2} ROWS ONLY;"
				: "SELECT * FROM Posts WHERE UserId = {0} ORDER BY Date DESC OFFSET {1} ROWS FETCH NEXT {2} ROWS ONLY;";

			List<PostModel> posts = await _Models.Posts
				.FromSqlRaw(sql, user.Id, startFromRow, amountOfRows)
				.AsNoTracking().ToListAsync();

			posts = await _Logic.FillPostsProperties(posts, user);

			return posts;
		}

		//Use to fetch more posts in UserPage as the user scrolls down.
		public async Task<IActionResult> LoadMorePosts(string userId, int startFromRow, int amountOfRows = AmountPostsPerLoad, bool onlyMedia = false)
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
				return View("Profile", pageUser);
			}

			//Load the first posts with the comments from the databas (from 0 to inicialAmountPostsToLoad).
			pageUser.Posts = await GetLikedPost(pageUser.Id, 0, inicialAmountPostsToLoad);
			//Use to pass the variables to a javascript methods (SetScrollEvent and SwitchTo...).
			ViewData["startFromPost"] = inicialAmountPostsToLoad;
			ViewData["PostsPerLoad"] = AmountPostsPerLoad;
			ViewData["profileTabName"] = "#pageuser-likes";

			pageUser.Followers = await _Logic.GetFollowers(pageUser.Id);
			pageUser.Following = await _Logic.GetFollowingUsers(pageUser.Id);

			await LoadPageUserStats(pageUser.Id);
			await LoadPostListInfo(loggedUser);

			return View("Profile", pageUser);
		}

		private async Task<List<PostModel>> GetLikedPost(string userId, int startFromRow, int amountOfRows)
		{
			List<PostModel> posts = await _Models.Posts
				.FromSqlRaw("SELECT * FROM Posts WHERE Id IN " +
				"(SELECT PostId FROM PostLikes WHERE UserId = {0} " +
				"ORDER BY LikedDate DESC OFFSET {1} ROWS FETCH NEXT {2} ROWS ONLY)", userId, startFromRow, amountOfRows)
				.AsNoTracking().ToListAsync();

			posts = await _Logic.FillPostsProperties(posts);

			return posts;
		}

		public async Task<IActionResult> LoadMoreLikedPosts(string userId, int startFromRow, int amountOfRows = AmountPostsPerLoad)
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
				comment.UserLikes = await _Logic.GetCommentLikesSelective(comment.Id);
				comment.User = await _Models.Users
				.Select(u => new UserModel { Id = u.Id, UserName = u.UserName, ProfilePicture = u.ProfilePicture })
				.Where(u => u.Id == comment.UserId).AsNoTracking().FirstOrDefaultAsync();

				comment.Post = await _Models.Posts.FromSqlRaw("SELECT * FROM Posts WHERE Id = {0}", comment.PostId).AsNoTracking().FirstOrDefaultAsync();
				comment.Post.User = await _Models.Users
					.Select(u => new UserModel { Id = u.Id, UserName = u.UserName, ProfilePicture = u.ProfilePicture })
					.Where(u => u.Id == comment.Post.UserId).AsNoTracking().FirstOrDefaultAsync();
				comment.Post.UserLikes = await _Logic.GetPostLikesSelective(comment.Post.Id);
				comment.Post.Comments = new List<CommentModel>() { comment };

				posts.Add(comment.Post);
			}

			return posts;
		}

		public async Task<IActionResult> LoadMoreLikedComments(string userId, int startFromRow, int amountOfRows = AmountPostsPerLoad)
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
				return View("Profile", pageUser);
			}

			//Load the first posts with the comments from the database, from 0 to inicialAmountPostsToLoad.
			pageUser.Posts = await GetCommentedPosts(pageUser, 0, inicialAmountPostsToLoad);

			//Use to pass the variables to a javascript methods (SetScrollEvent and SwitchTo...).
			ViewData["startFromPost"] = inicialAmountPostsToLoad;
			ViewData["PostsPerLoad"] = AmountPostsPerLoad;
			ViewData["profileTabName"] = "#pageuser-comments";

			pageUser.Followers = await _Logic.GetFollowers(pageUser.Id);
			pageUser.Following = await _Logic.GetFollowingUsers(pageUser.Id);

			await LoadPageUserStats(pageUser.Id);
			await LoadPostListInfo(loggedUser);

			return View("Profile", pageUser);
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
				comment.UserLikes = await _Logic.GetCommentLikesSelective(comment.Id);
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
					c.UserLikes = await _Logic.GetCommentLikesSelective(c.Id);
					c.User = user;
					//In the case that the comment was already fetch in comments.
					comments.Remove(c);
				}

				post.Comments = samePostComments;

				post.Comments.Add(comment);

				post.UserLikes = await _Logic.GetPostLikesSelective(post.Id);

				post.User = await _Models.Users
						.Select(u => new UserModel { Id = u.Id, UserName = u.UserName, ProfilePicture = u.ProfilePicture })
						.Where(u => u.Id == post.UserId).AsNoTracking().FirstOrDefaultAsync();

				posts.Add(post);
			}

			return posts;
		}

		public async Task<IActionResult> LoadMoreCommentedPosts(string userId, int startFromRow, int amountOfRows = AmountPostsPerLoad)
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
			if (string.IsNullOrEmpty(userId))
				return;

			string[] queries = {
				//Total likes the page user has received in his/her posts.
				"SELECT SUM(Likes) FROM Posts WHERE UserId = {0}",
				//Number of posts the page user has made.
				"SELECT COUNT(Id) FROM Posts WHERE UserId = {0}",
				//Number of comments the page user has made.
				"SELECT COUNT(Id) FROM Comments WHERE UserId = {0}"
			};

			int[] counts = new int[queries.Length];

			for (int i = 0; i < queries.Length; i++)
			{
				//The try catch are in case the query retruns null when there are no records.
				try
				{
					List<int> total = await _Models.Database
					.SqlQueryRaw<int>(queries[i], userId).ToListAsync();
					counts[i] = total.FirstOrDefault();
				}
				catch{ }
			}

			TempData["TotalLikes"] = counts[0];
			TempData["TotalPosts"] = counts[1];
			TempData["TotalComments"] = counts[2];

			////Total likes the page user has received in his/her posts.
			//try
			//{
			//	List<int> totalLikes = await _Models.Database
			//	.SqlQueryRaw<int>("SELECT SUM(Likes) FROM Posts WHERE UserId = {0}", userId).ToListAsync();
			//	TempData["TotalLikes"] = totalLikes.First();

			//}
			//catch
			//{
			//	TempData["TotalLikes"] = 0;
			//}


			////Number of posts the page user has made.
			//try
			//{
			//	List<int> totalPosts = await _Models.Database
			//	.SqlQueryRaw<int>("SELECT COUNT(Id) FROM Posts WHERE UserId = {0}", userId).ToListAsync();
			//	TempData["TotalPosts"] = totalPosts.First();
			//}
			//catch
			//{
			//	TempData["TotalPosts"] = 0;
			//}


			////Number of comments the page user has made.
			//try
			//{
			//	List<int> totalComments = await _Models.Database
			//	.SqlQueryRaw<int>("SELECT COUNT(Id) FROM Comments WHERE UserId = {0}", userId).ToListAsync();
			//	TempData["TotalComments"] = totalComments.First();
			//}
			//catch
			//{
			//	TempData["TotalComments"] = 0;
			//}
		}

		public async Task<string> Follow(string userId)
		{
			if (string.IsNullOrEmpty(userId))
				return "0";

			UserModel loggedUser = await _UserManager.GetUserAsync(HttpContext.User);

			if (loggedUser.Id == userId)
				return "0";

			List<string> creatorId = await _Models.Database
				.SqlQueryRaw<string>("SELECT CreatorId FROM Followers WHERE CreatorId = {0} AND FollowerId = {1}", userId, loggedUser.Id)
				.AsNoTracking().ToListAsync();

			//Row exist that means the user wants to unfollow.
			if (creatorId.Count == 1)
			{
				int result = await _Models.Database
					.ExecuteSqlRawAsync("DELETE FROM Followers WHERE CreatorId = {0} AND FollowerId = {1}", creatorId[0], loggedUser.Id);

				return result == 1 ? "-" : "0";
			}
			else
			{
				//If row doesn't exist the user wants to follow.
				List<string> userIdExist = await _Models.Database.SqlQueryRaw<string>("SELECT Id FROM Users WHERE Id = {0}", userId).AsNoTracking().ToListAsync();

				if (userIdExist.Count == 1)
				{
					int result = await _Models.Database
					.ExecuteSqlRawAsync("INSERT INTO Followers (CreatorId, FollowerId, FollowedDate) VALUES ({0}, {1}, {2})", userIdExist[0], loggedUser.Id, DateTime.Now);

					return result == 1 ? "+" : "0";
				}
				else
					return "0";
			}
		}

		public async Task<IActionResult> FollowersUsersTab(string userId)
		{
			List<UserModel> followers = new();
			try
			{
				followers = await _Models.Users
				.FromSqlRaw("SELECT * FROM Users WHERE Id IN(SELECT FollowerId FROM Followers WHERE CreatorId = {0} ORDER BY FollowedDate DESC OFFSET 0 ROWS);", userId)
				.AsNoTracking().ToListAsync();
			}
			catch
			{
				return NotFound("Followers could not be loaded from the database.");
			}

			if (followers.Count == 0)
				return NoContent();

			return PartialView("UsersList", followers);
		}
		
		public async Task<IActionResult> FollowingUsersTab(string userId)
		{
			List<UserModel> followingUsers = new();
			try
			{
				followingUsers = await _Models.Users
				.FromSqlRaw("SELECT * FROM Users WHERE Id IN(SELECT CreatorId FROM Followers WHERE FollowerId = {0} ORDER BY FollowedDate DESC OFFSET 0 ROWS);", userId)
				.AsNoTracking().ToListAsync();
			}
			catch
			{
				return NotFound("Following users could not be loaded from the database.");
			}

			if (followingUsers.Count == 0)
				return NoContent();

			return PartialView("UsersList", followingUsers);
		}

		public string UpdateUserStats(string userId)
		{
			string[] sqlQueries = { 
				"SELECT SUM(Likes) FROM Posts WHERE UserId = {0}",
				"SELECT COUNT(Id) FROM Posts WHERE UserId = {0}",
				"SELECT COUNT(Id) FROM Comments WHERE UserId = {0}",
				"SELECT COUNT(FollowerId) FROM Followers WHERE CreatorId = {0}",
				"SELECT COUNT(CreatorId) FROM Followers WHERE FollowerId = {0}"
			};

			int[] stats = new int[sqlQueries.Length];

			for (int i = 0; i < sqlQueries.Length; i++)
			{
				//The try catch is in case it returns null which throws an exception.
				try
				{
					stats[i] = _Models.Database.SqlQueryRaw<int>(sqlQueries[i], userId).ToListAsync().Result[0];
				}
				catch { }
			}

			return JsonConvert.SerializeObject(new { 
				totalLikes = stats[0], totalPosts = stats[1], 
				totalComments = stats[2], followersCount = stats[3], 
				followingCount = stats[4] });
		}

		public async Task<IActionResult> SearchUsers()
		{
			UserModel loggedUser = await _UserManager.GetUserAsync(HttpContext.User);

			return View(loggedUser);
		}

		public async Task<IActionResult> LookUsersByUserName(string userName)
		{
			List<UserModel> users = await _Models.Users
				.FromSqlRaw("SELECT * FROM Users WHERE UserName LIKE {0};", userName + "%")
				.AsNoTracking().ToListAsync();

			return PartialView("UsersList", users);
		}

		public async Task<IActionResult> GetRandomUsersView(int amountOfUsers)
		{
			List<UserModel> users = await _Models.Users.FromSqlRaw($"SELECT TOP { amountOfUsers } * FROM Users ORDER BY NEWID();")
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
		
		public IActionResult UsersWithMostFollowersView()
		{
			return PartialView("UsersList", _Tendency.UsersWithMostFollowers);
		}
	}
}
