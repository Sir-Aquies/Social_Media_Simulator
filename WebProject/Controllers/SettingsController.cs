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
                return NotFound();
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
            ViewBag.Message = "Profile successfully updated.";

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

        [HttpPost]
        public async Task<IActionResult> Appearance(bool ShowImages, int? UserId)
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

            if (userModel.ShowImages)
            {
                userModel.ShowImages = false;
            }
            else if (!userModel.ShowImages)
            {
                userModel.ShowImages = true;
            }

            _Models.Attach(userModel).State = EntityState.Modified;
            await _Models.SaveChangesAsync();

            return View(userModel);
        }

        public async Task<IActionResult> Security(int? UserId)
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

            if (TempData["ErrorMessage"] != null)
            {
                if (!string.IsNullOrEmpty(TempData["ErrorMessage"].ToString()))
                {
                    ViewBag.ErrorMessage = TempData["ErrorMessage"].ToString();
                } 
            }

            if (TempData["Message"] != null)
            {
                if (!string.IsNullOrEmpty(TempData["Message"].ToString()))
                {
                    ViewBag.Message = TempData["Message"].ToString();
                } 
            }

            return View(userModel);
        }

        [HttpPost]
        public async Task<IActionResult> Security(UserModel UserPassword, string OldPassword)
        {
            UserModel userModel = new UserModel();

            userModel = await _Models.Users.AsNoTracking().FirstOrDefaultAsync(us => us.Id == UserPassword.Id);

            if (OldPassword != userModel.Password)
            {
                ViewBag.ErrorMessage = "Old Password is invalid";
                return View(userModel);
            }

            if (userModel == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(UserPassword.Password))
            {
                userModel.Password = UserPassword.Password;

                _Models.Attach(userModel).State = EntityState.Modified;
                await _Models.SaveChangesAsync();
                ViewBag.Message = "Password successfully updated.";
            }

            return View(userModel);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeEmail(UserModel UserEmail)
        {
            UserModel userModel = new UserModel();

            userModel = await _Models.Users.AsNoTracking().FirstOrDefaultAsync(us => us.Id == UserEmail.Id);

            if (userModel == null)
            {
                return NotFound();
            }

            if (userModel.EmailAddress == UserEmail.EmailAddress)
            {
                TempData["ErrorMessage"] = "Email address is the same.";
                return RedirectToAction("Security", new { UserId = UserEmail.Id });
            }

            if (!string.IsNullOrEmpty(UserEmail.EmailAddress))
            {
                userModel.EmailAddress = UserEmail.EmailAddress;

                _Models.Attach(userModel).State = EntityState.Modified;
                await _Models.SaveChangesAsync();
                TempData["Message"] = "Email address successfully updated.";
            }

            return RedirectToAction("Security", new { UserId = UserEmail.Id });
        }

        private async Task<byte[]> GetBytes(IFormFile formFile)
        {
            await using var memoryStream = new MemoryStream();
            await formFile.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
