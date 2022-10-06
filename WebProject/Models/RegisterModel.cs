#nullable disable
using System.ComponentModel.DataAnnotations;

namespace WebProject.Models
{
	public class RegisterModel
	{
        [Required]
        public string UserName { get; set; }
        [Required]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "E-mail is not valid")]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of birth:")]
        public DateTime DateofBirth { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password do not match")]
        public string ConfirmPassword { get; set; }

	}
}
