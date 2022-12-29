#nullable disable
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using WebProject.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Identity;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using WebProject.Data;

namespace WebProject.Services
{
	public class SocialMediaAlgorithm : BackgroundService
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly UserManager<UserModel> _userManager;
		private readonly IServiceScopeFactory _serviceScopeFactory;

		private readonly PeriodicTimer _userTimer = new PeriodicTimer(TimeSpan.FromSeconds(50));
		private readonly PeriodicTimer _postTimer = new PeriodicTimer(TimeSpan.FromSeconds(5));

		//TODO - add svg logos to some buttons.
		public SocialMediaAlgorithm(IHttpClientFactory httpClientFactory, UserManager<UserModel> manager, IServiceScopeFactory factory)
		{
			_httpClientFactory = httpClientFactory;
			_userManager = manager;
			_serviceScopeFactory = factory;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			//while (await _userTimer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
			//{
			//	await CreateBotFactory();
			//}

			while (await _postTimer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
			{
				await CreateRandomUserPost();
			}
		}

		public async Task CreateBotFactory()
		{
			UserModel output = new();
			HttpClient httpClient = _httpClientFactory.CreateClient();

			using (HttpResponseMessage response = await httpClient.GetAsync("https://randomuser.me/api/"))
			{
				response.EnsureSuccessStatusCode();
				string apiResponse = await response.Content.ReadAsStringAsync();

				Root root = JsonConvert.DeserializeObject<Root>(apiResponse);

				output.UserName = root.results[0].login.username;
				output.DateofBirth = root.results[0].dob.date;
				output.Email = root.results[0].email;

				output.Name = $"{root.results[0].name.first} {root.results[0].name.last}";
				output.ProfilePicture = root.results[0].picture.large;
				output.Description = $"Age: {root.results[0].dob.age} \nGender: {root.results[0].gender} \nCity: {root.results[0].location.country}, {root.results[0].location.city}";

				using (var scope = _serviceScopeFactory.CreateScope())
				{
					var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserModel>>();

					await userManager.CreateAsync(output, $"{root.results[0].login.password}.{root.results[0].login.salt}");
				}
			}
		}

		public async Task CreateRandomUserPost()
		{
			HttpClient httpClient = _httpClientFactory.CreateClient();
			PostModel randomPost = new();
			Random api = new();

			int apiToChosse = api.Next(3);

			//TODO - eliminate all the boilerplate.
			//TODO - add images to the post.
			if (apiToChosse == 0)
			{
				using (HttpResponseMessage response = await httpClient.GetAsync("https://random-data-api.com/api/hipster/random_hipster_stuff"))
				{
					response.EnsureSuccessStatusCode();
					string apiResponse = await response.Content.ReadAsStringAsync();

					HipsterText hipsterText = JsonConvert.DeserializeObject<HipsterText>(apiResponse);

					randomPost.Content = hipsterText.sentence;
					randomPost.PostDate = DateTime.Now;

					using (var scope = _serviceScopeFactory.CreateScope())
					{
						var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserModel>>();
						var _webProjectContext = scope.ServiceProvider.GetRequiredService<WebProjectContext>();
						Random index = new();

						List<UserModel> user = await userManager.Users.ToListAsync();

						randomPost.UserId = user[index.Next(user.Count)].Id;

						_webProjectContext.Posts.Add(randomPost);
						await _webProjectContext.SaveChangesAsync();
					}
				}
			}
			else if (apiToChosse == 1)
			{
				using (HttpResponseMessage response = await httpClient.GetAsync("https://favqs.com/api/qotd"))
				{
					response.EnsureSuccessStatusCode();
					string apiResponse = await response.Content.ReadAsStringAsync();

					FavQuote favQuote = JsonConvert.DeserializeObject<FavQuote>(apiResponse);

					randomPost.Content = $"{favQuote.quote.body}\n-{favQuote.quote.author}";
					randomPost.PostDate = DateTime.Now;

					using (var scope = _serviceScopeFactory.CreateScope())
					{
						var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserModel>>();
						var _webProjectContext = scope.ServiceProvider.GetRequiredService<WebProjectContext>();
						Random index = new();

						List<UserModel> user = await userManager.Users.ToListAsync();

						randomPost.UserId = user[index.Next(user.Count)].Id;

						_webProjectContext.Posts.Add(randomPost);
						await _webProjectContext.SaveChangesAsync();
					}
				}
			}
			else if (apiToChosse == 2)
			{
				using (HttpResponseMessage response = await httpClient.GetAsync("https://random-data-api.com/api/lorem_ipsum/random_lorem_ipsum"))
				{
					response.EnsureSuccessStatusCode();
					string apiResponse = await response.Content.ReadAsStringAsync();

					LoremIpsum loremIpsum = JsonConvert.DeserializeObject<LoremIpsum>(apiResponse);

					randomPost.Content = loremIpsum.very_long_sentence;
					randomPost.PostDate = DateTime.Now;

					using (var scope = _serviceScopeFactory.CreateScope())
					{
						var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserModel>>();
						var _webProjectContext = scope.ServiceProvider.GetRequiredService<WebProjectContext>();
						Random index = new();

						List<UserModel> user = await userManager.Users.ToListAsync();

						randomPost.UserId = user[index.Next(user.Count)].Id;

						_webProjectContext.Posts.Add(randomPost);
						await _webProjectContext.SaveChangesAsync();
					}
				}
			}
		}

		public async Task InitialSeed()
		{
			for (int i = 0; i < 0; i++)
			{
				await CreateBot();
			}
		}

		public async Task<UserModel> CreateBot() 
		{
			UserModel output = new();
			HttpClient httpClient = _httpClientFactory.CreateClient();

			using (HttpResponseMessage response = await httpClient.GetAsync("https://randomuser.me/api/"))
			{
				response.EnsureSuccessStatusCode();
				string apiResponse = await response.Content.ReadAsStringAsync();

				Root root = JsonConvert.DeserializeObject<Root>(apiResponse);

				output.UserName = root.results[0].login.username;
				output.DateofBirth = root.results[0].dob.date;
				output.Email = root.results[0].email;

				output.Name = $"{root.results[0].name.first} {root.results[0].name.last}";
				output.ProfilePicture = root.results[0].picture.large;
				output.Description = $"Age: {root.results[0].dob.age} \nGender: {root.results[0].gender} \nCity: {root.results[0].location.country}, {root.results[0].location.city}";

				await _userManager.CreateAsync(output, $"{root.results[0].login.password}.{root.results[0].login.salt}");
			}

			return output;
		}
	}
}
