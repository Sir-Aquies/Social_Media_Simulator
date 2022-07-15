using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace WebProject.Models
{
    public class ReplyModel
    {
        /// <summary>
        /// Represents the unique identifier { int }.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Represents the id of the user.
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// Represents the id of the user of the reply { int }.
        /// </summary>
        public UserModel User { get; set; } = new UserModel();
        /// <summary>
        /// Represents the id of the comment for SQL database { int }.
        /// </summary>
        public int CommentId { get; set; }
        /// <summary>
        /// Represents the content of the reply { nvarchar(MAX) }.
        /// </summary>
        [Required]
        [StringLength(int.MaxValue, MinimumLength = 0)]
        public string Content { get; set; } = string.Empty;
        /// <summary>
        /// Represents the amount of likes the reply has received { int }.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int Likes { get; set; } = 0;
    }
}