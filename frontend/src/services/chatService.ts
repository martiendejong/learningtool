import api from './api';

export interface ChatMessage {
  id: number;
  userId: string;
  role: 'user' | 'assistant' | 'system';
  content: string;
  toolCalls?: string; // JSON serialized
  timestamp: string;
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
  async sendMessage(message: string): Promise<ChatResponse> {
    const response = await api.post<ChatResponse>('/chat/message', { message });
    return response.data;
  },

  async getHistory(limit = 50): Promise<ChatMessage[]> {
    const response = await api.get<ChatMessage[]>('/chat/history', { params: { limit } });
    return response.data;
  },

  async startCourse(courseId: number): Promise<ChatResponse> {
    const response = await api.post<ChatResponse>('/chat/start-course', { courseId });
    return response.data;
  },

  async clearHistory(): Promise<void> {
    await api.delete('/chat/history');
  },
};
