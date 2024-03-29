﻿#nullable disable
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WebProject.Data;
using WebProject.Models;

namespace WebProject.Services
{
	public class RandomPosts : BackgroundService
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IServiceScopeFactory _serviceScopeFactory;

		private readonly PeriodicTimer _postTimer = new(TimeSpan.FromSeconds(7));

		public RandomPosts(IHttpClientFactory httpClientFactory, IServiceScopeFactory factory)
		{
			_httpClientFactory = httpClientFactory;
			_serviceScopeFactory = factory;
		}

		private async Task InitialSeed()
		{
			int postCount = 0;

			using (var scope = _serviceScopeFactory.CreateScope())
			{
				var _Models = scope.ServiceProvider.GetRequiredService<WebProjectContext>();
				postCount = await _Models.Posts.AsNoTracking().CountAsync();

				if (!await _Models.Users.AsNoTracking().AnyAsync())
				{
					postCount = -1;
				}
			}

			if (postCount < 10 && postCount != -1)
			{
				for (int i = 0; i < 100; i++)
				{
					await CreateRandomPost();
				}
			}
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			await InitialSeed();

			while (await _postTimer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
			{
				await CreateRandomPost();
			}
		}

		public async Task CreateRandomPost()
		{
			List<PostModel> randomPosts = new();
			Random rnd = new();

			int apiToChosse = rnd.Next(3);

			switch (apiToChosse)
			{
				case 0:
					//It only returns one PostModel.
					randomPosts.Add(await FavQuote());
					break;
				case 1:
					randomPosts = await LoremIpsumAsync();
					break;
				case 2:
					//Post with null Content property, it will only have an image.
					randomPosts.Add(new PostModel() { Date = DateTime.Now });
					break;
			}

			foreach (PostModel post in randomPosts)
			{
				//50% chance of adding an image to the post or the post have null Content property.
				if (rnd.Next(2) == 1 || apiToChosse == 2)
				{
					switch (rnd.Next(10))
					{
						// 40%
						case 0:
						case 1:
						case 2:
						case 3:
							post.Media = await GetPicsum();
							break;
						// 30%
						case 4:
						case 5:
						case 6:
							post.Media = await DogImage();
							break;
						// 20%
						case 7:
						case 8:
							post.Media = $"https://source.unsplash.com/random/id={Guid.NewGuid()}";
							break;
						// 10%
						case 9:
							post.Media = await GetWaifu();
							break;
					}
				}
			}

			await AddPostToRandomUser(randomPosts);
		}

		private async Task AddPostToRandomUser(List<PostModel> randomPosts)
		{
			using (var scope = _serviceScopeFactory.CreateScope())
			{
				var _Models = scope.ServiceProvider.GetRequiredService<WebProjectContext>();

				int numberOfUsers = 0;
				int numberOfWeebs = 0;

				for (int i = 0; i < randomPosts.Count; i++)
				{
					if (randomPosts[i].Media == null)
					{
						numberOfUsers++;
						continue;
					}

					if (randomPosts[i].Media.StartsWith("https://cdn"))
					{
						numberOfWeebs++;
					}
					else
					{
						numberOfUsers++;
					}
				}

				List<string> userIds = await _Models.Database
					.SqlQueryRaw<string>($"SELECT TOP {numberOfUsers} Id FROM Users WHERE ProfilePicture LIKE 'https%' ORDER BY NEWID();")
					.AsNoTracking().ToListAsync();

				List<string> weebIds = await _Models.Database
					.SqlQueryRaw<string>($"SELECT TOP {numberOfWeebs} Id FROM Users WHERE ProfilePicture LIKE 'https://cdn%' ORDER BY NEWID();")
					.AsNoTracking().ToListAsync();

				foreach (PostModel post in randomPosts)
				{
					if (post.Media != null && post.Media.StartsWith("https://cdn"))
					{
						post.UserId = weebIds.FirstOrDefault();
						weebIds.Remove(weebIds.FirstOrDefault());
						continue;
					}

					post.UserId = userIds.FirstOrDefault();
					userIds.Remove(userIds.FirstOrDefault());
				}

				_Models.Posts.AddRange(randomPosts);
				await _Models.SaveChangesAsync();
			}
		}

		private async Task<string> GetPicsum()
		{
			HttpClient httpClient = _httpClientFactory.CreateClient();
			//Add headers parameters or else it will return access unauthorized.
			httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/html,application/xhtml+xml,application/xml");
			httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");

			//Select one random from the api from 0 to 993.
			Random index = new();
			//Use the list version of the api, because in other versions some number have a null response.
			HttpResponseMessage response = await httpClient.GetAsync($"https://picsum.photos/v2/list?page={index.Next(994)}&limit=1");
			response.EnsureSuccessStatusCode();
			string apiResponse = await response.Content.ReadAsStringAsync();

			return JsonConvert.DeserializeObject<List<Picsum>>(apiResponse)[0].download_url;
		}

		private async Task<string> DogImage()
		{
			HttpClient httpClient = _httpClientFactory.CreateClient();
			HttpResponseMessage response = await httpClient.GetAsync("https://dog.ceo/api/breeds/image/random");
			response.EnsureSuccessStatusCode();
			string apiResponse = await response.Content.ReadAsStringAsync();

			return JsonConvert.DeserializeObject<DogAPI>(apiResponse).message;
		}

		private async Task<string> GetWaifu()
		{
			HttpClient httpClient = _httpClientFactory.CreateClient();

			HttpResponseMessage response = await httpClient.GetAsync("https://api.waifu.im/search");
			response.EnsureSuccessStatusCode();

			string apiResponse = await response.Content?.ReadAsStringAsync();

			return JsonConvert.DeserializeObject<Waifu>(apiResponse).images[0].url;
		}

		private async Task<PostModel> FavQuote()
		{
			HttpClient httpClient = _httpClientFactory.CreateClient();
			HttpResponseMessage response = await httpClient.GetAsync("https://favqs.com/api/qotd");
			response.EnsureSuccessStatusCode();

			string apiResponse = await response.Content.ReadAsStringAsync();
			FavQuote favQuote = JsonConvert.DeserializeObject<FavQuote>(apiResponse);

			PostModel output = new()
			{
				Content = $"{favQuote.quote.body}\n-{favQuote.quote.author}",
				Date = DateTime.Now
			};

			return output;
		}

		private async Task<List<PostModel>> LoremIpsumAsync()
		{
			HttpClient httpClient = _httpClientFactory.CreateClient();
			List<PostModel> output = new();

			HttpResponseMessage response = await httpClient.GetAsync("https://random-data-api.com/api/lorem_ipsum/random_lorem_ipsum");
			response.EnsureSuccessStatusCode();

			string apiResponse = await response.Content.ReadAsStringAsync();

			LoremIpsum loremIpsum = JsonConvert.DeserializeObject<LoremIpsum>(apiResponse);

			output.Add(new PostModel() { Content = loremIpsum.very_long_sentence, Date = DateTime.Now });

			Random paragraphsToTake = new();

			//Create post with different Content length.
			PostModel post = new();
			for (int i = paragraphsToTake.Next(1, loremIpsum.paragraphs.Count); i < loremIpsum.paragraphs.Count; i++)
			{
				post.Content += loremIpsum.paragraphs[i];
			}
			post.Date = DateTime.Now;

			output.Add(post);

			return output;
		}
	}
}
