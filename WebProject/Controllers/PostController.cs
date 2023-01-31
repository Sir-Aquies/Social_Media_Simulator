﻿#nullable disable
using Microsoft.AspNetCore.Mvc;
using WebProject.Models;
using WebProject.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.VisualBasic;

namespace WebProject.Controllers
{
	[Authorize]
	public class PostController : Controller
	{
		private readonly WebProjectContext _Models;
		private readonly UserManager<UserModel> _UserManager;

		public PostController(WebProjectContext Models, UserManager<UserModel> manager)
		{
			_Models = Models;
			_UserManager = manager;
		}

		public async Task<IActionResult> ViewPost(int postId)
		{
			PostModel post = await _Models.Posts.FromSqlRaw("SELECT * FROM Posts WHERE Id = {0}", postId).AsNoTracking().FirstOrDefaultAsync();

			if (post == null) 
				return NotFound("Error, post was not found.");

			post.UsersLikes = await GetPostLikesSelective(postId);
			post.Comments = await LoadComments(post);
			post.User = await _Models.Users
				.Select(u => new UserModel { Id = u.Id, UserName = u.UserName, ProfilePicture = u.ProfilePicture })
				.Where(u => u.Id == post.UserId).AsNoTracking().FirstOrDefaultAsync();

			return PartialView("ViewPost", post);
		}

