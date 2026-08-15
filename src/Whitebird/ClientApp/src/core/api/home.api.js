import apiService from '../services/api.service';

export const homeApi = {
  search: (keyword) => apiService.get(`/Home/search?keyword=${encodeURIComponent(keyword)}`),
};