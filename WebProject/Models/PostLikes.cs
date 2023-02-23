#nullable disable

namespace WebProject.Models
{
	public class PostLikes
	{
		public DateTime LikedDate { get; set; }

		public int PostId { get; set; }
		public PostModel Post { get; set; }

		public string UserId { get; set; }	
		public UserModel User { get; set; }
	}
}
