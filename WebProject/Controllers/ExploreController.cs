using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
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
		private readonly ModelLogic _Logic;

		private const int amountPostsPerLoad = 5;
		private const string sessionRetrievedPostsIds = "_retrievedPostsIds";
		private const int inicialAmountPostsToLoad = 10;
		private const int showCommentsPerPost = 3;

		private enum Tab
		{
			Random,
			Top,
			Recent,
			Oldest,
		}

		public ExploreController(WebProjectContext models, UserManager<UserModel> userManager, ModelLogic logic)
		{
			_Models = models;
			_UserManager = userManager;
			_Logic = logic;
		}

		public async Task<IActionResult> Top()
		{
			UserModel loggedUser = await _UserManager.GetUserAsync(HttpContext.User);

			loggedUser.Posts = await PostsFromTab(Tab.Top, 0, inicialAmountPostsToLoad);
			loggedUser.Posts = await _Logic.FillPostsProperties(loggedUser.Posts.ToList());

			ViewBag.StartFromRow = inicialAmountPostsToLoad;
			ViewBag.RowsPerLoad = amountPostsPerLoad;
			ViewBag.tabName = "Top";

			return View("Index", loggedUser);
		}

		public async Task<IActionResult> Recent()
		{
			UserModel loggedUser = await _UserManager.GetUserAsync(HttpContext.User);

			loggedUser.Posts = await PostsFromTab(Tab.Recent, 0, inicialAmountPostsToLoad);
			loggedUser.Posts = await _Logic.FillPostsProperties(loggedUser.Posts.ToList());

			ViewBag.StartFromRow = inicialAmountPostsToLoad;
			ViewBag.RowsPerLoad = amountPostsPerLoad;
			ViewBag.tabName = "Recent";

			return View("Index", loggedUser);
		}

		public async Task<IActionResult> Oldest()
		{
			UserModel loggedUser = await _UserManager.GetUserAsync(HttpContext.User);

			loggedUser.Posts = await PostsFromTab(Tab.Oldest, 0, inicialAmountPostsToLoad);
			loggedUser.Posts = await _Logic.FillPostsProperties(loggedUser.Posts.ToList());

			ViewBag.StartFromRow = inicialAmountPostsToLoad;
			ViewBag.RowsPerLoad = amountPostsPerLoad;
			ViewBag.tabName = "Oldest";

			return View("Index", loggedUser);
		}

		public async Task<IActionResult> Random()
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

			ViewBag.StartFromRow = inicialAmountPostsToLoad;
			ViewBag.RowsPerLoad = amountPostsPerLoad;
			ViewBag.tabName = "Random";

			return View("Index", loggedUser);
		}

		private async Task<List<PostModel>> PostsFromTab(Tab tabName, int startFromRow, int amountOfRows)
		{
			List<PostModel> posts = new();

			switch (tabName)
			{
				case Tab.Top:
					posts = await _Models.Posts
						.FromSqlRaw("SELECT * FROM Posts ORDER BY Likes DESC OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY;", startFromRow, amountOfRows)
						.AsNoTracking().ToListAsync();
					break;
				case Tab.Recent:
					posts = await _Models.Posts
						.FromSqlRaw("SELECT * FROM Posts ORDER BY Id DESC OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY;", startFromRow, amountOfRows)
						.AsNoTracking().ToListAsync();
					break;
				case Tab.Oldest:
					posts = await _Models.Posts
						.FromSqlRaw("SELECT * FROM Posts ORDER BY Id OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY;", startFromRow, amountOfRows)
						.AsNoTracking().ToListAsync();
					break;
			}

			return posts;
		}

		public async Task<IActionResult> LoadMoreRandomPosts()
		{
			string oldPostsIds = HttpContext.Session.GetString(sessionRetrievedPostsIds);

			List<PostModel> posts = string.IsNullOrEmpty(oldPostsIds) ?
				await RetrievedRandomPosts(amountPostsPerLoad, "") :
				await RetrievedRandomPosts(amountPostsPerLoad, oldPostsIds);

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

		public async Task<IActionResult> LoadMoreTopPosts(int startFromRow, int amountOfRows = amountPostsPerLoad)
		{
			List<PostModel> posts = await PostsFromTab(Tab.Top, startFromRow, amountOfRows);

			if (posts == null)
				return NotFound("Post could not be loaded");

			if (posts.Count == 0)
				return NoContent();

			posts = await _Logic.FillPostsProperties(posts);
			await LoadPartialViewInfo();

			return PartialView("PostList", posts);
		}

		public async Task<IActionResult> LoadMoreRecentPosts(int startFromRow, int amountOfRows = amountPostsPerLoad)
		{
			List<PostModel> posts = await PostsFromTab(Tab.Recent, startFromRow, amountOfRows);

			if (posts == null)
				return NotFound("Post could not be loaded");

			if (posts.Count == 0)
				return NoContent();

			posts = await _Logic.FillPostsProperties(posts);
			await LoadPartialViewInfo();

			return PartialView("PostList", posts);
		}

		public async Task<IActionResult> LoadMoreOldPosts(int startFromRow, int amountOfRows = amountPostsPerLoad)
		{
			List<PostModel> posts = await PostsFromTab(Tab.Oldest, startFromRow, amountOfRows);

			if (posts == null)
				return NotFound("Post could not be loaded");

			if (posts.Count == 0)
				return NoContent();

			posts = await _Logic.FillPostsProperties(posts);
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

			posts = await _Logic.FillPostsProperties(posts);

			return posts;
		}

		public async Task LoadPartialViewInfo(UserModel loggedUser = null)
		{
			loggedUser ??= await _UserManager.GetUserAsync(HttpContext.User);

			ViewData["LoggedUserId"] = loggedUser.Id;
			ViewData["commentsAmount"] = showCommentsPerPost;
			ViewBag.blur = loggedUser.ShowImages ? "1" : "0";
		}
	}
}
