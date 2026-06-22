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
  
  // Get asset summary for employee
  async getAssetSummary(id) { 
    return (await apiService.get(`/Employee/${id}/asset-summary`)).data; 
  }
}

export default new EmployeeSummaryApi();