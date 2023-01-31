using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
		private readonly Explore _Explore;

		private readonly int AmountPostsToLoad = 5;
		private const string sessionRetrievedPostsIds = "_retrievedPostsIds";

		public ExploreController(WebProjectContext models, UserManager<UserModel> userManager, ITendency tendency, Explore explore)
		{
			_Models = models;
			_UserManager = userManager;
			_Tendency = tendency;
			_Explore = explore;
		}

		public async Task<IActionResult> Index()
		{
			UserModel loggedUser = await _UserManager.GetUserAsync(HttpContext.User);

			loggedUser.Posts = await _Explore.RetrievedRandomPosts(10, "");

			List<int> postsIds = new();

			for (int i = 0; i < loggedUser.Posts.Count; i++)
			{
				postsIds.Add(loggedUser.Posts[i].Id);
			}

			string newIds = string.Join(", ", postsIds);
			HttpContext.Session.SetString(sessionRetrievedPostsIds, newIds);

			return View(loggedUser);
		}

		public async Task<IActionResult> LoadMoreRandomPosts()
		{
			string oldPostsIds = HttpContext.Session.GetString(sessionRetrievedPostsIds);

			List<PostModel> posts = string.IsNullOrEmpty(oldPostsIds) ?
				await _Explore.RetrievedRandomPosts(AmountPostsToLoad, "") :
				await _Explore.RetrievedRandomPosts(AmountPostsToLoad, oldPostsIds);

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

			UserModel loggedUser = await _UserManager.GetUserAsync(HttpContext.User);

			if (posts == null)
				return NotFound("An error has ocurred");

			if (posts.Count == 0)
				return NotFound("There are no more posts");

			ViewData["LoggedUserId"] = loggedUser.Id;
			ViewData["commentsAmount"] = "3";
			ViewBag.blur = loggedUser.ShowImages ? "1" : "0";

			return PartialView("PostList", posts);
		}
	}
}
