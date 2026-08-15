import apiService from '../services/api.service';

export const employeeSummaryApi = {
  lookup: (employeeId) => apiService.get(`/EmployeeSummary/lookup/${employeeId}`),
  getDetail: (employeeId) => apiService.get(`/EmployeeSummary/${employeeId}`),
  getStats: (employeeId) => apiService.get(`/EmployeeSummary/${employeeId}/stats`),
  getCurrentHolding: (employeeId, params) =>
    apiService.get(`/EmployeeSummary/${employeeId}/current-holding`, { params }),
  getAssetsHistory: (employeeId, params) =>
    apiService.get(`/EmployeeSummary/${employeeId}/assets-history`, { params }),
  getTransactionHistory: (employeeId, params) =>
    apiService.get(`/EmployeeSummary/${employeeId}/transaction-history`, { params }),
  getCurrentHoldingStats: (employeeId) =>
    apiService.get(`/EmployeeSummary/${employeeId}/current-holding-stats`),
  getTransactionHistoryStats: (employeeId) =>
    apiService.get(`/EmployeeSummary/${employeeId}/transaction-history-stats`),
  getScheduleStats: (employeeId) =>
    apiService.get(`/EmployeeSummary/${employeeId}/schedule-stats`),
  getPhoto: (employeeId) => apiService.get(`/EmployeeSummary/${employeeId}/photo`, { responseType: 'blob' }),
  getAttachments: (employeeId) => apiService.get(`/EmployeeSummary/${employeeId}/attachments`),
};