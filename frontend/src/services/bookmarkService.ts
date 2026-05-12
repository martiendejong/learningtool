import api from './api';

export interface Bookmark {
  id: number;
  courseId: number;
  courseName: string;
  courseDescription: string;
  note?: string;
  createdAt: string;
  updatedAt: string;
  skill?: { id: number; name: string };
  topic?: { id: number; name: string };
}

export const bookmarkService = {
  async getBookmarks(): Promise<Bookmark[]> {
    const response = await api.get<Bookmark[]>('/api/bookmarks');
    return response.data;
  },

  async toggleBookmark(courseId: number, note?: string): Promise<{ bookmarked: boolean }> {
    const response = await api.post<{ bookmarked: boolean }>('/api/bookmarks/toggle', { courseId, note });
    return response.data;
  },

  async updateNote(bookmarkId: number, note: string): Promise<void> {
    await api.put(`/api/bookmarks/${bookmarkId}/note`, { note });
  },

  async isBookmarked(courseId: number): Promise<boolean> {
    const response = await api.get<{ bookmarked: boolean }>(`/api/bookmarks/check/${courseId}`);
    return response.data.bookmarked;
  },
};
