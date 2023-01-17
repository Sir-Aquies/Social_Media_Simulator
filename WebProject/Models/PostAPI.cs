﻿#nullable disable
namespace WebProject.Models
{
	public class Image
	{
		public string signature { get; set; }
		public string extension { get; set; }
		public int image_id { get; set; }
		public int favourites { get; set; }
		public string dominant_color { get; set; }
		public string source { get; set; }
		public DateTime uploaded_at { get; set; }
		public object liked_at { get; set; }
		public bool is_nsfw { get; set; }
		public int width { get; set; }
		public int height { get; set; }
		public string url { get; set; }
		public string preview_url { get; set; }
		public List<Tag> tags { get; set; }
	}

	public class Waifu
	{
		public List<Image> images { get; set; }
	}

	public class Tag
	{
		public int tag_id { get; set; }
		public string name { get; set; }
		public string description { get; set; }
		public bool is_nsfw { get; set; }
	}
	public class DogAPI
	{
		public string message { get; set; }
	}

	public class Picsum
	{
		public string download_url { get; set; }
	}
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