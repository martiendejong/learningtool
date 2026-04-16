import api from './api';

export interface CodeSnippet {
  id: number;
  userId: string;
  courseId?: number | null;
  title: string;
  code: string;
  language: string;
  description?: string | null;
  tags: string[];
  isPublic: boolean;
  createdAt: string;
  updatedAt?: string | null;
}

export interface CreateSnippetPayload {
  title: string;
  code: string;
  language: string;
  description?: string | null;
  tags?: string[];
  courseId?: number | null;
  isPublic?: boolean;
}

export interface UpdateSnippetPayload {
  title: string;
  code: string;
  language: string;
  description?: string | null;
  tags?: string[];
  isPublic?: boolean;
}

export interface SnippetListFilters {
  language?: string;
  search?: string;
}

export const snippetService = {
  async list(filters: SnippetListFilters = {}): Promise<CodeSnippet[]> {
    const params: Record<string, string> = {};
    if (filters.language) params.language = filters.language;
    if (filters.search) params.search = filters.search;
    const response = await api.get<CodeSnippet[]>('/snippets', { params });
    return response.data;
  },

  async get(id: number): Promise<CodeSnippet> {
    const response = await api.get<CodeSnippet>(`/snippets/${id}`);
    return response.data;
  },

  async create(payload: CreateSnippetPayload): Promise<CodeSnippet> {
    const response = await api.post<CodeSnippet>('/snippets', {
      title: payload.title,
      code: payload.code,
      language: payload.language,
      description: payload.description ?? null,
      tags: payload.tags ?? [],
      courseId: payload.courseId ?? null,
      isPublic: payload.isPublic ?? false,
    });
    return response.data;
  },

  async update(id: number, payload: UpdateSnippetPayload): Promise<CodeSnippet> {
    const response = await api.put<CodeSnippet>(`/snippets/${id}`, {
      title: payload.title,
      code: payload.code,
      language: payload.language,
      description: payload.description ?? null,
      tags: payload.tags ?? [],
      isPublic: payload.isPublic ?? false,
    });
    return response.data;
  },

  async delete(id: number): Promise<void> {
    await api.delete(`/snippets/${id}`);
  },
};
