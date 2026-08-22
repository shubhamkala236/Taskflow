import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

import { environment } from '../../environments/environment';
import {
  CreateTaskRequest,
  RequestUploadSasResponse,
  TaskAttachment,
  TaskItem,
  UpdateTaskRequest
} from './task.model';

@Injectable({ providedIn: 'root' })
export class TasksService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/tasks`;

  getTasks() {
    return this.http.get<TaskItem[]>(this.baseUrl);
  }

  createTask(request: CreateTaskRequest) {
    return this.http.post<TaskItem>(this.baseUrl, request);
  }

  updateTask(id: string, request: UpdateTaskRequest) {
    return this.http.put<TaskItem>(`${this.baseUrl}/${id}`, request);
  }

  deleteTask(id: string) {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  private requestUploadSas(taskId: string, fileName: string, contentType: string) {
    return this.http.post<RequestUploadSasResponse>(`${this.baseUrl}/${taskId}/attachments/sas`, {
      fileName,
      contentType
    });
  }

  private uploadToBlob(uploadUrl: string, file: File) {
    return this.http.put(uploadUrl, file, {
      headers: {
        'x-ms-blob-type': 'BlockBlob',
        'Content-Type': file.type || 'application/octet-stream'
      }
    });
  }

  private confirmAttachment(taskId: string, attachmentId: string) {
    return this.http.post<void>(`${this.baseUrl}/${taskId}/attachments/${attachmentId}/confirm`, {});
  }

  async uploadAttachment(taskId: string, file: File): Promise<void> {
    const sas = await firstValueFrom(this.requestUploadSas(taskId, file.name, file.type || 'application/octet-stream'));
    await firstValueFrom(this.uploadToBlob(sas.uploadUrl, file));
    await firstValueFrom(this.confirmAttachment(taskId, sas.attachmentId));
  }

  getAttachments(taskId: string) {
    return this.http.get<TaskAttachment[]>(`${this.baseUrl}/${taskId}/attachments`);
  }
}
