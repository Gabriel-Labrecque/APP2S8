using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using Sanssoussi.Areas.Identity.Data;
using Sanssoussi.Models;

namespace Sanssoussi.Data
{
    public class SanssoussiContext : IdentityDbContext<SanssoussiUser>
    {
        public SanssoussiContext(DbContextOptions<SanssoussiContext> options)
            : base(options)
        {
        }

        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Comment>(
                comment =>
                {
                    comment.HasKey(c => c.CommentId);
                    comment.Property(c => c.Text).HasColumnName("Comment").HasMaxLength(Comment.MaxLength).IsRequired();
                    comment.HasOne(c => c.User).WithMany().HasForeignKey(c => c.UserId).IsRequired().OnDelete(DeleteBehavior.Cascade);
                });
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
    }
}