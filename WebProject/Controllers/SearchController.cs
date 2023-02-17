using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Text;
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
			List<UserModel> users = await _Models.Users.FromSqlRaw($"SELECT TOP {amountOfUsers} * FROM Users ORDER BY NEWID();")
				.AsNoTracking().ToListAsync();

			return PartialView("UsersList", users);
		}

		//var mostLikedUsers = _Models.Posts
		//	.GroupBy(p => p.UserId)
		//	.Select(g => new { UserId = g.Key, Total = g.Sum(p => p.Likes) })
		//	.OrderByDescending(x => x.Total)
		//	.Take(10)
		//	.ToList();

		public async Task<IActionResult> LoadMoreUsersWithMostCommmentsInPosts(int startFromRow, int amountOfRows = usersPerLoad)
		{
			List<UserModel> users = await GetUsersWithMostCommentsInPosts(startFromRow, amountOfRows);

			if (users == null)
				return NotFound("Users not found.");

			if (users.Count == 0)
				return NoContent();

			return PartialView("UsersList", users);
		}

		public async Task<List<UserModel>> GetUsersWithMostCommentsInPosts(int startFromRow, int amountOfRows)
		{
			List<UserModel> mostCommentedUsers = await _Models.Users
				.FromSqlRaw("SELECT Posts.UserId, COUNT(Comments.Id) AS TotalComments " +
				"FROM Posts JOIN Comments ON Comments.PostId = Posts.Id GROUP BY Posts.UserId " +
				"ORDER BY TotalComments DESC OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY;", startFromRow, amountOfRows).AsNoTracking().ToListAsync();

			return mostCommentedUsers;
		}

		public async Task<IActionResult> LoadMoreUsersWithMostFollowers (int startFromRow, int amountOfRows = usersPerLoad)
		{
			List<UserModel> users = await GetUsersWithMostFollowers(startFromRow, amountOfRows);

			if (users == null)
				return NotFound("Users not found.");

			if (users.Count == 0)
				return NoContent();

			return PartialView("UsersList", users);
		}

		public async Task<List<UserModel>> GetUsersWithMostFollowers(int startFromRow, int amountOfRows)
		{
			List<UserModel> usersWithMostFollowers = await _Models.Users
				.FromSqlRaw("SELECT Followers.CreatorId, COUNT(Followers.FollowerId) AS TotalFollowers " +
				"FROM Followers GROUP BY CreatorId ORDER BY TotalFollowers DESC " +
				"OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY;", startFromRow, amountOfRows).AsNoTracking().ToListAsync();

			return usersWithMostFollowers;
		}

		public async Task<IActionResult> LoadMoreUsersWithMostLikes(int startFromRow, int amountOfRows = usersPerLoad)
		{
			List<UserModel> users = await GetUsersWithMostLikes(startFromRow, amountOfRows);

			if (users == null)
				return NotFound("Users not found.");

			if (users.Count == 0)
				return NoContent();

			return PartialView("UsersList", users);
		}

		public async Task<List<UserModel>> GetUsersWithMostLikes(int startFromRow, int amountOfRows)
		{
			List<UserModel> usersWithMostLikes = await UsersFromQuery(
				"SELECT U.Id, SUM(Posts.Likes) AS Total " +
				"FROM (Users AS U INNER JOIN Posts ON Posts.UserId = U.Id) GROUP BY U.Id " +
				"ORDER BY Total DESC OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY;", 
				startFromRow, amountOfRows);

			return usersWithMostLikes;
		}

		private async Task<List<UserModel>> UsersFromQuery(string sqlQuery, int startFromRow, int amountOfRows)
		{
			List<string> idsOfUsers = await _Models.Database
				.SqlQueryRaw<string>(sqlQuery, startFromRow, amountOfRows)
				.AsNoTracking().ToListAsync();

			StringBuilder idsString = new();

			for (int i = 0; i < idsOfUsers.Count; i++)
			{
				if (i == idsOfUsers.Count - 1)
					idsString.Append($"'{idsOfUsers[i]}'");
				else
					idsString.Append($"'{idsOfUsers[i]}', ");
			}

			return await _Models.Users.FromSqlRaw("SELECT * FROM Users WHERE Id IN ({0})", idsString).AsNoTracking().ToListAsync(); ;
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
