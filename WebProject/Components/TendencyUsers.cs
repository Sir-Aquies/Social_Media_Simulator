#nullable disable
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

		//Move this logic to a service.
		public async Task<IViewComponentResult> InvokeAsync(int amount)
		{
			List<UserModel> usersWithMostLikes = await GetPosts(await _userManager.Users.AsNoTracking().ToListAsync());
			int length = usersWithMostLikes.Count;

			Dictionary<UserModel, int> likesAndUser = new();

			for (int i = 0; i < length; i++)
			{
				UserModel user = usersWithMostLikes[i];
				int postLength = user.Posts.Count;
				int totalLikes = 0;

				for (int j = 0; j < postLength; j++)
				{
					totalLikes += user.Posts[j].Likes;
				}

				likesAndUser.Add(user, totalLikes);
			}

			usersWithMostLikes.Clear();
			//likesAndUser = likesAndUser.OrderByDescending(u => u.Value).ToDictionary(keySelector: u => u.Key, k => k.Value);

			int count = 0;

			foreach (KeyValuePair<UserModel, int> userLikes in likesAndUser.OrderByDescending(u => u.Value)) 
			{
				usersWithMostLikes.Add(userLikes.Key);
				count++;

				if (count == amount) break;
			}

			return View("UsersList", usersWithMostLikes);
		}

		public async Task<List<UserModel>> GetPosts(List<UserModel> users)
		{
			List<PostModel> posts = await _Models.Posts.AsNoTracking().ToListAsync();

			int length = posts.Count;
			int userLength = users.Count;

			for (int i = 0; i < length; i++)
			{
				for (int j = 0; j < userLength; j++)
				{
					if (users[j].Posts == null) users[j].Posts = new List<PostModel>();

					if (posts[i].UserId == users[j].Id)
					{
						users[j].Posts.Add(posts[i]);
					}
				}
			}

			return users;
		}
	}
}
