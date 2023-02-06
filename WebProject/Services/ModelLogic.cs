using Microsoft.EntityFrameworkCore;
using System.Text;
using WebProject.Data;
using WebProject.Models;
#nullable disable

namespace WebProject.Services
{
	public class ModelLogic
	{
		private readonly WebProjectContext _Models;

		public ModelLogic(WebProjectContext models)
		{
			_Models = models;
		}

		public async Task<List<PostModel>> FillPostsProperties(List<PostModel> posts, UserModel postOwner = null)
		{
			List<UserModel> users = new();

			if (postOwner == null)
			{
				StringBuilder userIds = new();
				for (int i = 0; i < posts.Count; i++)
				{
					if (i == posts.Count - 1)
						userIds.Append($"'{posts[i].UserId}'");
					else
						userIds.Append($"'{posts[i].UserId}', ");
				}

				users = userIds.Length == 0 ? new() : await _Models.Users
					.FromSqlRaw(string.Format("SELECT * FROM AspNetUsers WHERE Id IN({0});", userIds.ToString()))
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

			posts = await RangeLoadComments(posts);

			return posts;
		}

		public async Task<PostModel> SingleFillPostProperties(PostModel post, UserModel postOwner = null)
		{
			var result = await FillPostsProperties(new List<PostModel> { post }, postOwner);

			return result.FirstOrDefault();
		}

		public async Task<List<PostModel>> RangeLoadComments(List<PostModel> posts)
		{
			StringBuilder modelsIds = new();

			for (int i = 0; i < posts.Count; i++)
			{
				if (i == posts.Count - 1)
					modelsIds.Append($"{posts[i].Id}");
				else
					modelsIds.Append($"{posts[i].Id}, ");
			}

			List<CommentModel> comments = modelsIds.Length == 0 ? new() : await _Models.Comments
				.FromSqlRaw(string.Format("SELECT * FROM Comments WHERE PostId IN ({0})", modelsIds.ToString()))
				.AsNoTracking().ToListAsync();

			modelsIds = new();

			for (int i = 0; i < comments.Count; i++)
			{
				if (i == comments.Count - 1)
					modelsIds.Append($"'{comments[i].UserId}'");
				else
					modelsIds.Append($"'{comments[i].UserId}', ");
			}

			List<UserModel> users = modelsIds.Length == 0 ? new() : await _Models.Users
				.FromSqlRaw(string.Format("SELECT * FROM AspNetUsers WHERE Id IN({0});", modelsIds.ToString()))
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

		public async Task<PostModel> SingleLoadComments(PostModel post)
		{
			var result = await RangeLoadComments(new List<PostModel> { post });

			return result.FirstOrDefault();
		}

		public async Task<List<UserModel>> GetPostLikesSelective(int postId)
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
		public async Task<List<UserModel>> GetCommentLikesSelective(int commentId)
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
