// src/pages/Departments/Departments.data.js
import apiClient from '../../core/api/apiClient';
import BaseData from '../../core/services/BaseData';
import ConfirmDialog from '../../components/molecules/ConfirmDialog/ConfirmDialog';
import { handleSuccess } from '../../core/services/successHandler';

class DepartmentsData extends BaseData {
  constructor() {
    super(apiClient.department);
    this.setEntityName('department');
  }

  /**
   * Bulk activate or deactivate departments
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
          entityName: 'department',
          message: `${count} department(s) ${actionText}d successfully`,
          result: result.data,
        });
        
        return { success: true, data: count };
      }
      
      ConfirmDialog.toast.success(`${ids.length} department(s) ${actionText}d successfully`);
      return { success: true };
    } catch (error) {
      return {
        success: false,
        error: error.userMessage || error.message || `Failed to ${actionText}`,
      };
    }
  }

  getCreateMessage() { return 'Department created successfully'; }
  getUpdateMessage() { return 'Department updated successfully'; }
  getDeleteMessage() { return 'Department deleted successfully'; }
  getDeleteTitle() { return 'Delete Department'; }
  getDeleteText() { return 'Are you sure you want to delete this department? This may affect employees assigned to it.'; }
}

export default DepartmentsData;