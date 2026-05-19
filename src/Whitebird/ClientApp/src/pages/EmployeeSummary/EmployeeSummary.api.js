import apiService from '../../core/services/api.service';

class EmployeeSummaryApi {
  async getEmployees() { return (await apiService.get('/Employee')).data; }
  async getEmployeeById(id) { return (await apiService.get(`/Employee/${id}`)).data; }
  async getAssetSummary(id) { return (await apiService.get(`/Employee/${id}/asset-summary`)).data; }
  async getAssetsByHolder(employeeId) { return (await apiService.get(`/Asset/holder/${employeeId}`)).data; }
  async getTransactionsByEmployee(employeeId) { return (await apiService.get(`/AssetTransaction/employee/${employeeId}`)).data; }
}

export default new EmployeeSummaryApi();