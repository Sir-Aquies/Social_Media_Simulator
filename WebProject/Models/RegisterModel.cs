#nullable disable
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using WebProject.CostumValidation;

namespace WebProject.Models
{
	public class RegisterModel
	{
		[Required]
		[MinLength(5, ErrorMessage = "Username should be longer than 5 characters")]
		public string UserName { get; set; }
		[Required]
		[RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Email is not valid")]
		public string Email { get; set; }
		[Required]
		[DataType(DataType.Date)]
		[Display(Name = "Date of birth")]
		[DateValidation]
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
