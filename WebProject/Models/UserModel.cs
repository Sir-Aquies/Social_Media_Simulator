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
		public DateTime DateofBirth { get; set; }
		/// <summary>
		/// User's description { nvarchar(1000) }.
		/// </summary>
		public string Description { get; set; }
		/// <summary>
		/// User's profile picture { image }.
		/// </summary>
		public string ProfilePicture { get; set; }
		/// <summary>
		/// User's publish posts { Table }
		/// </summary>
		public IList<PostModel> Posts { get; set; }
		/// <summary>
		/// User's favorite posts { Table }
		/// </summary>
		[NotMapped]
		public IList<PostModel> FavoritePost { get; set; }
		[BindNever]
		public bool ShowImages { get; set; } = true;
		public IList<CommentModel> Comments { get; set; }
		public IList<PostModel> LikedPost { get; set; }
		public IList<CommentModel> LikedComments { get; set; }
	}
}
