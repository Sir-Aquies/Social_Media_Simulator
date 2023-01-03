#nullable disable
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using WebProject.Data;
using WebProject.Models;

namespace WebProject.Services
{
	public class RandomLikes : BackgroundService
	{
		private readonly IServiceScopeFactory _serviceScopeFactory;

		private readonly PeriodicTimer _likeTimer = new(TimeSpan.FromSeconds(2.5));

		public RandomLikes(IServiceScopeFactory factory)
		{
			_serviceScopeFactory = factory;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (await _likeTimer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
			{
				await GiveRandomPostLike();
			}
		}
		//TODO - add followers.
		//TODO - add a tab for liked post.
		//TODO - add random comments and like them.
		private async Task GiveRandomPostLike()
		{
			Random index = new();

			using (var scope = _serviceScopeFactory.CreateScope())
			{
				var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserModel>>();
				var _webProjectContext = scope.ServiceProvider.GetRequiredService<WebProjectContext>();

				List<UserModel> users = await userManager.Users.AsNoTracking().Where(u => u.ProfilePicture.StartsWith("https")).ToListAsync();
				List<PostModel> posts = await _webProjectContext.Posts.AsNoTracking().ToListAsync();

				string userId = users[index.Next(users.Count)].Id;
				UserModel user = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);

				int postId = posts[index.Next(posts.Count)].Id;
				PostModel post = await _webProjectContext.Posts.Include(u => u.User).Include(p => p.UsersLikes).FirstOrDefaultAsync(p => p.Id == postId);

				bool alreadyLiked = false;

				foreach (UserModel userModel in post.UsersLikes) 
				{
					if (userModel.Id == user.Id)
					{
						alreadyLiked = true;
					}
				}

				if (!alreadyLiked)
				{
					post.Likes++;
					post.UsersLikes.Add(user);
					await _webProjectContext.SaveChangesAsync();
				}
			}
		}
	}
}
