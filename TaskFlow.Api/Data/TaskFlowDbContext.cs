using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Entities;

namespace TaskFlow.Api.Data
{
	public class TaskFlowDbContext(DbContextOptions<TaskFlowDbContext> options) : DbContext(options)
	{
		public DbSet<TaskItem> Tasks => Set<TaskItem>();

		public DbSet<TaskAttachment> Attachments => Set<TaskAttachment>();

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<TaskItem>(entity =>
			{
				entity.Property(t => t.TenantId).HasMaxLength(100).IsRequired();
				entity.Property(t => t.Title).HasMaxLength(200).IsRequired();
			});

			modelBuilder.Entity<TaskAttachment>(entity =>
			{
				entity.Property(a => a.BlobName).HasMaxLength(400).IsRequired();
				entity.Property(a => a.FileName).HasMaxLength(260).IsRequired();
				entity.Property(a => a.ContentType).HasMaxLength(150).IsRequired();

				entity.HasOne(a => a.TaskItem)
					.WithMany(t => t.Attachments)
					.HasForeignKey(a => a.TaskItemId)
					.OnDelete(DeleteBehavior.Cascade);
			});
		}
	}
}
