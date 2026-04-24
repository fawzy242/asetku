import apiService from '../../core/services/api.service';
import utilsHelper from '../../core/utils/utils.helper';

class EmployeesApi {
  async getAll() { return (await apiService.get('/Employee')).data; }
  async getById(id) { return (await apiService.get(`/Employee/${id}`)).data; }
  async getGridData(params) { return (await apiService.get(`/Employee/grid${utilsHelper.buildQueryString(params)}`)).data; }
  async getByDepartment(department) { return (await apiService.get(`/Employee/department/${department}`)).data; }
  async getByStatus(status) { return (await apiService.get(`/Employee/status/${status}`)).data; }
  async create(data) { return (await apiService.post('/Employee', data)).data; }
  async update(id, data) { return (await apiService.put(`/Employee/${id}`, data)).data; }
  async delete(id) { return (await apiService.delete(`/Employee/${id}`)).data; }
}

export default new EmployeesApi();