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

            userModel = await _Models.Users.AsNoTracking().FirstOrDefaultAsync(us => us.Id == UserId);

            if (userModel == null)
            {
                return NotFound();
            }

            return View(userModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(UserModel ChangeUser, IFormFile ProfilePicture)
        {
            UserModel userModel = new UserModel();

            userModel = await _Models.Users.AsNoTracking().FirstOrDefaultAsync(us => us.Id == ChangeUser.Id);

            if (userModel == null)
            {
                ViewBag.ErrorMessage = "Sorry, something went wrong.";
                return View(userModel);
            }

            if (ChangeUser == null)
            {
                ViewBag.ErrorMessage = "Sorry, something went wrong.";
                return View(userModel);
            }

            userModel.Username = ChangeUser.Username;
            userModel.FirstName = ChangeUser.FirstName;
            userModel.LastName = ChangeUser.LastName;
            userModel.Description = ChangeUser.Description;

            if (ProfilePicture != null)
            {
                userModel.ProfilePicture = await GetBytes(ProfilePicture);
            }

            _Models.Attach(userModel).State = EntityState.Modified;
            await _Models.SaveChangesAsync();
            ViewBag.Message = "Profile sucesfully updated.";

            //if (await TryUpdateModelAsync<UserModel>(
            //    userModel, "ChangeUser", u => u.Username, u => u.FirstName, u => u.FirstName, u => u.LastName, u => u.Description, u => u.ProfilePicture))
            //{
            //    _Models.Attach(userModel).State = EntityState.Modified;
            //    await _Models.SaveChangesAsync();
            //    ViewBag["Message"] = "Profile sucesfully updated.";
            //    return View(userModel);
            //}

            return View(userModel);
        }

        public async Task<IActionResult> Appearance(int? UserId)
        {
            UserModel userModel = new UserModel();

            if (UserId == null)
            {
                return View(null);
            }

            userModel = await _Models.Users.AsNoTracking().FirstOrDefaultAsync(us => us.Id == UserId);

            if (userModel == null)
            {
                return NotFound();
            }

            return View(userModel);
        }

        private async Task<byte[]> GetBytes(IFormFile formFile)
        {
            await using var memoryStream = new MemoryStream();
            await formFile.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
