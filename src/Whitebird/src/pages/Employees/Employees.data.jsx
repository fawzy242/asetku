import EmployeesApi from './Employees.api';
import ConfirmDialog from '../../components/molecules/ConfirmDialog/ConfirmDialog';

class EmployeesData {
  constructor() { this.api = EmployeesApi; }

  async fetchGridData({ page, pageSize, search = '', sortBy = 'fullName', sortDescending = false, status = '', department = '' }) {
    try {
      const result = await this.api.getGridData({ page, pageSize, search, sortBy, sortDescending, status, department });
      return { success: true, data: result.data };
    } catch { return { success: false, error: 'Failed to load employees' }; }
  }

  async fetchById(id) {
    try {
      const result = await this.api.getById(id);
      return result.isSuccess ? { success: true, data: result.data } : { success: false, error: result.message };
    } catch { return { success: false, error: 'Failed to load employee' }; }
  }

  async fetchDepartments() {
    try {
      const result = await this.api.getAll();
      const departments = [...new Set((result.data || []).map(e => e.department).filter(Boolean))];
      return { success: true, data: departments };
    } catch { return { success: false, error: 'Failed to load departments' }; }
  }

  async create(data) {
    try {
      const result = await this.api.create(data);
      if (result.isSuccess) { await ConfirmDialog.showSuccess('Success', 'Employee created'); return { success: true, data: result.data }; }
      await ConfirmDialog.showError('Error', result.message || 'Failed to create');
      return { success: false };
    } catch { await ConfirmDialog.showError('Error', 'Failed to create'); return { success: false }; }
  }

  async update(id, data) {
    try {
      const result = await this.api.update(id, data);
      if (result.isSuccess) { await ConfirmDialog.showSuccess('Success', 'Employee updated'); return { success: true }; }
      await ConfirmDialog.showError('Error', result.message || 'Failed to update');
      return { success: false };
    } catch { await ConfirmDialog.showError('Error', 'Failed to update'); return { success: false }; }
  }

  async delete(id) {
    const confirmed = await ConfirmDialog.showDelete('Delete Employee', 'Are you sure?');
    if (!confirmed) return { success: false, cancelled: true };
    try {
      const result = await this.api.delete(id);
      if (result.isSuccess) { await ConfirmDialog.showSuccess('Deleted', 'Employee deleted'); return { success: true }; }
      await ConfirmDialog.showError('Error', result.message || 'Failed to delete');
      return { success: false };
    } catch { await ConfirmDialog.showError('Error', 'Failed to delete'); return { success: false }; }
  }
}

export default EmployeesData;