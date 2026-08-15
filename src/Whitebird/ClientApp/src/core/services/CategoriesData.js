import { categoryApi } from '../api/category.api';
import BaseData from './BaseData';

class CategoriesData extends BaseData {
  constructor() {
    super(categoryApi);
    this.setEntityName('category');
  }

  async fetchParentCategories() {
    try {
      const result = await this.api.dropdown();
      return { success: true, data: result.data?.data || result.data || [] };
    } catch (error) {
      if (this.isCanceledError(error)) {
        return { success: false, data: [], error: 'canceled' };
      }
      return {
        success: false,
        data: [],
        error: error.userMessage || error.message || 'Failed to load parent categories'
      };
    }
  }

  async bulkActivate(ids, activate) {
    return super.bulkActivate(ids, activate);
  }

  getCreateMessage() { return 'Category created successfully'; }
  getUpdateMessage() { return 'Category updated successfully'; }
  getDeleteMessage() { return 'Category deleted successfully'; }
  getDeleteTitle() { return 'Delete Category'; }
  getDeleteText() { return 'Are you sure you want to delete this category?'; }
}

export default CategoriesData;