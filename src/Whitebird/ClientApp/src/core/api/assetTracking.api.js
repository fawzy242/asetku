import apiService from '../services/api.service';

export const assetTrackingApi = {
  lookup: (assetId) => apiService.get(`/AssetTracking/lookup/${assetId}`),
  getDetail: (assetId) => apiService.get(`/AssetTracking/${assetId}`),
  getStats: (assetId) => apiService.get(`/AssetTracking/${assetId}/stats`),
  getTimeline: (assetId) => apiService.get(`/AssetTracking/${assetId}/timeline`),
  getScheduleStats: (assetId, params) =>
    apiService.get(`/AssetTracking/${assetId}/schedule-stats`, { params }),
  getTransactionTypeStats: (assetId) =>
    apiService.get(`/AssetTracking/${assetId}/transactiontype-stats`),
  getEmployeeHistory: (assetId, params) =>
    apiService.get(`/AssetTracking/${assetId}/employee-history`, { params }),
  getPhoto: (assetId) => apiService.get(`/AssetTracking/${assetId}/photo`, { responseType: 'blob' }),
  getAttachments: (assetId) => apiService.get(`/AssetTracking/${assetId}/attachments`),
};