import apiService from '../../core/services/api.service';

class EmployeeSummaryApi {
  // Get all employees for dropdown
  async getEmployees() { 
    return (await apiService.get('/Employee')).data; 
  }
  
  // Get single employee detail
  async getEmployeeById(id) { 
    return (await apiService.get(`/Employee/${id}`)).data; 
  }
  
  // NEW: Get asset summary for employee (replaces /Asset/holder endpoint)
  async getAssetSummary(id) { 
    return (await apiService.get(`/Employee/${id}/asset-summary`)).data; 
  }
  
  // REMOVED: getAssetsByHolder (obsolete - replaced by getAssetSummary)
  // REMOVED: getTransactionsByEmployee (now included in asset summary)
}

export default new EmployeeSummaryApi();