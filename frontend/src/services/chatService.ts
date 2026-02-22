import api from './api';

// Hazina paged response
interface PagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface ChatMessage {
  id: string;  // GUID
  userId: string;
  role: 'user' | 'assistant' | 'system';
  content: string;
  courseId?: string;  // GUID, optional
  toolCalls?: string; // JSON serialized
  createdAt: string;
  updatedAt?: string;
  timestamp?: string; // Alias for createdAt for backwards compatibility
}

export interface ToolCall {
  id: string;
  toolName: string;
  arguments: Record<string, any>;
}

export interface ToolResult {
  toolCallId: string;
  success: boolean;
  result: string;
  data?: any;
}

export interface ChatResponse {
  message: string;
  toolCalls?: ToolCall[];
  requiresAction: boolean;
  toolResults?: ToolResult[];
}

export const chatService = {
  // Note: This endpoint would need a custom controller implementation
  // The Hazina entities.yaml doesn't handle AI chat logic, only storage
  async sendMessage(message: string, courseId?: string): Promise<ChatResponse> {
    // This would need to be implemented in a custom ChatController
    const response = await api.post<ChatResponse>('/chat/message', { message, courseId });
    return response.data;
  },

  async getHistory(limit = 50, courseId?: string): Promise<ChatMessage[]> {
    const params: any = { limit };
    if (courseId) {
      params.courseId = courseId;
    }

    // Use ChatController endpoint which filters by userId automatically
    const response = await api.get<ChatMessage[]>('/chat/history', { params });
    return response.data;
  },

  async startCourse(courseId: string): Promise<ChatResponse> {
    // This would need to be implemented in a custom ChatController
    const response = await api.post<ChatResponse>('/chat/start-course', { courseId });
    return response.data;
  },

  async clearHistory(): Promise<void> {
    // Delete all chat messages for current user
    // This would need implementation - either soft delete all or custom endpoint
    const messages = await this.getHistory(1000);
    for (const message of messages) {
      await api.delete(`/chatmessage/${message.id}`);
    }
  },

  // New method to save a message directly to Hazina
  async saveMessage(role: 'user' | 'assistant' | 'system', content: string, courseId?: string): Promise<ChatMessage> {
    const response = await api.post<ChatMessage>('/chatmessage', {
      role,
      content,
      courseId: courseId || null
    });
    return response.data;
  },
};
