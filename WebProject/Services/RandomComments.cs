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
				var _webProjectContext = scope.ServiceProvider.GetRequiredService<WebProjectContext>();

				if (await _webProjectContext.Comments.AsNoTracking().CountAsync() <= 5)
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

		//TODO - create a Post page to show all comments.
		private async Task CreateRandomComment()
		{
			List<CommentModel> comments = new();
			Random rnd = new();

			int apiToChosse = rnd.Next(3);

			if (apiToChosse == 0)
			{
				comments = await HipsterTextAsync();
			}
			else if (apiToChosse == 1)
			{
				comments.Add(await FavQuoteAsync());
			}
			else if (apiToChosse == 2)
			{
				comments = await LoremIpsumAsync();
			}

			if (comments.Count <= 0)
			{
				return;
			}

			await AddCommentsToRandomPosts(comments);
		}

		private async Task AddCommentsToRandomPosts(List<CommentModel> comments)
		{
			List<int> postsIds = await GetRandomPostIds(comments.Count);
			List<string> usersIds = await GetRandomUserIds(comments.Count);

			using (var scope = _serviceScopeFactory.CreateScope())
			{
				var _webProjectContext = scope.ServiceProvider.GetRequiredService<WebProjectContext>();

				List<CommentModel> commentsToAdd = new();

				for (int i = 0; i < comments.Count; i++)
				{
					comments[i].PostId = postsIds[i];
					comments[i].UserId = usersIds[i];

					commentsToAdd.Add(comments[i]);
				}

				await _webProjectContext.Comments.AddRangeAsync(commentsToAdd);
				await _webProjectContext.SaveChangesAsync();
			}
		}

		private async Task<List<string>> GetRandomUserIds(int amount)
		{
			List<string> output = new();
			Random index = new();

			using (var scope = _serviceScopeFactory.CreateScope())
			{
				var _userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserModel>>();

				List<UserModel> users = await _userManager.Users.AsNoTracking().Where(u => u.ProfilePicture.StartsWith("https")).ToListAsync();

				if (amount > users.Count)
				{
					return output;
				}

				for (int i = 0; i < amount; i++)
				{
					output.Add(users[index.Next(users.Count)].Id);
				}
			}

			return output;
		}

		private async Task<List<int>> GetRandomPostIds(int amount)
		{
			List<int> output = new();
			Random index = new();

			using (var scope = _serviceScopeFactory.CreateScope())
			{
				var _webProjectContext = scope.ServiceProvider.GetRequiredService<WebProjectContext>();

				List<PostModel> posts = await _webProjectContext.Posts.AsNoTracking().ToListAsync();

				if (amount > posts.Count)
				{
					return output;
				}

				for (int i = 0; i < amount; i++)
				{
					output.Add(posts[index.Next(posts.Count)].Id);
				}
			}

			return output;
		}

		private async Task<List<CommentModel>> HipsterTextAsync()
		{
			List<CommentModel> output = new();
			HttpClient httpClient = _httpClientFactory.CreateClient(); 

			HttpResponseMessage response = await httpClient.GetAsync("https://random-data-api.com/api/hipster/random_hipster_stuff");
			response.EnsureSuccessStatusCode();

			string apiResponse = await response.Content.ReadAsStringAsync();

			HipsterText hipsterText = JsonConvert.DeserializeObject<HipsterText>(apiResponse);

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
			CommentModel output = new();

			HttpResponseMessage response = await httpClient.GetAsync("https://favqs.com/api/qotd");
			response.EnsureSuccessStatusCode();

			string apiResponse = await response.Content.ReadAsStringAsync();

			FavQuote favQuote = JsonConvert.DeserializeObject<FavQuote>(apiResponse);

			output.Content = favQuote.quote.body;
			output.Date = DateTime.Now;

			return output;
		}

		private async Task<List<CommentModel>> LoremIpsumAsync()
		{
			HttpClient httpClient = _httpClientFactory.CreateClient();
			List<CommentModel> output = new();

			HttpResponseMessage response = await httpClient.GetAsync("https://random-data-api.com/api/lorem_ipsum/random_lorem_ipsum");
			response.EnsureSuccessStatusCode();

			string apiResponse = await response.Content.ReadAsStringAsync();

			LoremIpsum loremIpsum = JsonConvert.DeserializeObject<LoremIpsum>(apiResponse);

			output.Add(new CommentModel() { Content = loremIpsum.very_long_sentence, Date = DateTime.Now });

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
