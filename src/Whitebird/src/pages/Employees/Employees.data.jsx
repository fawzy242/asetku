import EmployeesApi from './Employees.api';
import BaseData from '../../core/services/BaseData';

class EmployeesData extends BaseData {
  constructor() {
    super(EmployeesApi);
  }

  async fetchDepartments() {
    try {
      const result = await this.api.getAll();
      const departments = [...new Set((result.data || []).map(e => e.department).filter(Boolean))];
      return { success: true, data: departments };
    } catch {
      return { success: false, error: 'Failed to load departments' };
    }
  }

  getCreateMessage() { return 'Employee created successfully'; }
  getUpdateMessage() { return 'Employee updated successfully'; }
  getDeleteMessage() { return 'Employee deleted successfully'; }
  getDeleteTitle() { return 'Delete Employee'; }
  getDeleteText() { return 'Are you sure you want to delete this employee?'; }
}

export default EmployeesData;