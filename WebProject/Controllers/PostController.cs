#nullable disable
using Microsoft.AspNetCore.Mvc;
using WebProject.Models;
using WebProject.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace WebProject.Controllers
{
	[Authorize]
	public class PostController : Controller
	{
		private readonly WebProjectContext _Models;
		private readonly UserManager<UserModel> userManager;

		public PostController(WebProjectContext Models, UserManager<UserModel> manager)
		{
			_Models = Models;
			userManager = manager;
		}

		public IActionResult LookForCreatePost() => PartialView("CreatePost");

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreatePost(IFormFile Media, string Content)
		{
			UserModel userModel = await userManager.GetUserAsync(HttpContext.User);

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
				_Models.Posts.Add(postModel);
				await _Models.SaveChangesAsync();
			}

			UserModel page = userModel;
			page.Posts = await GetPosts(userModel);

			userModel.LikedPost = GetPostsLiked(page.Posts, userModel.Id);
			userModel.LikedComments = GetCommentsLiked(page.Posts, userModel.Id);

			DynamicUser dynamic = new()
			{ 
				User = userModel,
				PageUser = page
			};

			return PartialView("UserPost", dynamic);
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
				return NotFound();
			}

			UserModel userModel = await userManager.GetUserAsync(HttpContext.User);

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

			UserModel page = userModel;
			page.Posts = await GetPosts(page);

			userModel.LikedPost = GetPostsLiked(page.Posts, userModel.Id);
			userModel.LikedComments = GetCommentsLiked(page.Posts, userModel.Id);

			DynamicUser dynamic = new()
			{
				User = userModel,
				PageUser = page
			};

			return PartialView("UserPost", dynamic);
		}

		[HttpPost]
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
			UserModel userModel = await userManager.GetUserAsync(HttpContext.User);
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
			UserModel userModel = await userManager.GetUserAsync(HttpContext.User);

			if (PostId <= 0)
			{
				return "0";
			}

			if (userModel == null)
			{
				return "0";
			}

			PostModel post = await _Models.Posts.Include(p => p.UsersLikes).FirstOrDefaultAsync(p => p.Id == PostId);

			if (post == null)
			{
				return "0";
			}

			if (post.UsersLikes.Contains(userModel))
			{
				post.Likes--;
				post.UsersLikes.Remove(userModel);
				await _Models.SaveChangesAsync();
				return "-";
			}
			else
			{
				post.Likes++;
				post.UsersLikes.Add(userModel);
				await _Models.SaveChangesAsync();
				return "+";
			}
		}

		[HttpPost]
		public IActionResult LookForCreateComment(int PostId, string UserName)
		{
			TempData["PostId"] = PostId;
			TempData["UserName"] = UserName;

			return PartialView("CreateComment");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreateComment(string Content)
		{
			UserModel userModel = await userManager.GetUserAsync(HttpContext.User);

			string UserName = "";
			int PostId = 0;
			bool PostIdBool = false;

			if (!string.IsNullOrEmpty(TempData["PostId"]?.ToString()) && !string.IsNullOrEmpty(TempData["UserName"]?.ToString()))
			{
				UserName = TempData["UserName"]?.ToString() ?? "empty";
				PostIdBool = int.TryParse(TempData["PostId"]?.ToString(), out PostId);
			}

			if (!string.IsNullOrEmpty(Content) && PostIdBool && !string.IsNullOrEmpty(UserName))
			{
				CommentModel comment = new()
				{
					Content = Content,
					UserId = userModel.Id,
					PostId = PostId,
					Date = DateTime.Now,
				};

				_Models.Add(comment);
				await _Models.SaveChangesAsync();
			}

			UserModel page = await userManager.FindByNameAsync(UserName);
			page.Posts = await GetPosts(page);

			userModel.LikedPost = GetPostsLiked(page.Posts, userModel.Id);
			userModel.LikedComments = GetCommentsLiked(page.Posts, userModel.Id);

			DynamicUser dynamic = new()
			{
				User = userModel,
				PageUser = page
			};

			return PartialView("UserPost", dynamic);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<bool> DeleteComment(int CommentId)
		{
			UserModel userModel = await userManager.GetUserAsync(HttpContext.User);

			CommentModel comment = await _Models.Comments.Include(c => c.UsersLikes)
					.Include(c => c.Post).ThenInclude(p => p.User).FirstOrDefaultAsync(c => c.Id == CommentId);

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
			UserModel userModel = await userManager.GetUserAsync(HttpContext.User);

			if (userModel != null && CommentId != 0)
			{
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

			return "0";
		}

		public List<PostModel> GetPostsLiked(IList<PostModel> posts, string userId)
		{
			List<PostModel> postsLiked = new();

			foreach (PostModel p in posts)
			{
				foreach (UserModel u in p.UsersLikes)
				{
					if (u.Id == userId)
					{
						postsLiked.Add(p);
					}
				}
			}

			return postsLiked;
		}

		public List<CommentModel> GetCommentsLiked(IList<PostModel> posts, string userId)
		{
			List<CommentModel> commentsLiked = new();

			foreach (PostModel p in posts)
			{
				foreach (CommentModel c in p.Comments)
				{
					foreach (UserModel u in c.UsersLikes)
					{
						if (u.Id == userId)
						{
							commentsLiked.Add(c);
						}
					}
				}
			}

			return commentsLiked;
		}

		public async Task<List<PostModel>> GetPosts(UserModel user)
		{
			List<PostModel> output = new();

			foreach (PostModel p in _Models.Posts.AsNoTracking())
			{
				PostModel post = p;

				if (post.UserId == user.Id)
				{
					post = await _Models.Posts.Include(p => p.UsersLikes).AsNoTracking().FirstOrDefaultAsync(p => p.Id == post.Id);
					post.User = user;
					post.Comments = await LoadComments(post);
					output.Add(post);
				}
			}

			return output;
		}

		public async Task<List<CommentModel>> LoadComments(PostModel post)
		{
			List<CommentModel> output = new();

			foreach (CommentModel c in _Models.Comments.AsNoTracking())
			{
				CommentModel comment = c;

				if (comment.PostId == post.Id)
				{
					comment = await _Models.Comments.Include(c => c.User).Include(c => c.UsersLikes).FirstOrDefaultAsync(c => c.Id == comment.Id);
					comment.Post = post;
					output.Add(comment);
				}
			}

			return output;
		}

		private async Task<byte[]> GetBytes(IFormFile formFile)
		{
			await using var memoryStream = new MemoryStream();
			await formFile.CopyToAsync(memoryStream);
			return memoryStream.ToArray();
		}
	}
}
