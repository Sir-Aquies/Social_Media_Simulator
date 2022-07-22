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

        public DbSet<UserModel> UserModel { get; set; }
        public DbSet<PostModel> PostModel { get; set; }
        //public DbSet<CommentModel> CommentModel { get; set; }
        //public DbSet<ReplyModel> ReplyModel { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserModel>().ToTable("Users");
            modelBuilder.Entity<PostModel>().ToTable("Posts");
            //modelBuilder.Entity<CommentModel>().ToTable("Comments");
            //modelBuilder.Entity<ReplyModel>().ToTable("Replies");
        }

    }
}
