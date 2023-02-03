using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebProject.Data;
using WebProject.Models;
using WebProject.Services;
#nullable disable

namespace WebProject.Controllers
{
	public class ExploreController : Controller
	{
		private readonly WebProjectContext _Models;
		private readonly UserManager<UserModel> _UserManager;
		private readonly ITendency _Tendency;

		private const int AmountPostsToLoad = 5;
		private const string sessionRetrievedPostsIds = "_retrievedPostsIds";
		private const int inicialAmountPostsToLoad = 10;
		private const int showCommentsPerPost = 3;

		public ExploreController(WebProjectContext models, UserManager<UserModel> userManager, ITendency tendency)
		{
			_Models = models;
			_UserManager = userManager;
			_Tendency = tendency;
		}

		public async Task<IActionResult> Index()
		{
			UserModel loggedUser = await _UserManager.GetUserAsync(HttpContext.User);

			loggedUser.Posts = await RetrievedRandomPosts(inicialAmountPostsToLoad, "");

			List<int> postsIds = new();

			for (int i = 0; i < loggedUser.Posts.Count; i++)
			{
				postsIds.Add(loggedUser.Posts[i].Id);
			}

			//Save the ids of the post in a session variable.
			string newIds = string.Join(", ", postsIds);
			HttpContext.Session.SetString(sessionRetrievedPostsIds, newIds);
			ViewData["inicialAmountPostsToLoad"] = inicialAmountPostsToLoad;

			return View(loggedUser);
		}

		public async Task<IActionResult> LoadMoreRandomPosts()
		{
			string oldPostsIds = HttpContext.Session.GetString(sessionRetrievedPostsIds);

			List<PostModel> posts = string.IsNullOrEmpty(oldPostsIds) ?
				await RetrievedRandomPosts(AmountPostsToLoad, "") :
				await RetrievedRandomPosts(AmountPostsToLoad, oldPostsIds);

			//Save the ids of the post in a session variable.
			if (!string.IsNullOrEmpty(oldPostsIds))
			{
				List<int> postsIds = new();

				for (int i = 0; i < posts.Count; i++)
				{
					postsIds.Add(posts[i].Id);
				}

				oldPostsIds += $", {string.Join(", ", postsIds)}";

				HttpContext.Session.SetString(sessionRetrievedPostsIds, oldPostsIds);
			}

			if (posts == null)
				return NotFound("An error has ocurred");

			if (posts.Count == 0)
				return NotFound("There are no more posts");

			await LoadPartialViewInfo();

			return PartialView("PostList", posts);
		}

		public async Task<IActionResult> LoadMoreTopPosts(int startFromRow, int amountOfRows = AmountPostsToLoad)
		{
			List<PostModel> posts = await _Models.Posts
				.FromSqlRaw("SELECT * FROM Posts ORDER BY Likes DESC OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY;", startFromRow, amountOfRows)
				.AsNoTracking().ToListAsync();

			if (posts == null)
				return NotFound("Post could not be loaded");

			if (posts.Count == 0)
				return NoContent();

			posts = await FillPostsProperties(posts);
			await LoadPartialViewInfo();

			return PartialView("PostList", posts);
		}

		public async Task<IActionResult> LoadMoreRecentPosts(int startFromRow, int amountOfRows = AmountPostsToLoad)
		{
			List<PostModel> posts = await _Models.Posts
				.FromSqlRaw("SELECT * FROM Posts ORDER BY Id DESC OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY;", startFromRow, amountOfRows)
				.AsNoTracking().ToListAsync();

			if (posts == null)
				return NotFound("Post could not be loaded");

			if (posts.Count == 0)
				return NoContent();

			posts = await FillPostsProperties(posts);
			await LoadPartialViewInfo();

			return PartialView("PostList", posts);
		}

		public async Task<IActionResult> LoadMoreOldPosts(int startFromRow, int amountOfRows = AmountPostsToLoad)
		{
			List<PostModel> posts = await _Models.Posts
				.FromSqlRaw("SELECT * FROM Posts ORDER BY Id OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY;", startFromRow, amountOfRows)
				.AsNoTracking().ToListAsync();

			if (posts == null)
				return NotFound("Post could not be loaded");

			if (posts.Count == 0)
				return NoContent();

			posts = await FillPostsProperties(posts);
			await LoadPartialViewInfo();

			return PartialView("PostList", posts);
		}

		private async Task<List<PostModel>> RetrievedRandomPosts(int amountOfPosts, string oldPostsIds)
		{
			string sql = !string.IsNullOrEmpty(oldPostsIds) ?
				$"SELECT TOP {amountOfPosts} * FROM Posts WHERE Id NOT IN ({oldPostsIds}) ORDER BY NEWID();" :
				$"SELECT TOP {amountOfPosts} * FROM Posts ORDER BY NEWID();";

			List<PostModel> posts = await _Models.Posts
				.FromSqlRaw(sql)
				.AsNoTracking().ToListAsync();

			posts = await FillPostsProperties(posts);

			return posts;
		}

		//Loads the comments for a single post.
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

		public async Task<List<PostModel>> FillPostsProperties(List<PostModel> posts)
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

		public async Task LoadPartialViewInfo(UserModel loggedUser = null)
		{
			loggedUser ??= await _UserManager.GetUserAsync(HttpContext.User);

			ViewData["LoggedUserId"] = loggedUser.Id;
			ViewData["commentsAmount"] = showCommentsPerPost;
			ViewBag.blur = loggedUser.ShowImages ? "1" : "0";
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
	}
}
