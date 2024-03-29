﻿#nullable disable
using Microsoft.EntityFrameworkCore;
using WebProject.Data;

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

		private async Task InitiaSeed()
		{
			int likesCount = 0;

			using (var scope = _serviceScopeFactory.CreateScope())
			{
				var _Models = scope.ServiceProvider.GetRequiredService<WebProjectContext>();
				List<int> amountInDatabase = await _Models.Database.SqlQueryRaw<int>("SELECT COUNT(PostId) FROM PostLikes").ToListAsync();
				likesCount = amountInDatabase.FirstOrDefault();

				if (!await _Models.Users.AsNoTracking().AnyAsync())
				{
					likesCount = -1;
				}
			}

			if (likesCount < 10 && likesCount != -1)
			{
				for (int i = 0; i < 500; i++)
				{
					await LikeRandomPosts(2);
					await LikeRandomComments(2);
				}
			}
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			await InitiaSeed();

			while (await _likeTimer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
			{
				await LikeRandomPosts(3);
				await LikeRandomComments(3);
			}
		}

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
