#nullable disable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebProject.Models
{
    public class CommentModel
    {
        /// <summary>
        /// Represents the commment's unique identifier { int }.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Represents the id of the user for SQL database { int }.
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// Represents the id of the user of the comment { int }.
        /// </summary>
        [NotMapped]
        public UserModel User { get; set; } = new UserModel();
        /// <summary>
        /// Represents the id of the post for SQL database { int }.
        /// </summary>
        public int PostId { get; set; }
        /// <summary>
        /// Represents the content of the comment { nvarchar(MAX) }.
        /// </summary>
        [Required]
        [StringLength(int.MaxValue, MinimumLength = 0)]
        public string CommentContent { get; set; } = String.Empty;
        /// <summary>
        /// Represents the amount of likes the comment has received { int }.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int Likes { get; set; } = 0;
        /// <summary>
        /// Represents the replies the comment has received { Table }
        /// </summary>
        [NotMapped]
        public ICollection<ReplyModel> Replies { get; set; }
    }
}