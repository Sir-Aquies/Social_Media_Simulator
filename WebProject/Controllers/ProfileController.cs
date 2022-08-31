#nullable disable
using Microsoft.AspNetCore.Mvc;
using WebProject.Models;
using WebProject.Data;
using Microsoft.EntityFrameworkCore;

namespace WebProject.Controllers
{
    public class ProfileController : Controller
    {
        private readonly WebProjectContext _Models;
        private readonly ILogger<ProfileController> _Logger;

        public ProfileController(WebProjectContext Models, ILogger<ProfileController> logger)
        {
            _Models = Models;
            _Logger = logger;
        }

        public async Task<IActionResult> Index(int? UserId)
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

        [HttpPost]
        public async Task<IActionResult> Index(string Content, int UserId, IFormFile pic)
        {
            PostModel post = new PostModel();
            post.UserModelId = UserId;
            if (Content != null)
            {
                post.PostContent = Content;
            }

            if (pic != null)
            {
                post.Media = await GetBytes(pic);
                //var picbyte = await GetBytes(pic);
                //post.Media = Convert.ToBase64String(picbyte);
                //TODO - change the byte[] to nvarchar(MAX).
            }

            if (!ModelState.IsValid)
            {
                return RedirectToAction("UserPage", "Profile", new { UserId = UserId });
            }

            _Models.Posts.Add(post);
            await _Models.SaveChangesAsync();

            return RedirectToAction("Index", new { UserId = UserId });
        }

        public async Task<IActionResult> EditPost(int? PostId, int UserId)
        {
            PostModel postModel = new PostModel();
            UserModel userModel = new UserModel();

            if (PostId == null)
            {
                return View();
            }

            postModel = await _Models.Posts.FirstOrDefaultAsync(us => us.Id == PostId);

            if (postModel == null)
            {
                return View();
            }

            //if (Content != null && Content != postModel.PostContent)
            //{
            //    postModel.PostContent = Content;
            //}

            //if (pic != null)
            //{
            //    postModel.Media = await GetBytes(pic);
            //}

            //TODO - add the is edited;

            return PartialView("EditPost", postModel);
        }

        public async Task<IActionResult> DeletePost(int? PostId, int? UserId)
        {
            PostModel postModel = new PostModel();

            if (PostId == null)
            {
                return NotFound();
            }

            postModel = await _Models.Posts.FirstOrDefaultAsync(us => us.Id == PostId);

            if (postModel != null)
            {
                //postModel.PostContent = null;
                //postModel.UserModelId = 0;
                //postModel.Media = null;
                //.Attach(postModel).State = EntityState.Modified;
                _Models.Remove(postModel);
                // TODO - make it so that i doesn't remove the post from the database. 
                await _Models.SaveChangesAsync();
            }

            return RedirectToAction("Index", new { UserId = UserId });
        }

        private async Task<byte[]> GetBytes(IFormFile formFile)
        {
            await using var memoryStream = new MemoryStream();
            await formFile.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
