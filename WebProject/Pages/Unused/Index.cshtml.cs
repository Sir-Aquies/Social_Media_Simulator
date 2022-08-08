#nullable disable
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebProject.Data;
using WebProject.Models;

namespace WebProject.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly WebProjectContext _Models;

        public IndexModel(ILogger<IndexModel> logger, WebProjectContext Models)
        {
            _logger = logger;
            _Models = Models;
        }
        
        [ModelBinder]
        public PostModel CreatePost { get; set; }
        public UserModel PageUser { get; set; }
        public bool IsthereUser { get; set; } = true;

        public async Task<IActionResult> OnGetAsync(int? userid)
        {

            if (userid == null)
            {
                IsthereUser = false;
                return Page();
            }

            PageUser = await _Models.Users.Include(u => u.Posts).AsNoTracking().FirstOrDefaultAsync(us => us.Id == userid);
            CreatePost.UserModelId = (int)userid;

            if (PageUser == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var emptypost = new PostModel();

            if (!ModelState.IsValid)
            {
                return Page();
            }
            
            if (await TryUpdateModelAsync<PostModel>(
                emptypost, "CreatePost", u => u.UserModelId, u => u.PostContent))
            {
                _Models.Posts.Add(emptypost);
                await _Models.SaveChangesAsync();
                return RedirectToPage("./Index", new {userid = emptypost.UserModelId});
            }

            return Page();
        }
    }
}