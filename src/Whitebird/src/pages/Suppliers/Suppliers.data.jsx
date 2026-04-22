import SuppliersApi from './Suppliers.api';
import Swal from 'sweetalert2';

class SuppliersData {
  constructor() {
    this.api = SuppliersApi;
  }

  async loadGridData(page, pageSize, search) {
    try {
      const result = await this.api.getGridData({ page, pageSize, search });
      
      return {
        success: true,
        data: result.data
      };
    } catch (error) {
      return {
        success: false,
        error: 'Failed to load suppliers'
      };
    }
  }

  async loadSupplier(id) {
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
        error: 'Failed to load supplier'
      };
    }
  }

  async createSupplier(data) {
    try {
      const result = await this.api.create(data);
      
      if (result.isSuccess) {
        Swal.fire({
          title: 'Success',
          text: 'Supplier created successfully',
          icon: 'success',
          confirmButtonColor: '#dc2626'
        });
        return { success: true, data: result.data };
      }
      
      Swal.fire({
        title: 'Error',
        text: result.message || 'Failed to create supplier',
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
      
      return { success: false, error: result.message };
    } catch (error) {
      Swal.fire({
        title: 'Error',
        text: 'Failed to create supplier',
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
      return { success: false, error: 'Failed to create supplier' };
    }
  }

  async updateSupplier(id, data) {
    try {
      const result = await this.api.update(id, data);
      
      if (result.isSuccess) {
        Swal.fire({
          title: 'Success',
          text: 'Supplier updated successfully',
          icon: 'success',
          confirmButtonColor: '#dc2626'
        });
        return { success: true, data: result.data };
      }
      
      Swal.fire({
        title: 'Error',
        text: result.message || 'Failed to update supplier',
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
      
      return { success: false, error: result.message };
    } catch (error) {
      Swal.fire({
        title: 'Error',
        text: 'Failed to update supplier',
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
      return { success: false, error: 'Failed to update supplier' };
    }
  }

  async deleteSupplier(id) {
    try {
      const result = await Swal.fire({
        title: 'Are you sure?',
        text: 'This supplier will be deleted.',
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
          text: 'Supplier has been deleted.',
          icon: 'success',
          confirmButtonColor: '#dc2626'
        });
        return { success: true };
      }
      
      Swal.fire({
        title: 'Error',
        text: deleteResult.message || 'Failed to delete supplier',
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
      
      return { success: false };
    } catch (error) {
      Swal.fire({
        title: 'Error',
        text: 'Failed to delete supplier',
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
      return { success: false };
    }
  }
}

export default SuppliersData;