#nullable disable
namespace WebProject.Models
{
	public class NormalUser
	{
		public List<Result> results { get; set; }
	}
	public class Result
	{
		public string gender { get; set; }
		public Name name { get; set; }
		public Location location { get; set; }
		public string email { get; set; }
		public Login login { get; set; }
		public Dob dob { get; set; }
		public Picture picture { get; set; }
	}

	public class Dob
	{
		public DateTime date { get; set; }
		public int age { get; set; }
	}

	public class Location
	{
		public string city { get; set; }
		public string country { get; set; }
	}

	public class Login
	{
		public string username { get; set; }
		public string password { get; set; }
		public string salt { get; set; }
	}

	public class Name
	{
		public string first { get; set; }
		public string last { get; set; }
	}

	public class Picture
	{
		public string large { get; set; }
	}
}
