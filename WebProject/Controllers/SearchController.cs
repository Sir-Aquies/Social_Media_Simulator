using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Text;
using System.Xml.Linq;
using WebProject.Data;
using WebProject.Models;
using WebProject.Services;
#nullable disable

namespace WebProject.Controllers
{
	public class SearchController : Controller
	{
		private readonly WebProjectContext _Models;
		private readonly UserManager<UserModel> _UserManager;
		private readonly ITendency _Tendency;
		private readonly ModelLogic _Logic;

		private const int inicialAmountOfUsers = 10;
		private const int usersPerLoad = 5;

		public SearchController(WebProjectContext models, UserManager<UserModel> userManager, ITendency tendency, ModelLogic modelLogic)
		{
			_Models = models;
			_UserManager = userManager;
			_Tendency = tendency;
			_Logic = modelLogic;
		}

		public async Task<IActionResult> SearchUsers()
		{
			UserModel loggedUser = await _UserManager.GetUserAsync(HttpContext.User);

			ViewData["startFromRow"] = inicialAmountOfUsers;
			ViewData["rowsPerLoad"] = usersPerLoad;

			return View(loggedUser);
		}

		public async Task<IActionResult> LookUsersByUserName(string userName)
		{
			List<UserModel> users = await _Models.Users
				.FromSqlRaw("SELECT * FROM Users WHERE UserName LIKE {0};", userName + "%")
				.AsNoTracking().ToListAsync();

			return PartialView("UsersList", users);
		}

		public async Task<IActionResult> LoadRandomUsers(int amountOfRows)
		{
			List<UserModel> users = await _Models.Users.FromSqlRaw($"SELECT TOP {amountOfRows} * FROM Users ORDER BY NEWID();")
				.AsNoTracking().ToListAsync();

			return PartialView("UsersList", users);
		}

		public async Task<IActionResult> LoadMoreUsersWithMostLikes(int startFromRow, int amountOfRows = usersPerLoad)
		{
			List<UserModel> users = await GetUsersWithMostLikes(startFromRow, amountOfRows);

			if (users == null)
				return NotFound("Users not found.");

			if (users.Count == 0)
				return NoContent();

			ViewData["userListStatName"] = "Total Likes";

			return PartialView("UsersList", users);
		}

		//var mostLikedUsers = _Models.Posts
		//	.GroupBy(p => p.UserId)
		//	.Select(g => new { UserId = g.Key, Total = g.Sum(p => p.Likes) })
		//	.OrderByDescending(x => x.Total)
		//	.Take(10)
		//	.ToList();
		private async Task<List<UserModel>> GetUsersWithMostLikes(int startFromRow, int amountOfRows)
		{
			string usersQuery = "SELECT * FROM Users WHERE Id IN " +
				"(SELECT UserId FROM Posts GROUP BY UserId ORDER BY SUM(Likes) " +
				"DESC OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY);";

			string statQuery = "SELECT SUM(Likes) FROM Posts WHERE UserId = {0};";

			List<UserModel> usersWithMostLikes = await UsersFromQuery(usersQuery, startFromRow, amountOfRows, statQuery);

			return usersWithMostLikes;
		}

		public async Task<IActionResult> LoadMoreUsersWithMostFollowers(int startFromRow, int amountOfRows = usersPerLoad)
		{
			List<UserModel> users = await GetUsersWithMostFollowers(startFromRow, amountOfRows);

			if (users == null)
				return NotFound("Users not found.");

			if (users.Count == 0)
				return NoContent();

			ViewData["userListStatName"] = "Total Followers";

			return PartialView("UsersList", users);
		}

		private async Task<List<UserModel>> GetUsersWithMostFollowers(int startFromRow, int amountOfRows)
		{
			string usersQuery = "SELECT * FROM Users WHERE Id IN " +
				"(SELECT CreatorId FROM Followers GROUP BY CreatorId ORDER BY " +
				"COUNT(FollowerId) DESC OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY);";

			string statQuery = "SELECT COUNT(FollowerId) FROM Followers WHERE CreatorId = {0};";

			List<UserModel> usersWithMostFollowers = await UsersFromQuery(usersQuery, startFromRow, amountOfRows, statQuery);

			return usersWithMostFollowers;
		}

		public async Task<IActionResult> LoadMoreUsersWithMostCommmentsInPosts(int startFromRow, int amountOfRows = usersPerLoad)
		{
			List<UserModel> users = await GetUsersWithMostCommentsInPosts(startFromRow, amountOfRows);

			if (users == null)
				return NotFound("Users not found.");

			if (users.Count == 0)
				return NoContent();

			ViewData["userListStatName"] = "Total Comments";

			return PartialView("UsersList", users);
		}

		private async Task<List<UserModel>> GetUsersWithMostCommentsInPosts(int startFromRow, int amountOfRows)
		{
			string usersQuery = "SELECT * FROM Users WHERE Id IN" +
				"(SELECT MainPosts.UserId FROM Posts AS MainPosts GROUP BY MainPosts.UserId ORDER BY " +
				"COALESCE((SELECT COUNT(Comments.Id) FROM Comments WHERE Comments.PostId IN " +
				"(SELECT Posts.Id FROM Posts WHERE Posts.UserId = MainPosts.UserId)), 0) " +
				"DESC OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY);";

			string statQuery = "SELECT COUNT(Comments.Id) FROM Comments WHERE Comments.PostId IN (SELECT Posts.Id FROM Posts WHERE Posts.UserId = {0})";

			List<UserModel> mostCommentedUsers = await UsersFromQuery(usersQuery, startFromRow, amountOfRows, statQuery);

			return mostCommentedUsers;
		}

		private async Task<List<UserModel>> UsersFromQuery(string usersQuery, int startFromRow, int amountOfRows, string statQuery = null)
		{
			List<UserModel> topUsers = await _Models.Users.FromSqlRaw(usersQuery, startFromRow, amountOfRows).AsNoTracking().ToListAsync();

			if (statQuery != null)
			{
				for (int i = 0; i < topUsers.Count; i++)
				{
					List<int> total = await _Models.Database.SqlQueryRaw<int>(statQuery, topUsers[i].Id).ToListAsync();

					topUsers[i].Total = total.FirstOrDefault();
				}
			}
			
			return topUsers;
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
