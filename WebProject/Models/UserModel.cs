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
		/// User's followers.
		/// </summary>
		public IList<Followers> Followers { get; set; }
		/// <summary>
		/// User's followign users.
		/// </summary>
		public IList<Followers> Following { get; set; }

		[BindNever]
		public bool ShowImages { get; set; } = true;

		[NotMapped]
		public IList<PostModel> FavoritePost { get; set; }
	}
}
