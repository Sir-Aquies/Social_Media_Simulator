#nullable disable
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebProject.Data;
using WebProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;

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

            //if (CreateUser.DateofBirth > DateTime.Now)
            //{
            //    ModelState.AddModelError(nameof(CreateUser.DateofBirth), "Date of Birth cannot be in the future");
            //}
            //else if (CreateUser.DateofBirth < new DateTime(1960, 1, 1))
            //{
            //    ModelState.AddModelError(nameof(CreateUser.DateofBirth), "Date of Birth should not be before 1960");
            //}

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (await TryUpdateModelAsync<UserModel>(
                emptyUser, "CreateUser", u => u.EmailAddress, u => u.Username, u => u.DateofBirth, u => u.Password, u => u.ConfirmPassword))
            {
                _Models.Users.Add(emptyUser);
                await _Models.SaveChangesAsync();
                return RedirectToPage("./Login");
            }

            return Page();
        }
    }
}
