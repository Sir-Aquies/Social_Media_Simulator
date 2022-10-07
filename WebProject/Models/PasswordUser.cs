#nullable disable
using System.ComponentModel.DataAnnotations;

namespace WebProject.Models
{
	public class PasswordUser
	{
		[Required]
		[DataType(DataType.Password)]
		public string OldPassword { get; set; }
		[Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
		[Required]
		[Compare("NewPassword")]
        [DataType(DataType.Password)]
        public string ConfirmNewPassword { get; set; }

		public UserModel User { get; set; }
	}
}
