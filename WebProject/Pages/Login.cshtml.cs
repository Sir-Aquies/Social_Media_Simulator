#nullable disable
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebProject.Models;
using WebProject.Controllers;

namespace WebProject.Pages
{
    public class LoginModel : PageModel
    {
        private readonly Data.WebProjectContext _Models;

        public LoginModel(Data.WebProjectContext Models)
        {
            _Models = Models;
        }

        public IActionResult OnGetAsync()
        {
            return Page();
        }

        [BindProperty]
        public UserModel LoginUser { get; set; }
        public string loginError { get; set; } = String.Empty;

        public async Task<IActionResult> OnPostAsync()
        {
            var emptyUser = new UserModel();
            emptyUser = await _Models.Users.FirstOrDefaultAsync(u => (u.EmailAddress == LoginUser.EmailAddress) && (u.Password == LoginUser.Password));

            if (emptyUser == null)
            {
                loginError = "Either the password or the email are wrong";
                return Page();
            }

            if (emptyUser != null)
            {
                //return RedirectToActionPermanent("Index", "Profile", new { userId = emptyUser.Id });
                return RedirectToRoute(new { controller = "Profile", action = "Index", UserId = emptyUser.Id });
            }

            return Page();
        }
    }
}
