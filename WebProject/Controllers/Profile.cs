﻿#nullable disable
using Microsoft.AspNetCore.Mvc;
using WebProject.Models;
using WebProject.Data;
using Microsoft.EntityFrameworkCore;

namespace WebProject.Controllers
{
    public class Profile : Controller
    {
        private readonly WebProjectContext _Models;

        public Profile(WebProjectContext Models)
        {
            _Models = Models;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult UserPage(int? userId)
        {
            UserModel userModel = new UserModel();

            if (userId == null)
            {
                return NotFound();
            }

            userModel = _Models.Users.Include(u => u.Posts).AsNoTracking().FirstOrDefault(us => us.Id == userId);

            if (userModel == null)
            {
                return NotFound();
            }

            return View(userModel);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost(PostModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            _Models.Posts.Add(model);
            await _Models.SaveChangesAsync();

            return View();
        }
    }
}
