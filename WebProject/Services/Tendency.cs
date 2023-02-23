using Microsoft.EntityFrameworkCore;
using System.Text;
using WebProject.Data;
using WebProject.Models;
#nullable disable
namespace WebProject.Services
{
	public class Tendency : ITendency
	{
		public List<UserModel> UsersWithMostFollowers => userFollowers;
		public List<UserModel> UsersWithMostLikes => userLikes;
		public List<UserModel> UsersWithMostPost => userPosts;
		public List<PostModel> PostsWithMostLikes => postLikes;
		public List<PostModel> PostWithMostComments => postComments;

		public List<UserModel> userFollowers = new();
		public List<UserModel> userLikes = new();
		private List<UserModel> userPosts = new();
		private List<PostModel> postLikes = new();
		private List<PostModel> postComments = new();

		//TODO - Refactor all the CSS files.
		public async Task UpdateStats(WebProjectContext _Models, int amountOfRows = 10)
		{
			StringBuilder usersQuery = new("SELECT * FROM Users WHERE Id IN " +
				"(SELECT UserId FROM Posts GROUP BY UserId ORDER BY SUM(Likes) " +
				"DESC OFFSET 0 ROWS FETCH NEXT {0} ROWS ONLY);");
			StringBuilder statQuery = new("SELECT SUM(Likes) FROM Posts WHERE UserId = {0};");

			userLikes = await UsersFromQuery(_Models, usersQuery, amountOfRows, statQuery);

			usersQuery.Clear();
			statQuery.Clear();

			usersQuery.Append("SELECT * FROM Users WHERE Id IN " +
				"(SELECT CreatorId FROM Followers GROUP BY CreatorId ORDER BY " +
				"COUNT(FollowerId) DESC OFFSET 0 ROWS FETCH NEXT {0} ROWS ONLY);");

			statQuery.Append("SELECT COUNT(FollowerId) FROM Followers WHERE CreatorId = {0};");

			userFollowers = await UsersFromQuery(_Models, usersQuery, amountOfRows, statQuery);

			usersQuery.Clear();
			statQuery.Clear();

			usersQuery.Append("SELECT * FROM Users ORDER BY " +
				"COALESCE((SELECT COUNT(Posts.Id) FROM Posts WHERE Posts.UserId = Users.Id), 1) " +
				"DESC OFFSET 0 ROWS FETCH NEXT {0} ROWS ONLY;");

			statQuery.Append("SELECT COUNT(Id) FROM Posts WHERE UserId = {0};");

			userPosts = await UsersFromQuery(_Models, usersQuery, amountOfRows, statQuery);

			postLikes = await _Models.Posts
				.FromSqlRaw($"SELECT TOP {amountOfRows} * FROM Posts ORDER BY Likes DESC;")
				.AsNoTracking().ToListAsync();

			postComments = await _Models.Posts.FromSqlRaw("SELECT * FROM Posts ORDER BY " +
				"COALESCE((SELECT COUNT(Comments.Id) FROM Comments WHERE Comments.PostId = Posts.Id), 0) " +
				"DESC OFFSET 0 ROWS FETCH NEXT {0} ROWS ONLY;", amountOfRows).AsNoTracking().ToListAsync();
		}

		private async Task<List<UserModel>> UsersFromQuery(WebProjectContext _Models, StringBuilder usersQuery, int amountOfRows, StringBuilder statQuery = null)
		{
			List<UserModel> topUsers = await _Models.Users.FromSqlRaw(usersQuery.ToString(), amountOfRows).AsNoTracking().ToListAsync();

			if (statQuery != null)
			{
				for (int i = 0; i < topUsers.Count; i++)
				{
					List<int> total = await _Models.Database.SqlQueryRaw<int>(statQuery.ToString(), topUsers[i].Id).ToListAsync();

					topUsers[i].Total = total.FirstOrDefault();
				}
			}

			return topUsers;
		}
	}

	public interface ITendency
	{
		List<UserModel> UsersWithMostLikes { get; }
		List<UserModel> UsersWithMostPost { get; }
		List<UserModel> UsersWithMostFollowers { get; }
		List<PostModel> PostsWithMostLikes { get; }
		List<PostModel> PostWithMostComments { get; }
		Task UpdateStats(WebProjectContext _Models, int amount = 10);
	}
}
