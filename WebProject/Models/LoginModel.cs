#nullable disable
using System.ComponentModel.DataAnnotations;

namespace WebProject.Models
{
	public class LoginModel
	{
		[Required]
		public string UserName { get; set; }
		[Required]
		[DataType(DataType.Password)]
		public string Password { get; set; }
		public string ReturnUrl { get; set; }
	}
}
