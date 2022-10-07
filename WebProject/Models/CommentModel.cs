#nullable disable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebProject.Models
{
    public class CommentModel
    {
        /// <summary>
        /// Represents the commment's unique identifier.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Represents the id of the user for SQL database.
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// Represents the id of the post for SQL database { int }.
        /// </summary>
        public int PostId { get; set; }
        /// <summary>
        /// Represents the content of the comment { nvarchar(MAX) }.
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// Represents the amount of likes the comment has received { int }.
        /// </summary>
        public int Likes { get; set; } = 0;
        /// <summary>
        /// Represents the replies the comment has received { Table }
        /// </summary>

        /// <summary>
        /// Represents the user of the comment { int }.
        /// </summary>
        public UserModel User { get; set; }
        public PostModel Post { get; set; }
        [NotMapped]
        public ICollection<ReplyModel> Replies { get; set; }
    }
}