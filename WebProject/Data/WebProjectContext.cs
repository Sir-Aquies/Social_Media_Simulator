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

			builder.Entity<FollowUsers>(entity =>
			{
				//TODO - change name
				//entity.ToTable("Followers");

				entity.HasKey(f => new { f.CreatorId, f.FollowerId });

				entity.Property(f => f.CreatorId)
				.HasColumnType("NVARCHAR(450)");

				entity.Property(f => f.FollowerId)
				.HasColumnType("NVARCHAR(450)");

				entity.Property(f => f.FollowedDate)
				.HasColumnType("DATETIME2")
				.HasDefaultValueSql("GETDATE()");

				entity.HasOne(f => f.Creator).WithMany(u => u.Followers).HasForeignKey(f => f.FollowerId).OnDelete(DeleteBehavior.Cascade);

				entity.HasOne(f => f.Follower).WithMany(u => u.Following).HasForeignKey(f => f.CreatorId).OnDelete(DeleteBehavior.ClientCascade);
			});

			builder.Entity<UserModel>(entity =>
			{
				entity.ToTable("Users");

				entity.Property(u => u.Name)
				.HasColumnType("NVARCHAR(255)");
				entity.Property(u => u.Description)
				.HasColumnType("NVARCHAR(500)");

				entity.Ignore(u => u.PhoneNumber);
				entity.Ignore(u => u.PhoneNumberConfirmed);
			});

			builder.Entity<PostModel>(entity =>
			{
				entity.HasKey(p => p.Id);

				entity.Property(p => p.UserId)
				.HasColumnType("NVARCHAR(450)");

				entity.Property(p => p.Date)
					.HasColumnType("datetime2")
					.HasDefaultValueSql("getdate()");

				entity.Property(p => p.EditedDate)
					.HasColumnType("datetime2");

				entity.HasOne(p => p.User).WithMany(u => u.Posts).HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.ClientCascade);
			});

			builder.Entity<PostLikes>(entity =>
			{
				entity.HasKey(pl => new { pl.PostId, pl.UserId });

				entity.Property(pl => pl.UserId)
				.HasColumnType("NVARCHAR(450)");

				entity.Property(pl => pl.LikedDate)
				.HasColumnType("DATETIME2")
				.HasDefaultValueSql("GETDATE()");

				entity.HasOne(pl => pl.Post).WithMany(p => p.UserLikes).HasForeignKey(pl => pl.PostId).OnDelete(DeleteBehavior.Cascade);
				entity.HasOne(pl => pl.User).WithMany().HasForeignKey(pl => pl.UserId).OnDelete(DeleteBehavior.Cascade);

			});

			builder.Entity<CommentModel>(entity =>
			{
				entity.HasKey(c => c.Id);

				entity.Property(c => c.UserId).HasColumnType("NVARCHAR(450)");

				entity.Property(c => c.Date)
					.HasColumnType("datetime2")
					.HasDefaultValueSql("getdate()");

				entity.HasOne(c => c.User).WithMany().HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.SetNull);

				entity.HasOne(c => c.Post).WithMany(p => p.Comments).HasForeignKey(c => c.PostId).OnDelete(DeleteBehavior.Cascade);
			});

			builder.Entity<CommentLikes>(entity =>
			{
				entity.HasKey(cl => new { cl.CommentId, cl.UserId });

				entity.Property(cl => cl.UserId)
				.HasColumnType("NVARCHAR(450)");

				entity.Property(cl => cl.LikedDate)
				.HasColumnType("DATETIME2")
				.HasDefaultValueSql("GETDATE()");

				entity.HasOne(cl => cl.Comment).WithMany(c => c.UserLikes).HasForeignKey(cl => cl.CommentId).OnDelete(DeleteBehavior.Cascade);
				entity.HasOne(cl => cl.User).WithMany().HasForeignKey(cl => cl.UserId).OnDelete(DeleteBehavior.Cascade);

			});
		}
	}
}
