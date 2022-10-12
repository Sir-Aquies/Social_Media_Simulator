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

				entity.Property(p => p.EditedDate)
					.HasColumnType("datetime2")
					.HasDefaultValueSql("getdate()");

				entity.HasOne(p => p.User).WithMany(u => u.Posts).HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);

				entity.HasMany(p => p.UsersLikes).WithMany(u => u.LikedPost).UsingEntity<Dictionary<string, object>>(
						"PostLikes",
						p => p
							.HasOne<UserModel>().WithMany().HasForeignKey("UserId").HasConstraintName("FK_PostLikes_User_UserId").OnDelete(DeleteBehavior.NoAction), 
						p => p
							.HasOne<PostModel>().WithMany().HasForeignKey("PostId").HasConstraintName("FK_PostLikes_Post_PostId").OnDelete(DeleteBehavior.NoAction));
			});

			builder.Entity<CommentModel>(entity =>
			{
				entity.HasKey(c => c.Id);

				entity.Property(c => c.Date)
					.HasColumnType("datetime2")
					.HasDefaultValueSql("getdate()");

				entity.HasOne(c => c.User).WithMany(u => u.Comments).HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.NoAction);
				entity.HasOne(c => c.Post).WithMany(p => p.Comments).HasForeignKey(c => c.PostId).OnDelete(DeleteBehavior.Cascade);

				entity.HasMany(p => p.UsersLikes).WithMany(u => u.LikedComments).UsingEntity<Dictionary<string, object>>(
						"CommentLikes",
						p => p
							.HasOne<UserModel>().WithMany().HasForeignKey("UserId").HasConstraintName("FK_CommentLikes_User_UserId").OnDelete(DeleteBehavior.NoAction),
						p => p
							.HasOne<CommentModel>().WithMany().HasForeignKey("CommentId").HasConstraintName("FK_CommentLikes_Comment_CommentId").OnDelete(DeleteBehavior.NoAction));
			});
		}
	}
}
