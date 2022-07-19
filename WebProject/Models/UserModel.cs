using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebProject.Models
{
    public class UserModel
    {
        /// <summary>
        /// User's unique identifier for the C# code and SQl database { int }.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// User's identification name { nvarchar(200) }.
        /// </summary>
        [Required]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Username must be at least 5 characters")]
        public string Username { get; set; } = String.Empty;
        /// <summary>
        /// User's first name { nvarchar(200) }.
        /// </summary>
        [Display(Name = "First Name")]
        [StringLength(200)]
        public string FirstName { get; set; } = String.Empty;
        /// <summary>
        /// User's last name { nvarchar(200) }.
        /// </summary>
        [Display(Name = "Last Name")]
        [StringLength(200)]
        public string LastName { get; set; } = String.Empty;
        /// <summary>
        /// User's date of birth { datetime2(7) }.
        /// </summary>
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateofBirth { get; set; } = DateTime.MinValue;
        /// <summary>
        /// User's personal email { nvarchar(200) }.
        /// </summary>
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = String.Empty;
        /// <summary>
        /// User's personal password { nvarchar(250) }.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [StringLength(250, MinimumLength = 8, ErrorMessage = "Username must be at least 8 characters")]
        public string Password { get; set; } = String.Empty;
        /// <summary>
        /// User's description { nvarchar(1000) }.
        /// </summary>
        [StringLength(1000)]
        public string Description { get; set; } = String.Empty;
        /// <summary>
        /// User's profile picture { image }.
        /// </summary>
        public byte[]? ProfilePicture { get; set; }
        /// <summary>
        /// User's publish posts { Table }
        /// </summary>
        public List<PostModel> Posts { get; set; } = new List<PostModel>();
        /// <summary>
        /// User's favorite posts { Table }
        /// </summary>
        public List<PostModel> FavoritePost { get; set; } = new List<PostModel>();

    }
}