		public IActionResult LookForCreatePost() => PartialView("CreatePost");

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreatePost(IFormFile Media, string Content)
		{
			UserModel loggedUser = await _UserManager.GetUserAsync(HttpContext.User);

			if (loggedUser == null)
				return Unauthorized("User is null");

			PostModel newPost = new();

			if (!string.IsNullOrEmpty(Content))
			{
				newPost.Content = Content;
			}

			if (Media != null)
			{
				newPost.Media = Convert.ToBase64String(await GetBytes(Media));
			}

			if (!string.IsNullOrEmpty(newPost.Content) || newPost.Media != null)
			{
				newPost.PostDate = DateTime.Now;
				newPost.UserId = loggedUser.Id;
				_Models.Posts.Add(newPost);
				await _Models.SaveChangesAsync();
			}
			else
			{
				return BadRequest("Post could not be created");
			}

			//Initiate lists so they are not null.
			newPost.UsersLikes = new List<UserModel>();
			newPost.Comments = new List<CommentModel>();
			ViewData["UserId"] = loggedUser.Id;

			return PartialView("Post", newPost);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditPost(string Content, IFormFile Media, bool DeleteMedia)
		{
			int PostId = 0;
			bool PostIdBool = false;

			if (!string.IsNullOrEmpty(TempData["PostId"]?.ToString()))
			{
				PostIdBool = int.TryParse(TempData["PostId"]?.ToString(), out PostId);
			}

			PostModel postModel = new();

			if (PostIdBool)
			{
				postModel = await _Models.Posts.FirstOrDefaultAsync(p => p.Id == PostId);
			}
			else
			{
				return NotFound("Post not found.");
			}

			UserModel loggedUser = await _UserManager.GetUserAsync(HttpContext.User);

			if (loggedUser.Id != postModel.UserId)
			{
				return Unauthorized("Access denied, post does not belong to current user.");
			}

			if (Media != null)
			{
				postModel.Media = Convert.ToBase64String(await GetBytes(Media));
			}
			else if (DeleteMedia)
			{
				postModel.Media = null;
			}

			if (Content != postModel.Content)
			{
				postModel.Content = Content;
			}

			if (!string.IsNullOrEmpty(postModel.Content) || postModel.Media != null)
			{
				postModel.IsEdited = true;
				postModel.EditedDate = DateTime.Now;

				_Models.Posts.Update(postModel);
				await _Models.SaveChangesAsync();
			}
			else
			{
				return BadRequest("Error, post was not edited due to not having content");
			}

			postModel.UsersLikes = await GetPostLikesSelective(postModel.Id);
			postModel.Comments = await LoadComments(postModel);
			ViewData["UserId"] = loggedUser.Id;

			return PartialView("Post", postModel);
		}

		[HttpGet]
		public async Task<IActionResult> LookforPost(int PostId)
		{
			PostModel post = await _Models.Posts.AsNoTracking().FirstOrDefaultAsync(us => us.Id == PostId);

			if (post == null)
				return NotFound("Post does not exist.");

			UserModel loggedUser = await _UserManager.GetUserAsync(HttpContext.User);

			if (loggedUser != null && loggedUser.Id != post.UserId)
				return Unauthorized("Access denied, post does not belong to current user.");

			//Save the Id to be use in EditPost
			TempData["PostId"] = PostId;
			return PartialView("EditPost", post);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<bool> DeletePost(int PostId)
		{
			UserModel loggedUser = await _UserManager.GetUserAsync(HttpContext.User);
			PostModel post = await _Models.Posts.Include(p => p.Comments).ThenInclude(c => c.UsersLikes).Include(p => p.UsersLikes).FirstOrDefaultAsync(us => us.Id == PostId);

			if (loggedUser.Id != post.UserId || post == null)
			{
				return false;
			}

			post.UsersLikes.Clear();
			foreach (CommentModel comment in post.Comments)
			{
				comment.UsersLikes.Clear();
			}

			_Models.Remove(post);
			await _Models.SaveChangesAsync();
			return true;
		}

		[HttpPost]
		public async Task<string> LikePost(int postId)
		{
			UserModel loggedUser = await _UserManager.GetUserAsync(HttpContext.User);

			if (postId < 1)
			{
				return "0";
			}

			if (loggedUser == null)
			{
				return "0";
			}

			//Check if a row exists in PostLikes, if it exists returns the post's id.
			List<int> postIdExists = await _Models.Database
				.SqlQueryRaw<int>("SELECT PostId FROM PostLikes WHERE PostId = {0} AND UserId = {1};", postId, loggedUser.Id).ToListAsync();
			
			if (postIdExists.Count == 1)
			{
				//Decrease by one the likes column of the post row.
				int affectedRows = await _Models.Database.ExecuteSqlRawAsync("UPDATE Posts SET Likes = Likes - 1 WHERE Id = {0};", postId);

				if (affectedRows == 1) 
					//Delete the row from PostLikes.
					affectedRows += await _Models.Database
						.ExecuteSqlRawAsync("DELETE FROM PostLikes WHERE PostId = {0} AND UserId = {1};", postId, loggedUser.Id);

				//If both action were successful affectedRows should be 2.
				return affectedRows == 2 ? "-" : "0";
			}
			else
			{
				//Increase by one the likes column of the post row.
				int affectedRows = await _Models.Database.ExecuteSqlRawAsync("UPDATE Posts SET Likes = Likes + 1 WHERE Id = {0};", postId);

				if (affectedRows == 1)
					//Add a new row to PostLikes.
					affectedRows += await _Models.Database
						.ExecuteSqlRawAsync("INSERT INTO PostLikes (PostId, UserId) VALUES ({0}, {1});", postId, loggedUser.Id);

				//If both action were successful affectedRows should be 2.
				return affectedRows == 2 ? "+" : "0";
			}
		}

		//Loads and returns in a partial view the users who like a certain post.
		[HttpGet]
		public async Task<IActionResult> PostLikesTab(int postId)
		{
			List<UserModel> users = await _Models.Users
				.FromSqlRaw($"Select * from AspNetUsers where Id in (Select UserId from PostLikes where PostId = {postId})")
				.AsNoTracking().ToListAsync();

			if (users.Count == 0)
			{
				return NotFound();
			}

			return PartialView("UsersList", users);
		}

		//Loads and returns in a partial view the users who like a certain comment.
		[HttpGet]
		public async Task<IActionResult> CommentLikesTab(int commentId)
		{
			List<UserModel> users = await _Models.Users
				.FromSqlRaw($"Select * from AspNetUsers where Id in (Select UserId from CommentLikes where CommentId = {commentId})")
				.AsNoTracking().ToListAsync();

			if (users.Count == 0)
			{
				return NotFound();
			}

			return PartialView("UsersList", users);
		}

		[HttpPost]
		public IActionResult LookForCreateComment(int PostId)
		{
			//Save the post id to be use in CreateComment.
			TempData["PostId"] = PostId;

			return PartialView("CreateComment");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreateComment(string Content)
		{
			UserModel loggedUser = await _UserManager.GetUserAsync(HttpContext.User);
			
			int PostId = 0;
			bool PostIdBool = false;

			if (!string.IsNullOrEmpty(TempData["PostId"]?.ToString()))
			{
				PostIdBool = int.TryParse(TempData["PostId"]?.ToString(), out PostId);
			}

			CommentModel comment = new();

			if (!string.IsNullOrEmpty(Content) && PostIdBool)
			{
				comment.PostId = PostId;
				comment.Date = DateTime.Now;
				comment.UserId = loggedUser.Id;
				comment.Content = Content;

				await _Models.AddAsync(comment);
				await _Models.SaveChangesAsync();
			}

			//Initiate list so is not null.
			comment.UsersLikes = new List<UserModel>();

			//Save info for the comment partial view.
			ViewData["LoggedUserId"] = loggedUser.Id;

			return PartialView("Comment", comment);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<bool> DeleteComment(int CommentId)
		{
			UserModel loggedUser = await _UserManager.GetUserAsync(HttpContext.User);

			CommentModel comment = await _Models.Comments.Include(c => c.UsersLikes)
					.Include(c => c.Post).FirstOrDefaultAsync(c => c.Id == CommentId);

			if (loggedUser.Id != comment.UserId)
			{
				return false;
			}

			if (comment != null)
			{
				comment.UsersLikes.Clear();
				_Models.Comments.Remove(comment);
				await _Models.SaveChangesAsync();

				return true;
			}
			else
			{
				return false;
			}
		}

		[HttpPost]
		public async Task<string> LikeComment(int CommentId)
		{
			UserModel userModel = await _UserManager.GetUserAsync(HttpContext.User);

			if (CommentId < 1)
			{
				return "0";
			}

			if (userModel == null)
			{
				return "0";
			}

			//Check if a row exists in CommentLikes, if it exists returns the comment's id.
			List<int> commentIdExists = await _Models.Database
				.SqlQueryRaw<int>("SELECT CommentId FROM CommentLikes WHERE CommentId = {0} AND UserId = {1};", CommentId, userModel.Id).ToListAsync();

			if (commentIdExists.Count == 1)
			{
				//Decrease by one the likes column of the comment row.
				int affectedRows = await _Models.Database.ExecuteSqlRawAsync("UPDATE Comments SET Likes = Likes - 1 WHERE Id = {0};", CommentId);

				if (affectedRows == 1)
					//Delete the row from CommentLikes.
					affectedRows += await _Models.Database
						.ExecuteSqlRawAsync("DELETE FROM CommentLikes WHERE CommentId = {0} AND UserId = {1};", CommentId, userModel.Id);

				//If both action were successful affectedRows should be 2.
				return affectedRows == 2 ? "-" : "0";
			}
			else
			{
				//Increase by one the likes column of the comment row.
				int affectedRows = await _Models.Database.ExecuteSqlRawAsync("UPDATE Comments SET Likes = Likes + 1 WHERE Id = {0};", CommentId);

				if (affectedRows == 1)
					//Add a new row to CommentLikes.
					affectedRows += await _Models.Database
						.ExecuteSqlRawAsync("INSERT INTO CommentLikes (CommentId, UserId) VALUES ({0}, {1});", CommentId, userModel.Id);

				//If both action were successful affectedRows should be 2.
				return affectedRows == 2 ? "+" : "0";
			}
		}

		public async Task<List<UserModel>> GetPostLikesSelective(int postId)
		{
			List<UserModel> users = new();

			string[] userLikes = await _Models.Database.SqlQueryRaw<string>($"Select UserId from PostLikes where PostId = {postId}").AsNoTracking().ToArrayAsync();

			for (int i = 0; i < userLikes.Length; i++)
			{
				users.Add(new UserModel { Id = userLikes[i] });
			}

			return users;
		}

		public async Task<List<CommentModel>> LoadComments(PostModel post)
		{
			List<CommentModel> comments = await _Models.Comments.FromSqlRaw($"Select * from Comments where PostId = {post.Id}").AsNoTracking().ToListAsync();

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

		public async Task<List<UserModel>> GetCommentLikesSelective(int commentId)
		{
			List<UserModel> users = new();

			string[] userLikes = await _Models.Database.SqlQueryRaw<string>($"Select UserId from CommentLikes where CommentId = {commentId}").AsNoTracking().ToArrayAsync();

			for (int i = 0; i < userLikes.Length; i++)
			{
				users.Add(new UserModel { Id = userLikes[i] });
			}

			return users;
		}

		private async Task<byte[]> GetBytes(IFormFile formFile)
		{
			await using var memoryStream = new MemoryStream();
			await formFile.CopyToAsync(memoryStream);
			return memoryStream.ToArray();
		}
	}
}
