#nullable disable
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.ComponentModel.Design;
using WebProject.Data;
using WebProject.Models;

namespace WebProject.Services
{
	public class RandomLikes : BackgroundService
	{
		private readonly IServiceScopeFactory _serviceScopeFactory;

		private readonly PeriodicTimer _likeTimer = new(TimeSpan.FromSeconds(1));

		public RandomLikes(IServiceScopeFactory factory)
		{
			_serviceScopeFactory = factory;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (await _likeTimer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
			{
				await GiveRandomPostLike(3);
				await GiveRandomCommentLike(3);
			}
		}

		//TODO - add followers.
		//TODO - add a trending page.
		//TODO - add a responsive top bar.
		private async Task GiveRandomPostLike(int count)
		{
			Random index = new();

			using (var scope = _serviceScopeFactory.CreateScope())
			{
				var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserModel>>();
				var _webProjectContext = scope.ServiceProvider.GetRequiredService<WebProjectContext>();

				List<UserModel> users = await userManager.Users.AsNoTracking().Where(u => u.ProfilePicture.StartsWith("https")).ToListAsync();
				List<PostModel> posts = await _webProjectContext.Posts.AsNoTracking().ToListAsync();

				List<string> userIds = new();

				for (int i = 0; i < count; i++)
				{
					userIds.Add(users[index.Next(users.Count)].Id);
				}


				List<int> postIds = new();

				for (int i = 0; i < count; i++)
				{
					postIds.Add(posts[index.Next(posts.Count)].Id);
				}

				for (int i = 0; i < count; i++)
				{
					UserModel user = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userIds[i]);
					PostModel post = await _webProjectContext.Posts.Include(p => p.UsersLikes).FirstOrDefaultAsync(p => p.Id == postIds[i]);

					bool alreadyLiked = false;

					foreach (UserModel u in post.UsersLikes)
					{
						if (u.Id == user.Id)
						{
							alreadyLiked = true;
							break;
						}
					}

					if (!alreadyLiked)
					{
						post.Likes++;
						post.UsersLikes.Add(user);
					}
				}

				await _webProjectContext.SaveChangesAsync();
			}
		}

		private async Task GiveRandomCommentLike(int count)
		{
			Random index = new();

			using (var scope = _serviceScopeFactory.CreateScope())
			{
				var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserModel>>();
				var _webProjectContext = scope.ServiceProvider.GetRequiredService<WebProjectContext>();

				List<UserModel> users = await userManager.Users.AsNoTracking().Where(u => u.ProfilePicture.StartsWith("https")).ToListAsync();
				List<CommentModel> comments = await _webProjectContext.Comments.AsNoTracking().ToListAsync();

				List<string> userIds = new();
				List<int> commentIds = new();

				for (int i = 0; i < count; i++)
				{
					userIds.Add(users[index.Next(users.Count)].Id);
					commentIds.Add(comments[index.Next(comments.Count)].Id);
				}

				for (int i = 0; i < count; i++)
				{
					UserModel user = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userIds[i]);
					CommentModel comment = await _webProjectContext.Comments.Include(c => c.UsersLikes).FirstOrDefaultAsync(p => p.Id == commentIds[i]);

					bool alreadyLiked = false;

					foreach (UserModel u in comment.UsersLikes)
					{
						if (u.Id == user.Id)
						{
							alreadyLiked = true;
							break;
						}
					}

					if (!alreadyLiked)
					{
						comment.Likes++;
						comment.UsersLikes.Add(user);
					}
				}

				await _webProjectContext.SaveChangesAsync();
			}
		}
	}
}
