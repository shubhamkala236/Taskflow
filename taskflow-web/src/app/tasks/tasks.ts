import { Component, OnInit, inject, signal } from '@angular/core';

import { TasksService } from './tasks.service';
import { TaskItem } from './task.model';
import { FeatureFlagsService } from '../features/feature-flags.service';

@Component({
  selector: 'app-tasks',
  imports: [],
  templateUrl: './tasks.html',
  styleUrl: './tasks.css'
})
export class Tasks implements OnInit {
  private readonly tasksService = inject(TasksService);
  private readonly featureFlagsService = inject(FeatureFlagsService);

  protected readonly tasks = signal<TaskItem[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly uploadingTaskId = signal<string | null>(null);
  protected readonly attachmentsEnabled = signal(false);

  ngOnInit(): void {
    this.loadTasks();
    this.featureFlagsService.getFeatures().subscribe({
      next: (flags) => this.attachmentsEnabled.set(flags.attachments),
      error: () => this.attachmentsEnabled.set(false)
    });
  }

  loadTasks(): void {
    this.loading.set(true);
    this.tasksService.getTasks().subscribe({
      next: (tasks) => {
        this.tasks.set(tasks);
        this.loading.set(false);
        this.error.set(null);
      },
      error: () => {
        this.loading.set(false);
        this.error.set('Failed to load tasks — check console for CORS/network errors');
      }
    });
  }

  createTask(titleInput: HTMLInputElement, descriptionInput: HTMLInputElement): void {
    const title = titleInput.value.trim();
    if (!title) {
      return;
    }

    this.tasksService.createTask({ title, description: descriptionInput.value.trim() || null }).subscribe({
      next: () => {
        titleInput.value = '';
        descriptionInput.value = '';
        this.loadTasks();
      },
      error: () => this.error.set('Failed to create task')
    });
  }

  toggleComplete(task: TaskItem): void {
    this.tasksService
      .updateTask(task.id, { title: task.title, description: task.description, isComplete: !task.isComplete })
      .subscribe({
        next: () => this.loadTasks(),
        error: () => this.error.set('Failed to update task')
      });
  }

  deleteTask(task: TaskItem): void {
    this.tasksService.deleteTask(task.id).subscribe({
      next: () => this.loadTasks(),
      error: () => this.error.set('Failed to delete task')
    });
  }

  async onFileSelected(task: TaskItem, event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) {
      return;
    }

    this.uploadingTaskId.set(task.id);
    try {
      await this.tasksService.uploadAttachment(task.id, file);
      this.loadTasks();
    } catch {
      this.error.set('Failed to upload attachment — check console for CORS/network errors');
    } finally {
      this.uploadingTaskId.set(null);
      input.value = '';
    }
  }
}
