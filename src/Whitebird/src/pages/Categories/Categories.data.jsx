import CategoriesApi from './Categories.api';
import Swal from 'sweetalert2';

class CategoriesData {
  constructor() {
    this.api = CategoriesApi;
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
        error: 'Failed to load categories'
      };
    }
  }

  async loadCategory(id) {
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
        error: 'Failed to load category'
      };
    }
  }

  async loadParentCategories() {
    try {
      const result = await this.api.getActiveOnly();
      return {
        success: true,
        data: result.data || []
      };
    } catch (error) {
      return {
        success: false,
        error: 'Failed to load parent categories'
      };
    }
  }

  async createCategory(data) {
    try {
      const result = await this.api.create(data);
      
      if (result.isSuccess) {
        Swal.fire({
          title: 'Success',
          text: 'Category created successfully',
          icon: 'success',
          confirmButtonColor: '#dc2626'
        });
        return { success: true, data: result.data };
      }
      
      Swal.fire({
        title: 'Error',
        text: result.message || 'Failed to create category',
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
      
      return { success: false, error: result.message };
    } catch (error) {
      Swal.fire({
        title: 'Error',
        text: 'Failed to create category',
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
      return { success: false, error: 'Failed to create category' };
    }
  }

  async updateCategory(id, data) {
    try {
      const result = await this.api.update(id, data);
      
      if (result.isSuccess) {
        Swal.fire({
          title: 'Success',
          text: 'Category updated successfully',
          icon: 'success',
          confirmButtonColor: '#dc2626'
        });
        return { success: true, data: result.data };
      }
      
      Swal.fire({
        title: 'Error',
        text: result.message || 'Failed to update category',
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
      
      return { success: false, error: result.message };
    } catch (error) {
      Swal.fire({
        title: 'Error',
        text: 'Failed to update category',
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
      return { success: false, error: 'Failed to update category' };
    }
  }

  async deleteCategory(id) {
    try {
      const result = await Swal.fire({
        title: 'Are you sure?',
        text: 'This category will be deleted.',
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
          text: 'Category has been deleted.',
          icon: 'success',
          confirmButtonColor: '#dc2626'
        });
        return { success: true };
      }
      
      Swal.fire({
        title: 'Error',
        text: deleteResult.message || 'Failed to delete category',
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
      
      return { success: false };
    } catch (error) {
      Swal.fire({
        title: 'Error',
        text: 'Failed to delete category',
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
      return { success: false };
    }
  }
}

export default CategoriesData;