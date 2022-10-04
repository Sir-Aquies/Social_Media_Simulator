#nullable disable
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace WebProject.Models
{
    public class UserModel : IdentityUser
    {
        public string Name { get; set; }
        /// <summary>
        /// User's date of birth { datetime2(7) }.
        /// </summary>
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of birth:")]
        public DateTime DateofBirth { get; set; }
        /// <summary>
        /// User's description { nvarchar(1000) }.
        /// </summary>
        [StringLength(1000)]
        public string Description { get; set; } = String.Empty;
        /// <summary>
        /// User's profile picture { image }.
        /// </summary>
        [BindNever]
        public byte[] ProfilePicture { get; set; }
        /// <summary>
        /// User's publish posts { Table }
        /// </summary>
        public ICollection<PostModel> Posts { get; set; }
        /// <summary>
        /// User's favorite posts { Table }
        /// </summary>
        [NotMapped]
        [BindNever]
        public ICollection<PostModel> FavoritePost { get; set; }
        [BindNever]
        public bool ShowImages { get; set; } = true;
        public ICollection<CommentModel> Comments { get; set; }
    }
}
