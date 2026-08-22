export interface TaskAttachment {
  id: string;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  uploadedAt: string;
}

export interface TaskItem {
  id: string;
  title: string;
  description: string | null;
  isComplete: boolean;
  createdAt: string;
  attachments: TaskAttachment[];
}

export interface CreateTaskRequest {
  title: string;
  description: string | null;
}

export interface UpdateTaskRequest {
  title: string;
  description: string | null;
  isComplete: boolean;
}

export interface RequestUploadSasResponse {
  attachmentId: string;
  uploadUrl: string;
  blobName: string;
}
