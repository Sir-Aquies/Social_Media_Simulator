#nullable disable
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using WebProject.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Identity;

namespace WebProject.Services
{
	public class SocialMediaAlgorithm
	{
		private readonly IHttpClientFactory _httpClientFactory;
		//private static readonly HttpClient httpClient = new HttpClient();
		private readonly UserManager<UserModel> userManager;

		public SocialMediaAlgorithm(IHttpClientFactory httpClientFactory, UserManager<UserModel> manager)
		{
			_httpClientFactory = httpClientFactory;
			userManager = manager;
		}

		public async Task InitialSeed()
		{
			for (int i = 0; i < 5; i++)
			{
				await CreateBot();
			}
		}
		
		//TODO - add a way to indetify the bots.
		//TODO - Add a way to show the profile picture of bots
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

				await userManager.CreateAsync(output, $"{root.results[0].login.password}.{root.results[0].login.salt}");
			}

			return output;
		}
	}
}
