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
        public async Task<IActionResult> CreatePost(string Content, int UserId, IFormFile photo)
        {
            PostModel post = new PostModel();
            post.UserModelId = UserId;
            post.PostContent = Content;
            post.Media = await GetBytes(photo);

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
