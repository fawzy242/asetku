import CategoriesApi from './Categories.api';
import BaseData from '../../core/services/BaseData';

class CategoriesData extends BaseData {
  constructor() {
    super(CategoriesApi);
  }

  async fetchParentCategories() {
    try {
      const result = await this.api.getActiveOnly();
      return { success: true, data: result.data || [] };
    } catch {
      return { success: false, error: 'Failed to load parent categories' };
    }
  }

  getCreateMessage() { return 'Category created successfully'; }
  getUpdateMessage() { return 'Category updated successfully'; }
  getDeleteMessage() { return 'Category deleted successfully'; }
  getDeleteTitle() { return 'Delete Category'; }
  getDeleteText() { return 'Are you sure you want to delete this category?'; }
}

export default CategoriesData;