#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebProject.Models;

namespace WebProject.Data
{
    public class WebProjectSQL : DbContext
    {
        public WebProjectSQL(DbContextOptions<WebProjectSQL> options)
            : base(options)
        {
        }

        public DbSet<WebProject.Models.UserModel> UserModel { get; set; }
        //public DbSet<WebProject.Models.PostModel> PostModel { get; set; }
        //public DbSet<WebProject.Models.CommentModel> CommentModel { get; set; }
        //public DbSet<WebProject.Models.ReplyModel> ReplyModel { get; set; }
    }
}
