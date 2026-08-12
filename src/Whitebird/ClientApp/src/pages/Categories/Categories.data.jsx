// src/pages/Categories/Categories.data.js
import apiClient from '../../core/api/apiClient';
import BaseData from '../../core/services/BaseData';
import ConfirmDialog from '../../components/molecules/ConfirmDialog/ConfirmDialog';
import { handleSuccess } from '../../core/services/successHandler';

class CategoriesData extends BaseData {
  constructor() {
    super(apiClient.category);
    this.setEntityName('category');
  }

  async fetchParentCategories() {
    try {
      const result = await this.api.dropdown();
      return { success: true, data: result.data?.data || result.data || [] };
    } catch (error) {
      return {
        success: false,
        error: error.userMessage || error.message || 'Failed to load parent categories'
      };
    }
  }

  /**
   * Bulk activate or deactivate categories
   */
  async bulkActivate(ids, activate) {
    const actionText = activate ? 'activate' : 'deactivate';
    
    try {
      const endpoint = activate ? this.api.activate : this.api.deactivate;
      const result = await endpoint({ ids });
      
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          const errorMsg = result.data.errors?.join('\n') || result.data.message || `Failed to ${actionText}`;
          ConfirmDialog.toast.error(errorMsg);
          return { success: false };
        }
        
        const count = result.data.data || ids.length;
        handleSuccess({
          operation: activate ? 'activate' : 'deactivate',
          entityName: 'category',
          message: `${count} categor(y/ies) ${actionText}d successfully`,
          result: result.data,
        });
        
        return { success: true, data: count };
      }
      
      ConfirmDialog.toast.success(`${ids.length} categor(y/ies) ${actionText}d successfully`);
      return { success: true };
    } catch (error) {
      return {
        success: false,
        error: error.userMessage || error.message || `Failed to ${actionText}`,
      };
    }
  }

  getCreateMessage() { return 'Category created successfully'; }
  getUpdateMessage() { return 'Category updated successfully'; }
  getDeleteMessage() { return 'Category deleted successfully'; }
  getDeleteTitle() { return 'Delete Category'; }
  getDeleteText() { return 'Are you sure you want to delete this category?'; }
}

export default CategoriesData;