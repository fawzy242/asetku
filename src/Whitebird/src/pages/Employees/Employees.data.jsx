import EmployeesApi from './Employees.api';
import Swal from 'sweetalert2';

class EmployeesData {
  constructor() {
    this.api = EmployeesApi;
  }

  async loadGridData(page, pageSize, search, sortBy, sortDescending) {
    try {
      const result = await this.api.getGridData({
        page,
        pageSize,
        search,
        sortBy,
        sortDescending
      });
      
      return {
        success: true,
        data: result.data
      };
    } catch (error) {
      return {
        success: false,
        error: 'Failed to load employees'
      };
    }
  }

  async loadEmployee(id) {
    try {
      const result = await this.api.getById(id);
      
      if (result.isSuccess) {
        return {
          success: true,
          data: result.data
        };
      }
      
      return {
        success: false,
        error: result.message
      };
    } catch (error) {
      return {
        success: false,
        error: 'Failed to load employee'
      };
    }
  }

  async createEmployee(data) {
    try {
      const result = await this.api.create(data);
      
      if (result.isSuccess) {
        Swal.fire({
          title: 'Success',
          text: 'Employee created successfully',
          icon: 'success',
          confirmButtonColor: '#dc2626'
        });
        return { success: true, data: result.data };
      }
      
      Swal.fire({
        title: 'Error',
        text: result.message || 'Failed to create employee',
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
      
      return { success: false, error: result.message };
    } catch (error) {
      Swal.fire({
        title: 'Error',
        text: 'Failed to create employee',
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
      return { success: false, error: 'Failed to create employee' };
    }
  }

  async updateEmployee(id, data) {
    try {
      const result = await this.api.update(id, data);
      
      if (result.isSuccess) {
        Swal.fire({
          title: 'Success',
          text: 'Employee updated successfully',
          icon: 'success',
          confirmButtonColor: '#dc2626'
        });
        return { success: true, data: result.data };
      }
      
      Swal.fire({
        title: 'Error',
        text: result.message || 'Failed to update employee',
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
      
      return { success: false, error: result.message };
    } catch (error) {
      Swal.fire({
        title: 'Error',
        text: 'Failed to update employee',
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
      return { success: false, error: 'Failed to update employee' };
    }
  }

  async deleteEmployee(id) {
    try {
      const result = await Swal.fire({
        title: 'Are you sure?',
        text: 'This employee will be deleted.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#dc2626',
        confirmButtonText: 'Yes, delete it'
      });
      
      if (!result.isConfirmed) {
        return { success: false, cancelled: true };
      }
      
      const deleteResult = await this.api.delete(id);
      
      if (deleteResult.isSuccess) {
        Swal.fire({
          title: 'Deleted',
          text: 'Employee has been deleted.',
          icon: 'success',
          confirmButtonColor: '#dc2626'
        });
        return { success: true };
      }
      
      Swal.fire({
        title: 'Error',
        text: deleteResult.message || 'Failed to delete employee',
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
      
      return { success: false };
    } catch (error) {
      Swal.fire({
        title: 'Error',
        text: 'Failed to delete employee',
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
      return { success: false };
    }
  }
}

export default EmployeesData;