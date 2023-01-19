#nullable disable
using Microsoft.AspNetCore.Mvc;
using WebProject.Models;
using WebProject.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Hosting;

namespace WebProject.Controllers
{
	[Authorize]
	public class PostController : Controller
	{
		private readonly WebProjectContext _Models;
		private readonly UserManager<UserModel> _userManager;

		public PostController(WebProjectContext Models, UserManager<UserModel> manager)
		{
			_Models = Models;
			_userManager = manager;
		}

		public IActionResult LookForCreatePost() => PartialView("CreatePost");

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreatePost(IFormFile Media, string Content)
		{
			UserModel userModel = await _userManager.GetUserAsync(HttpContext.User);

			PostModel postModel = new();

			if (!string.IsNullOrEmpty(Content))
			{
				postModel.Content = Content;
			}

			if (Media != null)
			{
				postModel.Media = Convert.ToBase64String(await GetBytes(Media));
			}

			if (!string.IsNullOrEmpty(postModel.Content) || postModel.Media != null)
			{
				postModel.PostDate = DateTime.Now;
				postModel.UserId = userModel.Id;
				await _Models.Posts.AddAsync(postModel);
				await _Models.SaveChangesAsync();
			}

			postModel.UsersLikes = new List<UserModel>();
			postModel.Comments = new List<CommentModel>();
			ViewData["UserId"] = userModel.Id;

			return PartialView("Post", postModel);
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
				TempData["ErrorMessage"] = "Post not found.";
				return NotFound();
			}

			UserModel userModel = await _userManager.GetUserAsync(HttpContext.User);

			if (userModel.Id == postModel.UserId)
			{
				if (Media != null)
				{
					postModel.Media = Convert.ToBase64String(await GetBytes(Media));
				}
				else if (DeleteMedia)
				{
					postModel.Media = null;
				}

				if (!string.IsNullOrEmpty(Content) && Content != postModel.Content)
				{
					postModel.Content = Content;
				}

				if (!string.IsNullOrEmpty(postModel.Content) || postModel.Media != null)
				{
					postModel.IsEdited = true;
					postModel.EditedDate = DateTime.Now;

					_Models.Posts.Update(postModel);
					await _Models.SaveChangesAsync();

					TempData["Message"] = "Post successfully edited.";
				}
				else
				{
					TempData["ErrorMessage"] = "Post was not edited.";
				}
			}
			else
			{
				//TODO - Make message show without reloading the page.
				TempData["ErrorMessage"] = "Access denied, post does not belong to current user.";
			}

			postModel.UsersLikes = await GetPostLikesSelective(postModel.Id);
			postModel.Comments = await LoadComments(postModel);
			ViewData["UserId"] = userModel.Id;

			return PartialView("Post", postModel);
		}

		[HttpGet]
		public async Task<IActionResult> LookforPost(int PostId)
		{
			PostModel postModel = await _Models.Posts.AsNoTracking().FirstOrDefaultAsync(us => us.Id == PostId);

			TempData["PostId"] = PostId;

			if (postModel == null)
			{
				return NotFound();
			}

			return PartialView("EditPost", postModel);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<bool> DeletePost(int PostId)
		{
			UserModel userModel = await _userManager.GetUserAsync(HttpContext.User);
			PostModel postModel = await _Models.Posts.Include(p => p.Comments).ThenInclude(c => c.UsersLikes).Include(p => p.UsersLikes).FirstOrDefaultAsync(us => us.Id == PostId);

			if (userModel.Id != postModel.UserId)
			{
				return false;
			}

			if (postModel != null)
			{
				postModel.UsersLikes.Clear();
				foreach (CommentModel comment in postModel.Comments)
				{
					comment.UsersLikes.Clear();
				}

				_Models.Remove(postModel);
				await _Models.SaveChangesAsync();
				return true;
			}
			else
			{
				return false;
			}
		}

		[HttpPost]
		public async Task<string> LikePost(int PostId)
		{
			UserModel userModel = await _userManager.GetUserAsync(HttpContext.User);

			if (PostId < 1)
			{
				return "0";
			}

			if (userModel == null)
			{
				return "0";
			}

			List<int> exists = await _Models.Database.SqlQueryRaw<int>("SELECT PostId FROM PostLikes WHERE PostId = {0} AND UserId = {1};", PostId, userModel.Id).ToListAsync();
			
			if (exists.Count == 1)
			{
				await _Models.Database.ExecuteSqlRawAsync("UPDATE Posts SET Likes = Likes - 1 WHERE Id = {0};", PostId);
				await _Models.Database.ExecuteSqlRawAsync("DELETE FROM PostLikes WHERE  PostId = {0} AND UserId = {1};", PostId, userModel.Id);
				return "-";
			}
			else
			{
				await _Models.Database.ExecuteSqlRawAsync("UPDATE Posts SET Likes = Likes + 1 WHERE Id = {0};", PostId);
				await _Models.Database.ExecuteSqlRawAsync("INSERT INTO PostLikes (PostId, UserId) VALUES ({0}, {1});", PostId, userModel.Id);
				return "+";
			}
		}

		[HttpGet]
		public async Task<IActionResult> PostLikesTab(int postId)
		{
			List<UserModel> users = await _Models.Users.FromSqlRaw($"Select * from AspNetUsers where Id in (Select UserId from PostLikes where PostId = {postId})").AsNoTracking().ToListAsync();

			if (users.Count == 0)
			{
				return NotFound();
			}

			return PartialView("UserTabList", users);
		}

		[HttpGet]
		public async Task<IActionResult> CommentLikesTab(int commentId)
		{
			List<UserModel> users = await _Models.Users.FromSqlRaw($"Select * from AspNetUsers where Id in (Select UserId from CommentLikes where CommentId = {commentId})").AsNoTracking().ToListAsync();

			if (users.Count == 0)
			{
				return NotFound();
			}

			return PartialView("UserTabList", users);
		}

		[HttpPost]
		public IActionResult LookForCreateComment(int PostId)
		{
			TempData["PostId"] = PostId;

			return PartialView("CreateComment");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreateComment(string Content)
		{
			UserModel userModel = await _userManager.GetUserAsync(HttpContext.User);
			
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
				comment.UserId = userModel.Id;
				comment.Content = Content;

				await _Models.AddAsync(comment);
				await _Models.SaveChangesAsync();
			}

			comment.UsersLikes = new List<UserModel>();
			ViewData["UserId"] = userModel.Id;

			return PartialView("Comment", comment);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<bool> DeleteComment(int CommentId)
		{
			UserModel userModel = await _userManager.GetUserAsync(HttpContext.User);



			CommentModel comment = await _Models.Comments.Include(c => c.UsersLikes)
					.Include(c => c.Post).FirstOrDefaultAsync(c => c.Id == CommentId);

			if (userModel.Id != comment.UserId)
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
			UserModel userModel = await _userManager.GetUserAsync(HttpContext.User);

			if (CommentId <= 0)
			{
				return "0";
			}

			if (userModel == null)
			{
				return "0";
			}

			CommentModel comment = await _Models.Comments.Include(p => p.UsersLikes).FirstOrDefaultAsync(p => p.Id == CommentId);

			if (comment.UsersLikes.Contains(userModel))
			{
				comment.Likes--;
				comment.UsersLikes.Remove(userModel);
				await _Models.SaveChangesAsync();
				return "-";
			}
			else
			{
				comment.Likes++;
				comment.UsersLikes.Add(userModel);
				await _Models.SaveChangesAsync();
				return "+";
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
