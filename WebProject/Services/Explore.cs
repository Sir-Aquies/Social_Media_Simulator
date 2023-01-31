using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Data.OleDb;
using WebProject.Data;
using WebProject.Models;

namespace WebProject.Services
{
	public class Explore
	{
		private readonly WebProjectContext _Models;
		private const string sessionRetrievedPostsIds = "_retrievedPostsIds";

		public Explore(WebProjectContext Models)
		{
			_Models = Models;
		}

		public async Task<List<PostModel>> RetrievedRandomPosts(int amountOfPosts, string oldPostsIds)
		{
			string sql = !string.IsNullOrEmpty(oldPostsIds) ?
				$"SELECT TOP {amountOfPosts} * FROM Posts WHERE Id NOT IN ({oldPostsIds}) ORDER BY NEWID();" :
				$"SELECT TOP {amountOfPosts} * FROM Posts ORDER BY NEWID();";

			List<int> newIds = new();
			
			List<PostModel> posts = await _Models.Posts
				.FromSqlRaw(sql)
				.AsNoTracking().ToListAsync();

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
