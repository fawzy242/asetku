// src/pages/Offices/Offices.data.js
import apiClient from '../../core/api/apiClient';
import BaseData from '../../core/services/BaseData';
import ConfirmDialog from '../../components/molecules/ConfirmDialog/ConfirmDialog';
import { handleSuccess } from '../../core/services/successHandler';

class OfficesData extends BaseData {
  constructor() {
    super(apiClient.office);
    this.setEntityName('office');
  }

  /**
   * Bulk activate or deactivate offices
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
          entityName: 'office',
          message: `${count} office(s) ${actionText}d successfully`,
          result: result.data,
        });
        
        return { success: true, data: count };
      }
      
      ConfirmDialog.toast.success(`${ids.length} office(s) ${actionText}d successfully`);
      return { success: true };
    } catch (error) {
      return {
        success: false,
        error: error.userMessage || error.message || `Failed to ${actionText}`,
      };
    }
  }

  getCreateMessage() { return 'Office created successfully'; }
  getUpdateMessage() { return 'Office updated successfully'; }
  getDeleteMessage() { return 'Office deleted successfully'; }
  getDeleteTitle() { return 'Delete Office'; }
  getDeleteText() { return 'Are you sure you want to delete this office? This may affect assets and employees assigned to it.'; }
}

export default OfficesData;