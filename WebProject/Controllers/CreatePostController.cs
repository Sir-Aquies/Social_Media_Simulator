using Microsoft.AspNetCore.Mvc;
using WebProject.Models;
using WebProject.Data;
using Microsoft.EntityFrameworkCore;

namespace WebProject.Controllers
{
    public class CreatePostController : Controller
    {
        private readonly WebProjectContext _Models;

        public CreatePostController(WebProjectContext Models)
        {
            _Models = Models;
        }

        public IActionResult Index()
        {
            return View();
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
