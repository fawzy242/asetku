import AssetsApi from './Assets.api';
import Swal from 'sweetalert2';

class AssetsData {
  constructor() {
    this.api = AssetsApi;
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
        error: 'Failed to load assets'
      };
    }
  }

  async loadAsset(id) {
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
        error: 'Failed to load asset'
      };
    }
  }

  async createAsset(data) {
    try {
      const result = await this.api.create(data);
      
      if (result.isSuccess) {
        Swal.fire({
          title: 'Success',
          text: 'Asset created successfully',
          icon: 'success',
          confirmButtonColor: '#dc2626'
        });
        return { success: true, data: result.data };
      }
      
      Swal.fire({
        title: 'Error',
        text: result.message || 'Failed to create asset',
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
      
      return { success: false, error: result.message };
    } catch (error) {
      Swal.fire({
        title: 'Error',
        text: 'Failed to create asset',
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
      return { success: false, error: 'Failed to create asset' };
    }
  }

  async updateAsset(id, data) {
    try {
      const result = await this.api.update(id, data);
      
      if (result.isSuccess) {
        Swal.fire({
          title: 'Success',
          text: 'Asset updated successfully',
          icon: 'success',
          confirmButtonColor: '#dc2626'
        });
        return { success: true, data: result.data };
      }
      
      Swal.fire({
        title: 'Error',
        text: result.message || 'Failed to update asset',
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
      
      return { success: false, error: result.message };
    } catch (error) {
      Swal.fire({
        title: 'Error',
        text: 'Failed to update asset',
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
      return { success: false, error: 'Failed to update asset' };
    }
  }

  async deleteAsset(id) {
    try {
      const result = await Swal.fire({
        title: 'Are you sure?',
        text: 'This asset will be deleted.',
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
          text: 'Asset has been deleted.',
          icon: 'success',
          confirmButtonColor: '#dc2626'
        });
        return { success: true };
      }
      
      Swal.fire({
        title: 'Error',
        text: deleteResult.message || 'Failed to delete asset',
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
      
      return { success: false };
    } catch (error) {
      Swal.fire({
        title: 'Error',
        text: 'Failed to delete asset',
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
      return { success: false };
    }
  }

  async loadDropdownData() {
    try {
      const [categories, suppliers, employees] = await Promise.all([
        this.api.getCategories(),
        this.api.getSuppliers(),
        this.api.getEmployees()
      ]);
      
      return {
        success: true,
        data: {
          categories: categories.data || [],
          suppliers: suppliers.data || [],
          employees: employees.data || []
        }
      };
    } catch (error) {
      return {
        success: false,
        error: 'Failed to load dropdown data'
      };
    }
  }
}

export default AssetsData;