#nullable disable
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.ComponentModel.Design;
using WebProject.Data;
using WebProject.Models;
using static System.Formats.Asn1.AsnWriter;

namespace WebProject.Services
{
	public class RandomLikes : BackgroundService
	{
		private readonly IServiceScopeFactory _serviceScopeFactory;

		private readonly PeriodicTimer _likeTimer = new(TimeSpan.FromSeconds(1.5));

		public RandomLikes(IServiceScopeFactory factory)
		{
			_serviceScopeFactory = factory;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			//int likesAmount = 0;

			//using (var scope = _serviceScopeFactory.CreateScope())
			//{
			//	var _Models = scope.ServiceProvider.GetRequiredService<WebProjectContext>();
			//	likesAmount = await _Models.Database.SqlQueryRaw<int>("SELECT COUNT(CreatorId) FROM FollowUsers").FirstOrDefaultAsync();
			//}

			//if (likesAmount == 0)
			//{
			//	for (int i = 0;)
			//}

			while (await _likeTimer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
			{
				await LikeRandomPosts(2);
				await LikeRandomComments(2);
			}
		}

		//TODO - add a responsive top bar.
		private async Task LikeRandomPosts(int count)
		{
			using (var scope = _serviceScopeFactory.CreateScope())
			{
				var _Models = scope.ServiceProvider.GetRequiredService<WebProjectContext>();

				List<string> userIds = await _Models.Database
					.SqlQueryRaw<string>($"SELECT TOP {count} Id FROM Users WHERE ProfilePicture LIKE 'https%' ORDER BY NEWID();")
					.AsNoTracking().ToListAsync();

				List<int> postIds = await _Models.Database
					.SqlQueryRaw<int>($"SELECT TOP {count} Id FROM Posts ORDER BY NEWID();").ToListAsync();

				for (int i = 0; i < userIds.Count; i++)
				{
					List<int> rowExists = await _Models.Database
						.SqlQueryRaw<int>("SELECT PostId FROM PostLikes WHERE PostId = {0} AND UserId = {1};", postIds[i], userIds[i]).ToListAsync();

					if (rowExists.Count == 0) {
						int affectedRows = await _Models.Database.ExecuteSqlRawAsync("UPDATE Posts SET Likes = Likes + 1 WHERE Id = {0};", postIds[i]);

						if (affectedRows == 1)
							await _Models.Database
								.ExecuteSqlRawAsync("INSERT INTO PostLikes (PostId, UserId, LikedDate) VALUES ({0}, {1}, {2});", postIds[i], userIds[i], DateTime.Now);
					}
				}
			}
		}

		private async Task LikeRandomComments(int count)
		{
			using (var scope = _serviceScopeFactory.CreateScope())
			{
				var _Models = scope.ServiceProvider.GetRequiredService<WebProjectContext>();

				List<string> userIds = await _Models.Database
					.SqlQueryRaw<string>($"SELECT TOP {count} Id FROM Users WHERE ProfilePicture LIKE 'https%' ORDER BY NEWID();")
					.AsNoTracking().ToListAsync();

				List<int> commentIds = await _Models.Database
					.SqlQueryRaw<int>($"SELECT TOP {count} Id FROM Comments ORDER BY NEWID();").ToListAsync();

				for (int i = 0; i < userIds.Count; i++)
				{
					List<int> rowExists = await _Models.Database
						.SqlQueryRaw<int>("SELECT CommentId FROM CommentLikes WHERE CommentId = {0} AND UserId = {1};", commentIds[i], userIds[i]).ToListAsync();

					if (rowExists.Count == 0)
					{
						int affectedRows = await _Models.Database.ExecuteSqlRawAsync("UPDATE Comments SET Likes = Likes + 1 WHERE Id = {0};", commentIds[i]);

						if (affectedRows == 1)
							await _Models.Database
								.ExecuteSqlRawAsync("INSERT INTO CommentLikes (CommentId, UserId, LikedDate) VALUES ({0}, {1}, {2});", commentIds[i], userIds[i], DateTime.Now);
					}
				}
			}
		}
	}
}
