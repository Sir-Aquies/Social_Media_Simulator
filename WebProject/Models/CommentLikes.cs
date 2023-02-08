#nullable disable
namespace WebProject.Models
{
	public class CommentLikes
	{
		public DateTime LikedDate { get; set; }

		public int CommentId { get; set; }
		public CommentModel Comment { get; set; }

		public string UserId { get; set; }
		public UserModel User { get; set; }
	}
}
