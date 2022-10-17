using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebProject.Controllers;
using WebProject.Data;
using WebProject.Models;

namespace WebProject.Components
{
	public class AllUsers : ViewComponent
	{
		private readonly UserManager<UserModel> userManager;

		public AllUsers(UserManager<UserModel> manager)
		{
			userManager = manager;
		}

		public async Task<IViewComponentResult> InvokeAsync()
		{
			List<UserModel> users = await userManager.Users.ToListAsync();
			return View("UsersList", users);
		}
	}
}
