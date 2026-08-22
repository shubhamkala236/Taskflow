using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Activity;
using TaskFlow.Api.Data;
using TaskFlow.Api.Dtos;
using TaskFlow.Api.Entities;
using TaskFlow.Api.Services;
using TaskFlow.Api.Tenancy;

namespace TaskFlow.Api.Controllers
{
	[ApiController]
	[Route("api/tasks")]
	public class TasksController(
		TaskFlowDbContext db,
		IBlobSasService blobSasService,
		IActivityLogService activityLogService) : ControllerBase
	{
		[HttpGet]
		public async Task<ActionResult<IReadOnlyList<TaskDto>>> GetTasks(CancellationToken cancellationToken)
		{
			var tasks = await db.Tasks
				.Where(t => t.TenantId == TenantContext.CurrentTenantId)
				.Include(t => t.Attachments.Where(a => a.Confirmed))
				.OrderByDescending(t => t.CreatedAt)
				.ToListAsync(cancellationToken);

			return Ok(tasks.Select(ToDto).ToList());
		}

		[HttpGet("{id:guid}")]
		public async Task<ActionResult<TaskDto>> GetTask(Guid id, CancellationToken cancellationToken)
		{
			var task = await db.Tasks
				.Where(t => t.TenantId == TenantContext.CurrentTenantId && t.Id == id)
				.Include(t => t.Attachments.Where(a => a.Confirmed))
				.FirstOrDefaultAsync(cancellationToken);

			if (task is null)
			{
				return NotFound();
			}

			return Ok(ToDto(task));
		}

		[HttpPost]
		public async Task<ActionResult<TaskDto>> CreateTask(CreateTaskRequest request, CancellationToken cancellationToken)
		{
			var task = new TaskItem
			{
				Id = Guid.NewGuid(),
				TenantId = TenantContext.CurrentTenantId,
				Title = request.Title,
				Description = request.Description,
				IsComplete = false,
				CreatedAt = DateTimeOffset.UtcNow
			};

			db.Tasks.Add(task);
			await db.SaveChangesAsync(cancellationToken);

			await activityLogService.LogAsync(new ActivityLogEntry
			{
				TenantId = TenantContext.CurrentTenantId,
				TaskItemId = task.Id.ToString(),
				Action = "TaskCreated",
				Details = task.Title
			}, cancellationToken);

			return CreatedAtAction(nameof(GetTask), new { id = task.Id }, ToDto(task));
		}

		[HttpPut("{id:guid}")]
		public async Task<ActionResult<TaskDto>> UpdateTask(Guid id, UpdateTaskRequest request, CancellationToken cancellationToken)
		{
			var task = await db.Tasks
				.Where(t => t.TenantId == TenantContext.CurrentTenantId && t.Id == id)
				.Include(t => t.Attachments.Where(a => a.Confirmed))
				.FirstOrDefaultAsync(cancellationToken);

			if (task is null)
			{
				return NotFound();
			}

			task.Title = request.Title;
			task.Description = request.Description;
			task.IsComplete = request.IsComplete;
			await db.SaveChangesAsync(cancellationToken);

			await activityLogService.LogAsync(new ActivityLogEntry
			{
				TenantId = TenantContext.CurrentTenantId,
				TaskItemId = task.Id.ToString(),
				Action = "TaskUpdated",
				Details = task.Title
			}, cancellationToken);

			return Ok(ToDto(task));
		}

		[HttpDelete("{id:guid}")]
		public async Task<IActionResult> DeleteTask(Guid id, CancellationToken cancellationToken)
		{
			var task = await db.Tasks
				.FirstOrDefaultAsync(t => t.TenantId == TenantContext.CurrentTenantId && t.Id == id, cancellationToken);

			if (task is null)
			{
				return NotFound();
			}

			db.Tasks.Remove(task);
			await db.SaveChangesAsync(cancellationToken);

			await activityLogService.LogAsync(new ActivityLogEntry
			{
				TenantId = TenantContext.CurrentTenantId,
				TaskItemId = id.ToString(),
				Action = "TaskDeleted",
				Details = task.Title
			}, cancellationToken);

			return NoContent();
		}

		[HttpPost("{id:guid}/attachments/sas")]
		public async Task<ActionResult<RequestUploadSasResponse>> RequestUploadSas(
			Guid id,
			RequestUploadSasRequest request,
			CancellationToken cancellationToken)
		{
			var taskExists = await db.Tasks
				.AnyAsync(t => t.TenantId == TenantContext.CurrentTenantId && t.Id == id, cancellationToken);

			if (!taskExists)
			{
				return NotFound();
			}

			var blobName = $"{id}/{Guid.NewGuid()}-{request.FileName}";

			var attachment = new TaskAttachment
			{
				Id = Guid.NewGuid(),
				TaskItemId = id,
				BlobName = blobName,
				FileName = request.FileName,
				ContentType = request.ContentType,
				SizeBytes = 0,
				UploadedAt = DateTimeOffset.UtcNow,
				Confirmed = false
			};

			db.Attachments.Add(attachment);
			await db.SaveChangesAsync(cancellationToken);

			var uploadUrl = blobSasService.GenerateUploadSasUri(blobName, request.ContentType);

			return Ok(new RequestUploadSasResponse(attachment.Id, uploadUrl.ToString(), blobName));
		}

		[HttpPost("{id:guid}/attachments/{attachmentId:guid}/confirm")]
		public async Task<IActionResult> ConfirmAttachment(Guid id, Guid attachmentId, CancellationToken cancellationToken)
		{
			var attachment = await db.Attachments
				.FirstOrDefaultAsync(a => a.Id == attachmentId && a.TaskItemId == id, cancellationToken);

			if (attachment is null)
			{
				return NotFound();
			}

			attachment.Confirmed = true;
			await db.SaveChangesAsync(cancellationToken);

			await activityLogService.LogAsync(new ActivityLogEntry
			{
				TenantId = TenantContext.CurrentTenantId,
				TaskItemId = id.ToString(),
				Action = "AttachmentUploaded",
				Details = attachment.FileName
			}, cancellationToken);

			return NoContent();
		}

		[HttpGet("{id:guid}/attachments")]
		public async Task<ActionResult<IReadOnlyList<AttachmentDto>>> GetAttachments(Guid id, CancellationToken cancellationToken)
		{
			var attachments = await db.Attachments
				.Where(a => a.TaskItemId == id && a.Confirmed)
				.Select(a => new AttachmentDto(a.Id, a.FileName, a.ContentType, a.SizeBytes, a.UploadedAt))
				.ToListAsync(cancellationToken);

			return Ok(attachments);
		}

		private static TaskDto ToDto(TaskItem task) => new(
			task.Id,
			task.Title,
			task.Description,
			task.IsComplete,
			task.CreatedAt,
			task.Attachments
				.Select(a => new AttachmentDto(a.Id, a.FileName, a.ContentType, a.SizeBytes, a.UploadedAt))
				.ToList());
	}
}
