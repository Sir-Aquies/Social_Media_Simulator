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
			Random rnd = new();

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
			HttpClient httpClient = _httpClientFactory.CreateClient();

			using HttpResponseMessage response = await httpClient.GetAsync("https://random-data-api.com/api/v2/users");
			response.EnsureSuccessStatusCode();
			string apiResponse = await response.Content.ReadAsStringAsync();

			ClassicUser classicUser = JsonConvert.DeserializeObject<ClassicUser>(apiResponse);

			UserModel newUser = new()
			{
				UserName = classicUser.username,
				DateofBirth = DateTime.Parse(classicUser.date_of_birth),
				Email = classicUser.email,

				Name = $"{classicUser.first_name} {classicUser.last_name}",
				Description = $"{classicUser.gender} \n{classicUser.employment.title}, {classicUser.employment.key_skill} \n{classicUser.address.state}, {classicUser.address.country}"
			};

			Random rnd = new();
			
			switch (rnd.Next(3))
			{
				case 0:
					newUser.ProfilePicture = await GetWaifu();
					break;

				case 1:
				case 2:
					newUser.ProfilePicture = await GetPicsum();
					break;
			}

			using (var scope = _serviceScopeFactory.CreateScope())
			{
				UserManager<UserModel> userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserModel>>();

				//Verify that no other user has the same profile picture or username.
				//If there is don't save the new user in the database
				foreach (UserModel u in userManager.Users.AsNoTracking())
				{
					if (u.UserName == newUser.UserName)
					{
						return;
					}
				}

				await userManager.CreateAsync(newUser, $"{classicUser.password}.{classicUser.id}");
			}
		}

		public async Task CreateNormalUser()
		{
			HttpClient httpClient = _httpClientFactory.CreateClient();

			using HttpResponseMessage response = await httpClient.GetAsync("https://randomuser.me/api/");
			response.EnsureSuccessStatusCode();
			string apiResponse = await response.Content.ReadAsStringAsync();

			NormalUser normalUser = JsonConvert.DeserializeObject<NormalUser>(apiResponse);

			UserModel newUser = new()
			{
				UserName = normalUser.results[0].login.username,
				DateofBirth = normalUser.results[0].dob.date,
				Email = normalUser.results[0].email,

				Name = $"{normalUser.results[0].name.first} {normalUser.results[0].name.last}",
				ProfilePicture = normalUser.results[0].picture.large,
				Description = $"Age: {normalUser.results[0].dob.age} \nGender: {normalUser.results[0].gender} \nCity: {normalUser.results[0].location.country}, {normalUser.results[0].location.city}"
			};

			using (var scope = _serviceScopeFactory.CreateScope())
			{
				UserManager<UserModel> userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserModel>>();

				//Verify that no other user has the same profile picture or username.
				//If there is don't save the new user in the database
				foreach (UserModel u in userManager.Users.AsNoTracking())
				{
					if (u.ProfilePicture == u.ProfilePicture || u.UserName == u.UserName)
					{
						return;
					}
				}

				await userManager.CreateAsync(newUser, $"{normalUser.results[0].login.password}.{normalUser.results[0].login.salt}");
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
			string url = $"https://picsum.photos/v2/list?page={index.Next(994)}&limit=1";

			HttpResponseMessage response = await httpClient.GetAsync(url);

			response.EnsureSuccessStatusCode();
			string apiResponse = await response.Content.ReadAsStringAsync();

			return JsonConvert.DeserializeObject<List<Picsum>>(apiResponse)[0].download_url;
		}

		private async Task<string> GetWaifu()
		{
			HttpClient httpClient = _httpClientFactory.CreateClient();

			HttpResponseMessage response = await httpClient.GetAsync("https://api.waifu.im/search");
			response.EnsureSuccessStatusCode();

			string apiResponse = await response.Content?.ReadAsStringAsync();

			return JsonConvert.DeserializeObject<Waifu>(apiResponse).images[0].url;
		}

		public async Task InitialSeed()
		{
			if (_userManager.Users.AsNoTracking().Count() < 10)
			{
				for (int i = 0; i < 25; i++)
				{
					await CreateNormalUser();
				}
			}
			
		}
	}
}
