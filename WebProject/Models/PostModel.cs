#nullable disable
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
        [Required]
        public int UserModelId { get; set; }
        /// <summary>
        /// Represents the id of the user of the post { int }.
        /// </summary>
        [NotMapped]
        public UserModel User { get; set; }
        /// <summary>
        /// Represents the description of the post { nvarchar(MAX) }.
        /// </summary>
        [Required]
        public string PostContent { get; set; }
        /// <summary>
        /// Represents the images, videos attach to the post { Table }.
        /// </summary>
        public byte[] Media { get; set; }
        /// <summary>
        /// Represents the amount of likes the post has received { int }.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int Likes { get; set; }
        /// <summary>
        /// Represent if the post has been edited { BIT }.
        /// </summary>
        [NotMapped]
        public bool IsEdited { get; set; }
        /// <summary>
        /// Represents the comments the post has received { Table }
        /// </summary>
        [NotMapped]
        public ICollection<CommentModel> Comments { get; set; }
    }
}