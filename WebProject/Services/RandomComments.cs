#nullable disable
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WebProject.Data;
using WebProject.Models;

namespace WebProject.Services
{
	public class RandomComments : BackgroundService
	{
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly IHttpClientFactory _httpClientFactory;

		private readonly PeriodicTimer _commentTimer = new(TimeSpan.FromSeconds(5));

		public RandomComments(IServiceScopeFactory serviceScopeFactory, IHttpClientFactory httpClientFactory)
		{
			_serviceScopeFactory = serviceScopeFactory;
			_httpClientFactory = httpClientFactory;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			using (var scope = _serviceScopeFactory.CreateScope())
			{
				var _Models = scope.ServiceProvider.GetRequiredService<WebProjectContext>();

				if (await _Models.Comments.AsNoTracking().CountAsync() < 0)
				{
					for (int i = 0; i < 100; i++)
					{
						await CreateRandomComment();
					}
				}

			}

			while (await _commentTimer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
			{
				await CreateRandomComment();
			}
		}

		private async Task CreateRandomComment()
		{
			List<CommentModel> comments = new();
			Random rnd = new();

			switch (rnd.Next(3))
			{
				case 0:
					comments = await HipsterTextAsync();
					break;
				case 1:
					//Only returns one comment.
					comments.Add(await FavQuoteAsync());
					break;
				case 2:
					comments = await LoremIpsumAsync();
					break;
			}

			if (comments.Count <= 0)
			{
				return;
			}

			await AddCommentsToRandomPosts(comments);
		}

		private async Task AddCommentsToRandomPosts(List<CommentModel> comments)
		{
			using (var scope = _serviceScopeFactory.CreateScope())
			{
				var _Models = scope.ServiceProvider.GetRequiredService<WebProjectContext>();

				List<int> postsIds = await _Models.Database
					.SqlQueryRaw<int>($"SELECT TOP {comments.Count} Id FROM Posts ORDER BY NEWID();").ToListAsync();

				List<string> usersIds = await _Models.Database
					.SqlQueryRaw<string>($"SELECT TOP {comments.Count} Id FROM AspNetUsers WHERE ProfilePicture LIKE 'https%' ORDER BY NEWID();").AsNoTracking().ToListAsync();

				List<CommentModel> commentsToAdd = new();

				for (int i = 0; i < comments.Count; i++)
				{
					comments[i].PostId = postsIds[i];
					comments[i].UserId = usersIds[i];

					commentsToAdd.Add(comments[i]);
				}

				_Models.Comments.AddRange(commentsToAdd);
				await _Models.SaveChangesAsync();
			}
		}

		private async Task<List<CommentModel>> HipsterTextAsync()
		{
			HttpClient httpClient = _httpClientFactory.CreateClient(); 

			HttpResponseMessage response = await httpClient.GetAsync("https://random-data-api.com/api/hipster/random_hipster_stuff");
			response.EnsureSuccessStatusCode();

			string apiResponse = await response.Content.ReadAsStringAsync();

			HipsterText hipsterText = JsonConvert.DeserializeObject<HipsterText>(apiResponse);

			List<CommentModel> output = new();
			foreach (string content in hipsterText.paragraphs)
			{
				CommentModel post = new()
				{
					Content = content,
					Date = DateTime.Now
				};

				output.Add(post);
			}

			return output;
		}

		private async Task<CommentModel> FavQuoteAsync()
		{
			HttpClient httpClient = _httpClientFactory.CreateClient();

			HttpResponseMessage response = await httpClient.GetAsync("https://favqs.com/api/qotd");
			response.EnsureSuccessStatusCode();

			string apiResponse = await response.Content.ReadAsStringAsync();

			return new CommentModel()
			{
				Content = JsonConvert.DeserializeObject<FavQuote>(apiResponse).quote.body,
				Date = DateTime.Now
			};
		}

		private async Task<List<CommentModel>> LoremIpsumAsync()
		{
			HttpClient httpClient = _httpClientFactory.CreateClient();
			HttpResponseMessage response = await httpClient.GetAsync("https://random-data-api.com/api/lorem_ipsum/random_lorem_ipsum");
			response.EnsureSuccessStatusCode();

			string apiResponse = await response.Content.ReadAsStringAsync();
			LoremIpsum loremIpsum = JsonConvert.DeserializeObject<LoremIpsum>(apiResponse);

			List<CommentModel> output = new()
			{
				new CommentModel() { Content = loremIpsum.very_long_sentence, Date = DateTime.Now }
			};

			Random paragrapsToTake = new();

			for (int i = paragrapsToTake.Next(1, loremIpsum.paragraphs.Count); i < loremIpsum.paragraphs.Count; i++)
			{
				CommentModel comment = new()
				{
					Content = loremIpsum.paragraphs[i],
					Date = DateTime.Now
				};

				output.Add(comment);
			}

			return output;
		}
	}
}
