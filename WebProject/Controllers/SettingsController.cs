#nullable disable
using Microsoft.AspNetCore.Mvc;
using WebProject.Models;
using WebProject.Data;
using Microsoft.EntityFrameworkCore;

namespace WebProject.Controllers
{
    public class SettingsController : Controller
    {
        private readonly WebProjectContext _Models;

        public SettingsController(WebProjectContext Models)
        {
            _Models = Models;
        }

        public async Task<IActionResult> EditProfile(int? UserId)
        {
            UserModel userModel = new UserModel();

            if (UserId == null)
            {
                return View(null);
            }

            userModel = await _Models.Users.Include(u => u.Posts).AsNoTracking().FirstOrDefaultAsync(us => us.Id == UserId);

            if (userModel == null)
            {
                return NotFound();
            }

            return View(userModel);
        }

        public async Task<IActionResult> Appearance(int? UserId)
        {
            UserModel userModel = new UserModel();

            if (UserId == null)
            {
                return View(null);
            }

            userModel = await _Models.Users.Include(u => u.Posts).AsNoTracking().FirstOrDefaultAsync(us => us.Id == UserId);

            if (userModel == null)
            {
                return NotFound();
            }

            return View(userModel);
        }

    }
}
