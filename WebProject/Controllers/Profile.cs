#nullable disable
using Microsoft.AspNetCore.Mvc;
using WebProject.Models;
using WebProject.Data;
using Microsoft.EntityFrameworkCore;

namespace WebProject.Controllers
{
    public class Profile : Controller
    {
        private readonly WebProjectContext _Models;
        private readonly ILogger<Profile> _Logger;

        public Profile(WebProjectContext Models, ILogger<Profile> logger)
        {
            _Models = Models;
            _Logger = logger;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Userpage");
        }

        public IActionResult UserPage(int? userId)
        {
            UserModel userModel = new UserModel();

            if (userId == null)
            {
                return View(null);
            }

            userModel = _Models.Users.Include(u => u.Posts).AsNoTracking().FirstOrDefault(us => us.Id == userId);

            if (userModel == null)
            {
                return NotFound();
            }

            return View(userModel);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost(string Content, int UserId, IFormFile pic)
        {
            PostModel post = new PostModel();
            post.UserModelId = UserId;
            post.PostContent = Content;
            if (pic != null)
            {
                post.Media = await GetBytes(pic);
                //TODO - change the byte[] to nvarchar(MAX).
            }

            if (!ModelState.IsValid)
            {
                return RedirectToAction("UserPage", "Profile", new { userId = UserId });
            }

            _Models.Posts.Add(post);
            await _Models.SaveChangesAsync();

            return RedirectToAction("UserPage", "Profile", new { userId = UserId });
        }

        public async Task<byte[]> GetBytes(IFormFile formFile)
        {
            await using var memoryStream = new MemoryStream();
            await formFile.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
