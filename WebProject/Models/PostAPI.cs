#nullable disable
namespace WebProject.Models
{
	public class HipsterText
	{
		public string word { get; set; }
		public List<string> words { get; set; }
		public string sentence { get; set; }
		public List<string> sentences { get; set; }
		public string paragraph { get; set; }
		public List<string> paragraphs { get; set; }
	}

	public class Quote
	{
		public int id { get; set; }
		public bool dialogue { get; set; }
		public bool @private { get; set; }
		public List<string> tags { get; set; }
		public string url { get; set; }
		public int favorites_count { get; set; }
		public int upvotes_count { get; set; }
		public int downvotes_count { get; set; }
		public string author { get; set; }
		public string author_permalink { get; set; }
		public string body { get; set; }
	}

	public class FavQuote
	{
		public DateTime qotd_date { get; set; }
		public Quote quote { get; set; }
	}

	public class LoremIpsum
	{
		public int id { get; set; }
		public string uid { get; set; }
		public string word { get; set; }
		public List<string> words { get; set; }
		public string characters { get; set; }
		public string short_sentence { get; set; }
		public string long_sentence { get; set; }
		public string very_long_sentence { get; set; }
		public List<string> paragraphs { get; set; }
		public string question { get; set; }
		public List<string> questions { get; set; }
	}
}
