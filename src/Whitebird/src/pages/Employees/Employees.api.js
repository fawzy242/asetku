import apiService from '../../core/services/api.service';
import utilsHelper from '../../core/utils/utils.helper';

class EmployeesApi {
  async getAll() {
    const response = await apiService.get('/employee');
    return response.data;
  }

  async getById(id) {
    const response = await apiService.get(`/employee/${id}`);
    return response.data;
  }

  async getGridData(params) {
    const queryString = utilsHelper.buildQueryString(params);
    const response = await apiService.get(`/employee/grid${queryString}`);
    return response.data;
  }

  async getByDepartment(department) {
    const response = await apiService.get(`/employee/department/${department}`);
    return response.data;
  }

  async getByStatus(status) {
    const response = await apiService.get(`/employee/status/${status}`);
    return response.data;
  }

  async create(data) {
    const response = await apiService.post('/employee', data);
    return response.data;
  }

  async update(id, data) {
    const response = await apiService.put(`/employee/${id}`, data);
    return response.data;
  }

  async delete(id) {
    const response = await apiService.delete(`/employee/${id}`);
    return response.data;
  }

  async softDelete(id) {
    const response = await apiService.delete(`/employee/${id}/soft`);
    return response.data;
  }
}

export default new EmployeesApi();