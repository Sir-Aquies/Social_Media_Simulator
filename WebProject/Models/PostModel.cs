using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public int UserId { get; set; }
        /// <summary>
        /// Represents the id of the user of the post { int }.
        /// </summary>
        public UserModel User { get; set; } = new UserModel();
        /// <summary>
        /// Represents the description of the post { nvarchar(MAX) }.
        /// </summary>
        public string Content { get; set; } = String.Empty;
        /// <summary>
        /// Represents the images, videos attach to the post { Table }.
        /// </summary>
        public List<byte[]>? Media { get; set; }
        /// <summary>
        /// Represents the amount of likes the post has received { int }.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int Likes { get; set; } = 0;
        /// <summary>
        /// Represent if the post has been edited { BIT }.
        /// </summary>
        public bool IsEdited { get; set; } = false;
        /// <summary>
        /// Represents the comments the post has received { Table }
        /// </summary>
        public List<CommentModel> Comments { get; set; } = new List<CommentModel>();
    }
}