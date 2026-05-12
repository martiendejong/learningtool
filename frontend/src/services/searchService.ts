import api from './api';

export interface SearchResults {
  skills: { id: number; name: string; description: string; difficulty: string }[];
  topics: { id: number; name: string; description: string; skillName: string }[];
  courses: { id: number; name: string; description: string; topicName: string; skillName: string }[];
}

export const searchService = {
  async search(query: string): Promise<SearchResults> {
    const response = await api.get<SearchResults>('/api/search', { params: { q: query } });
    return response.data;
  },
};
