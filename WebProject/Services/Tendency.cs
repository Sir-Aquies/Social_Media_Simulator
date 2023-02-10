using Microsoft.EntityFrameworkCore;
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

		//TODO - Create a tab in to show full list.
		public async Task UpdateStats(WebProjectContext _Models, int amount = 10)
		{
			userFollowers = await SqlInUsers($"SELECT TOP {amount} U.Id, COUNT(FollowUsers.FollowerId) AS Followers FROM " +
				$"(Users AS U INNER JOIN FollowUsers ON FollowUsers.CreatorId = U.Id) " +
				$"GROUP BY U.Id ORDER BY Followers DESC;", _Models);

			userLikes = await SqlInUsers($"SELECT TOP {amount} U.Id, SUM(Posts.Likes) AS Total " +
				"FROM (Users AS U INNER JOIN Posts ON Posts.UserId = U.Id) " +
				"GROUP BY U.Id ORDER BY Total DESC;", _Models);

			userPosts = await SqlInUsers($"SELECT TOP {amount} U.Id, Count(P.Id) AS Total " +
				"FROM Users AS U INNER JOIN Posts AS P ON P.UserId = U.Id " +
				"GROUP BY U.Id ORDER BY Total DESC;", _Models);

			postLikes = await _Models.Posts
				.FromSqlRaw($"SELECT TOP {amount} * FROM Posts ORDER BY Likes DESC;")
				.AsNoTracking().ToListAsync();

			List<int> postCommenstIds = await _Models.Database
				.SqlQueryRaw<int>($"SELECT TOP {amount} P.Id, COUNT(C.Id) AS total " +
				$"FROM Posts AS P INNER JOIN Comments AS C ON C.PostId = P.Id " +
				$"GROUP BY P.Id ORDER BY total DESC;").ToListAsync();

			postComments = await _Models.Posts.Where(p => postCommenstIds.Contains(p.Id)).AsNoTracking().ToListAsync();
		}

		public async Task<List<UserModel>> SqlInUsers(string sql, WebProjectContext _Models)
		{
			List<string> idsForLikes = await _Models.Database
				.SqlQueryRaw<string>(sql).AsNoTracking().ToListAsync();

			List<UserModel> users = new();

			for (int i = 0; i < idsForLikes.Count; i++)
			{
				users.Add(await _Models.Users.Select(u =>
				new UserModel { Id = u.Id, UserName = u.UserName, ProfilePicture = u.ProfilePicture })
					.Where(u => u.Id == idsForLikes[i])
					.AsNoTracking().FirstOrDefaultAsync());
			}

			return users;
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
