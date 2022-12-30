#nullable disable
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WebProject.Data;
using WebProject.Models;

namespace WebProject.Services
{
	public class RandomPost : BackgroundService
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IServiceScopeFactory _serviceScopeFactory;

		private readonly PeriodicTimer _postTimer = new(TimeSpan.FromSeconds(10));

		public RandomPost(IHttpClientFactory httpClientFactory, IServiceScopeFactory factory)
		{
			_httpClientFactory = httpClientFactory;
			_serviceScopeFactory = factory;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (await _postTimer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
			{
				await CreateRandomUserPost();
			}
		}

		public async Task CreateRandomUserPost()
		{
			List<PostModel> randomPosts = new();
			Random rnd = new();

			int apiToChosse = rnd.Next(4);

			if (apiToChosse == 0)
			{
				randomPosts = await HipsterTextAsync();
			}
			else if (apiToChosse == 1)
			{
				randomPosts.Add(await FavQuoteAsync());
			}
			else if (apiToChosse == 2)
			{
				randomPosts = await LoremIpsumAsync();
			}
			else if (apiToChosse == 3)
			{
				randomPosts.Add(new PostModel() { PostDate = DateTime.Now });
			}

			foreach (PostModel post in randomPosts)
			{
				if (rnd.Next(2) == 1 || apiToChosse == 3)
				{
					string media = rnd.Next(3) switch
					{
						0 => "https://source.unsplash.com/random",
						1 => await DogImage(),
						2 => await GetPicsum(),
						_ => null
					};

					post.Media = media;
				}
			}

			await AdPostToRandomUser(randomPosts);
		}

		private async Task AdPostToRandomUser(List<PostModel> randomPosts)
		{
			using (var scope = _serviceScopeFactory.CreateScope())
			{
				var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserModel>>();
				var _webProjectContext = scope.ServiceProvider.GetRequiredService<WebProjectContext>();

				Random index = new();
				List<UserModel> user = await userManager.Users.AsNoTracking().Where(u => u.ProfilePicture.StartsWith("https")).ToListAsync();

				foreach (PostModel post in randomPosts)
				{
					post.UserId = user[index.Next(user.Count)].Id;

					_webProjectContext.Posts.Add(post);
					await _webProjectContext.SaveChangesAsync();
				}
			}
		}

		private async Task<string> GetPicsum()
		{
			string output = string.Empty;
			HttpClient httpClient = _httpClientFactory.CreateClient();
			Random index = new();
			httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/html,application/xhtml+xml,application/xml");
			httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");

			string url = $"https://picsum.photos/v2/list?page={index.Next(994)}&limit=1";

			using (HttpResponseMessage response = await httpClient.GetAsync(url))
			{
				response.EnsureSuccessStatusCode();
				string apiResponse = await response.Content.ReadAsStringAsync();

				Picsum picsum = JsonConvert.DeserializeObject<List<Picsum>>(apiResponse)[0];

				output = picsum.download_url;
			}

			return output;
		}

		private async Task<string> DogImage()
		{
			string output = string.Empty;
			HttpClient httpClient = _httpClientFactory.CreateClient();

			using (HttpResponseMessage response = await httpClient.GetAsync("https://dog.ceo/api/breeds/image/random"))
			{
				response.EnsureSuccessStatusCode();
				string apiResponse = await response.Content.ReadAsStringAsync();

				DogAPI dog = JsonConvert.DeserializeObject<DogAPI>(apiResponse);
				output = dog.message;
			}

			return output;
		}

		private async Task<List<PostModel>> HipsterTextAsync()
		{
			List<PostModel> output = new();
			HttpClient httpClient = _httpClientFactory.CreateClient();

			using (HttpResponseMessage response = await httpClient.GetAsync("https://random-data-api.com/api/hipster/random_hipster_stuff"))
			{
				response.EnsureSuccessStatusCode();
				string apiResponse = await response.Content.ReadAsStringAsync();

				HipsterText hipsterText = JsonConvert.DeserializeObject<HipsterText>(apiResponse);

				foreach (string content in hipsterText.paragraphs)
				{
					PostModel post = new();

					post.Content = content;
					post.PostDate = DateTime.Now;

					output.Add(post);
				}
			}

			return output;
		}

		private async Task<PostModel> FavQuoteAsync()
		{
			HttpClient httpClient = _httpClientFactory.CreateClient();
			PostModel output = new();

			using (HttpResponseMessage response = await httpClient.GetAsync("https://favqs.com/api/qotd"))
			{
				response.EnsureSuccessStatusCode();
				string apiResponse = await response.Content.ReadAsStringAsync();

				FavQuote favQuote = JsonConvert.DeserializeObject<FavQuote>(apiResponse);

				output.Content = $"{favQuote.quote.body}\n-{favQuote.quote.author}";
				output.PostDate = DateTime.Now;
			}

			return output;
		}

		private async Task<List<PostModel>> LoremIpsumAsync()
		{
			HttpClient httpClient = _httpClientFactory.CreateClient();
			List<PostModel> output = new();

			using (HttpResponseMessage response = await httpClient.GetAsync("https://random-data-api.com/api/lorem_ipsum/random_lorem_ipsum"))
			{
				response.EnsureSuccessStatusCode();
				string apiResponse = await response.Content.ReadAsStringAsync();

				LoremIpsum loremIpsum = JsonConvert.DeserializeObject<LoremIpsum>(apiResponse);

				output.Add(new PostModel() { Content = loremIpsum.very_long_sentence, PostDate = DateTime.Now });

				Random paragrapsToTake = new();

				PostModel post = new();
				for (int i = paragrapsToTake.Next(1, loremIpsum.paragraphs.Count); i < loremIpsum.paragraphs.Count; i++)
				{
					post.Content += loremIpsum.paragraphs[i];
				}
				post.PostDate = DateTime.Now;
			}

			return output;
		}
	}
}
