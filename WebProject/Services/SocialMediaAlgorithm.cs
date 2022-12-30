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
	public class SocialMediaAlgorithm : BackgroundService
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly UserManager<UserModel> _userManager;
		private readonly IServiceScopeFactory _serviceScopeFactory;

		private readonly PeriodicTimer _userTimer = new(TimeSpan.FromSeconds(50));

		//TODO - add svg logos to some buttons.
		public SocialMediaAlgorithm(IHttpClientFactory httpClientFactory, UserManager<UserModel> manager, IServiceScopeFactory factory)
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
				await CreateBotFactory();
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
