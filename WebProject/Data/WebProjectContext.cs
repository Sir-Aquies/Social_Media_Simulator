#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebProject.Models;

namespace WebProject.Data
{
    public class WebProjectContext : DbContext
    {
        public WebProjectContext (DbContextOptions<WebProjectContext> options)
            : base(options)
        {
        }

        public DbSet<UserModel> Users { get; set; }
        public DbSet<PostModel> Posts { get; set; }
        public DbSet<CommentModel> CommentModel { get; set; }
        //public DbSet<ReplyModel> ReplyModel { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserModel>().ToTable("Users");
            modelBuilder.Entity<PostModel>().ToTable("Posts");
            modelBuilder.Entity<CommentModel>().ToTable("Comments");
            //modelBuilder.Entity<ReplyModel>().ToTable("Replies");
        }
    }
}
