#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebProject.Models;

namespace WebProject.Data
{
	public class WebProjectContext : IdentityDbContext<UserModel>
	{
		public WebProjectContext(DbContextOptions<WebProjectContext> options) : base(options)
		{
		}

		public DbSet<PostModel> Posts { get; set; }
		public DbSet<CommentModel> Comments { get; set; }

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			builder.Entity<PostModel>(entity =>
			{
				entity.HasKey(p => p.Id);

				entity.Property(p => p.PostDate)
					.HasColumnType("datetime2")
					.HasDefaultValueSql("getdate()");

				entity.HasOne(p => p.User).WithMany(u => u.Posts).HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
			});

			builder.Entity<CommentModel>(entity =>
			{
				entity.HasKey(c => c.Id);

				entity.HasOne(c => c.User).WithMany(u => u.Comments).HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Restrict);
				entity.HasOne(c => c.Post).WithMany(p => p.Comments).HasForeignKey(c => c.PostId).OnDelete(DeleteBehavior.Cascade);
			});
		}
	}
}
