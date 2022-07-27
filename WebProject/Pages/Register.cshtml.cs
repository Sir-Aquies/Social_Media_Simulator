#nullable disable
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebProject.Data;
using WebProject.Models;

namespace WebProject.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly WebProject.Data.WebProjectContext _Models;

        public RegisterModel(WebProject.Data.WebProjectContext Models)
        {
            _Models = Models;
        }

        public IActionResult OnGet()    
        {
            return Page();
        }
        
        [BindProperty]
        public UserModel CreateUser { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            var emptyUser = new UserModel();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (await TryUpdateModelAsync<UserModel>(
                emptyUser, "CreateUser", u => u.EmailAddress, u => u.Username, u => u.DateofBirth, u => u.Password, u => u.ConfirmPassword))
            {
                _Models.Users.Add(emptyUser);
                await _Models.SaveChangesAsync();
                return RedirectToPage("./Index");
            }

            return Page();
        }
    }
}
