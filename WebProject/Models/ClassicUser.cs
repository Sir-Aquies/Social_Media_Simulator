#nullable disable
namespace WebProject.Models
{
	public class ClassicUser
	{
		public int id { get; set; }
		public string password { get; set; }
		public string first_name { get; set; }
		public string last_name { get; set; }
		public string username { get; set; }
		public string email { get; set; }
		public string gender { get; set; }
		public string date_of_birth { get; set; }
		public Employment employment { get; set; }
		public Address address { get; set; }
	}
	public class Address
	{
		public string state { get; set; }
		public string country { get; set; }
	}

	public class Employment
	{
		public string title { get; set; }
		public string key_skill { get; set; }
	}
}
