#nullable disable
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using WebProject.Data;
using WebProject.Models;

namespace WebProject.Components
{
	public class TendencyUsers : ViewComponent
	{
		private readonly UserManager<UserModel> _userManager;
		private readonly WebProjectContext _Models;

		public TendencyUsers(UserManager<UserModel> userManager, WebProjectContext Models)
		{
			_userManager = userManager;
			_Models = Models;
		}

		//TODO - Move this logic to a service.
		//TODO - Found a way to measure the speed an this way is faster, but by a milisecond so query the results.
		public async Task<IViewComponentResult> InvokeAsync(int amount)
		{
			//First, pass the list of users and post from the databse to GiveUsersThierPosts who will return a list of users with their post.
			//Second, CountTotalLikes will return a list of key value pair that includes the user and the total amount of likes.
			//Third, GetUsersFromKeyValuePair will order the return a list of users with the most likes.
			return View("UsersList", GetUsersFromKeyValuePair(CountTotalLikes(GiveUsersThierPosts(
					await _userManager.Users.Select(u => new UserModel { Id = u.Id, UserName = u.UserName, ProfilePicture = u.ProfilePicture }).AsNoTracking().ToListAsync(), 
					await _Models.Posts.Select(p => new PostModel { UserId = p.UserId, Likes = p.Likes }).AsNoTracking().ToListAsync())), amount));
		}

		//Get the first users from the already Ordered list of KeyValuePair
		public List<UserModel> GetUsersFromKeyValuePair(IEnumerable<KeyValuePair<UserModel, int>> likesAndUsers, int amount)
		{
			List<UserModel> output = new();
			int count = 0;
			//Order likesAndUsers from the users with most likes to the least and loop a span from the list.
			foreach (KeyValuePair<UserModel, int> userLikes in CollectionsMarshal.AsSpan(likesAndUsers.OrderByDescending(u => u.Value).ToList()))
			{
				output.Add(userLikes.Key);
				count++;

				if (count == amount) break;
			}

			return output;
		}

		public List<KeyValuePair<UserModel, int>> CountTotalLikes(List<UserModel> users)
		{
			List<KeyValuePair<UserModel, int>> output = new();

			//Get the span from the user list
			Span<UserModel> spanUsers = CollectionsMarshal.AsSpan(users);
			
			for (int i = 0; i < spanUsers.Length; i++)
			{
				//Get the user from the span of users.
				UserModel user = spanUsers[i];
				int totalLikes = 0;

				//Get the span from the user's posts.
				Span<PostModel> spanPosts = CollectionsMarshal.AsSpan(user.Posts.ToList());

				for (int j = 0; j < spanPosts.Length; j++)
				{
					//Count the likes of every post.
					totalLikes += spanPosts[j].Likes;
				}

				//Add the user and the total amount of likes to a key value pair list.
				output.Add(new KeyValuePair<UserModel, int>(user, totalLikes));
			}

			return output;
		}

		public List<UserModel> GiveUsersThierPosts(List<UserModel> users, List<PostModel> posts)
		{
			//Get a span from the users and posts list.
			Span<PostModel> spanPosts = CollectionsMarshal.AsSpan<PostModel>(posts);
			Span<UserModel> spanUsers = CollectionsMarshal.AsSpan<UserModel>(users);

			for (int i = 0; i < spanPosts.Length; i++)
			{
				for (int j = 0; j < spanUsers.Length; j++)
				{
					//If the user's posts property in null set it to a new list.
					if (spanUsers[j].Posts == null) spanUsers[j].Posts = new List<PostModel>();

					//If the userId of the post is the id of the user, add the post to the user's posts.
					if (spanPosts[i].UserId == spanUsers[j].Id)
					{
						spanUsers[j].Posts.Add(spanPosts[i]);
					}
				}
			}

			return users;
		}
	}
}
