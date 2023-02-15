using Microsoft.EntityFrameworkCore;
using System.Text;
using WebProject.Data;

namespace WebProject.Services
{
	public class RandomFollowers : BackgroundService
	{
		private readonly IServiceScopeFactory _serviceScopeFactory;

		private readonly PeriodicTimer _followerTimer = new(TimeSpan.FromSeconds(6));

		public RandomFollowers(IServiceScopeFactory factory)
		{
			_serviceScopeFactory = factory;
		}

		private async Task InitialSeed()
		{
			int followerCount = 0;

			using (var scope = _serviceScopeFactory.CreateScope())
			{
				var _Models = scope.ServiceProvider.GetRequiredService<WebProjectContext>();
				List<int> amountInDatabase = await _Models.Database.SqlQueryRaw<int>("SELECT COUNT(FollowerId) FROM Followers;").ToListAsync();
				followerCount = amountInDatabase.FirstOrDefault();

				if (!await _Models.Users.AsNoTracking().AnyAsync())
				{
					followerCount = -1;
				}
			}

			if (followerCount < 10 && followerCount != -1)
			{
				for (int i = 0; i < 200; i++)
				{
					await AddRandomFollower(2);
				}
			}
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			await InitialSeed();

			while (await _followerTimer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
			{
				await AddRandomFollower(2);
			}
		}

		private async Task AddRandomFollower(int count)
		{
			using (var scope = _serviceScopeFactory.CreateScope())
			{
				var _Models = scope.ServiceProvider.GetRequiredService<WebProjectContext>();

				List<string> newFollowersIds = await _Models.Database
					.SqlQueryRaw<string>($"SELECT TOP {count} Id FROM Users WHERE ProfilePicture LIKE 'https%' ORDER BY NEWID();")
					.AsNoTracking().ToListAsync();

				StringBuilder userIds = new();
				for (int i = 0; i < newFollowersIds.Count; i++)
				{
					if (i == newFollowersIds.Count - 1)
						userIds.Append($"'{newFollowersIds[i]}'");
					else
						userIds.Append($"'{newFollowersIds[i]}', ");
				}

				List<string> newCreatorIds = await _Models.Database
					.SqlQueryRaw<string>($"SELECT TOP {count} Id FROM Users WHERE Id NOT IN ({userIds}) ORDER BY NEWID();")
					.AsNoTracking().ToListAsync();

				//take the minimun to avoid exceptions of index out of range in case there few users in the database.
				int minLength = Math.Min(newFollowersIds.Count, newCreatorIds.Count);

				for (int i = 0; i < minLength; i++)
				{
					List<string> alreadyFollowing = await _Models.Database
						.SqlQueryRaw<string>("SELECT CreatorId FROM Followers WHERE CreatorId = {0} AND FollowerId = {1}", newCreatorIds[i], newFollowersIds[i])
						.AsNoTracking().ToListAsync();

					if (alreadyFollowing.Count == 0)
					{
						await _Models.Database
						.ExecuteSqlRawAsync("INSERT INTO Followers (CreatorId, FollowerId, FollowedDate) VALUES ({0}, {1}, {2})",
						newCreatorIds[i], newFollowersIds[i], DateTime.Now);
					}
					
				}

			}
		}
	}
}
