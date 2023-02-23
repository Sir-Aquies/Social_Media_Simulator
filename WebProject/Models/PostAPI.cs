#nullable disable
namespace WebProject.Models
{
	public class Image
	{
		public string url { get; set; }
	}

	public class Waifu
	{
		public List<Image> images { get; set; }
	}

	public class DogAPI
	{
		public string message { get; set; }
	}

	public class Picsum
	{
		public string download_url { get; set; }
	}

	public class Quote
	{
		public string author { get; set; }
		public string body { get; set; }
	}

	public class FavQuote
	{
		public Quote quote { get; set; }
	}

	public class LoremIpsum
	{
		public string very_long_sentence { get; set; }
		public List<string> paragraphs { get; set; }
	}
}
