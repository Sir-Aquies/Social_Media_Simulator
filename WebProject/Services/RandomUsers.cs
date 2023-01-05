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
using System.Reflection.Metadata;
using System.Security.Policy;

namespace WebProject.Services
{
	public class RandomUsers : BackgroundService
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly UserManager<UserModel> _userManager;
		private readonly IServiceScopeFactory _serviceScopeFactory;

		private readonly PeriodicTimer _userTimer = new(TimeSpan.FromSeconds(60));

		public RandomUsers(IHttpClientFactory httpClientFactory, UserManager<UserModel> manager, IServiceScopeFactory factory)
		{
			_httpClientFactory = httpClientFactory;
			_userManager = manager;
			_serviceScopeFactory = factory;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			await InitialSeed();

			while (await _userTimer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
			{
				await CreateRandomUser();
			}
		}

		private async Task CreateRandomUser()
		{
			Random rnd = new Random();

			switch (rnd.Next(2))
			{
				case 0:
					await CreateClassicUser();
					break;
				case 1:
					await CreateNormalUser();
					break;
			}
		}

		private async Task CreateClassicUser()
		{
			UserModel output = new();
			HttpClient httpClient = _httpClientFactory.CreateClient();

			using HttpResponseMessage response = await httpClient.GetAsync("https://random-data-api.com/api/v2/users");
			response.EnsureSuccessStatusCode();
			string apiResponse = await response.Content.ReadAsStringAsync();

			ClassicUser user = JsonConvert.DeserializeObject<ClassicUser>(apiResponse);

			output.UserName = user.username;
			output.DateofBirth = DateTime.Parse(user.date_of_birth);
			output.Email = user.email;

			output.Name = $"{user.first_name} {user.last_name}";
			output.Description = $"{user.gender} \n{user.employment.title}, {user.employment.key_skill} \n{user.address.state}, {user.address.country}";

			Random rnd = new();
			
			switch (rnd.Next(2))
			{
				case 0:
					output.ProfilePicture = await GetWaifu();
					break;
				case 1:
					output.ProfilePicture = await GetPicsum();
					break;
			}

			using (var scope = _serviceScopeFactory.CreateScope())
			{
				UserManager<UserModel> userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserModel>>();

				foreach (UserModel u in userManager.Users.AsNoTracking())
				{
					if (u.UserName == output.UserName)
					{
						return;
					}
				}

				await userManager.CreateAsync(output, $"{user.password}.{user.id}");
			}
		}

		public async Task CreateNormalUser()
		{
			UserModel output = new();
			HttpClient httpClient = _httpClientFactory.CreateClient();

			using HttpResponseMessage response = await httpClient.GetAsync("https://randomuser.me/api/");
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
				UserManager<UserModel> userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserModel>>();

				foreach (UserModel user in userManager.Users.AsNoTracking())
				{
					if (user.ProfilePicture == output.ProfilePicture || user.UserName == output.UserName)
					{
						return;
					}
				}

				await userManager.CreateAsync(output, $"{root.results[0].login.password}.{root.results[0].login.salt}");
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

			HttpResponseMessage response = await httpClient.GetAsync(url);

			response.EnsureSuccessStatusCode();
			string apiResponse = await response.Content.ReadAsStringAsync();

			Picsum picsum = JsonConvert.DeserializeObject<List<Picsum>>(apiResponse)[0];

			output = picsum.download_url;

			return output;
		}

		private async Task<string> GetWaifu()
		{
			string output = string.Empty;

			HttpClient httpClient = _httpClientFactory.CreateClient();

			HttpResponseMessage response = await httpClient.GetAsync("https://api.waifu.im/search");
			response.EnsureSuccessStatusCode();

			string apiResponse = await response.Content?.ReadAsStringAsync();

			Waifu waifu = JsonConvert.DeserializeObject<Waifu>(apiResponse);

			output = waifu.images[0].url;

			return output;
		}

		public async Task InitialSeed()
		{
			if (_userManager.Users.AsNoTracking().Count() < 10)
			{
				for (int i = 0; i < 25; i++)
				{
					await CreateBot();
				}
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
