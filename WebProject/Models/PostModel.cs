#nullable disable
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Permissions;

namespace WebProject.Models
{
	public class PostModel
	{
		/// <summary>
		/// Post's unique identifier for C# code and SQl database { int } .
		/// </summary>
		public int Id { get; set; }
		/// <summary>
		/// Represents the id of the user for SQL database { int }.
		/// </summary>
		public string UserId { get; set; }
		/// <summary>
		/// Represents the description of the post { nvarchar(MAX) }.
		/// </summary>
		public string Content { get; set; }
		/// <summary>
		/// Represents the images, videos attach to the post { Table }.
		/// </summary>
		public string Media { get; set; }
		/// <summary>
		/// Represents the amount of likes the post has received { int }.
		/// </summary>
		[BindNever]
		public int Likes { get; set; }
		/// <summary>
		/// Represent if the post has been edited { BIT }.
		/// </summary>
		[BindNever]
		public bool IsEdited { get; set; }
		public DateTime EditedDate { get; set; }
		public DateTime PostDate { get; set; }

		/// <summary>
		/// Represents the comments the post has received.
		/// </summary>
		public IList<CommentModel> Comments { get; set; }
		/// <summary>
		/// Represents the id of the user of the post { int }.
		/// </summary>
		public UserModel User { get; set; }
		public IList<UserModel> UsersLikes { get; set; }
		
	}
}