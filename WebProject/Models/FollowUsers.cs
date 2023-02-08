#nullable disable

namespace WebProject.Models
{
	public class FollowUsers
	{
		public DateTime FollowedDate { get; set; }

		public string CreatorId { get; set; }
		public UserModel Creator { get; set; }

		public string FollowerId { get; set; }
		public UserModel Follower { get; set; }

	}
}
