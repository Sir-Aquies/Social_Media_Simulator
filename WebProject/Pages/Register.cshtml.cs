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
        private readonly WebProject.Data.WebProjectSQL _Models;

        public RegisterModel(WebProject.Data.WebProjectSQL Models)
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
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _Models.UserModel.Add(CreateUser);
            await _Models.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
