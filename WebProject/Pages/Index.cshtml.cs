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

        [BindProperty]
        public UserModel PageUser { get; set; }
        [BindProperty]
        public PostModel CreatePost { get; set; }

        public async Task<IActionResult> OnGetAsync(int? userid)
        {
            userid = 1;
            if (userid == null)
            {
                return NotFound();
            }

            PageUser = await _Models.Users.Include(u => u.Posts).AsNoTracking().FirstOrDefaultAsync(us => us.Id == userid);

            if (PageUser == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}