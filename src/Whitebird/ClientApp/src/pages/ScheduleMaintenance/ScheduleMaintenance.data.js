// src/pages/ScheduleMaintenance/ScheduleMaintenance.data.js
import apiClient from '../../core/api/apiClient';
import BaseData from '../../core/services/BaseData';
import ConfirmDialog from '../../components/molecules/ConfirmDialog/ConfirmDialog';
import { handleSuccess } from '../../core/services/successHandler';

class ScheduleMaintenanceData extends BaseData {
  constructor() {
    super(apiClient.scheduleMaintenance);
    this.setEntityName('schedule');
  }

  /**
   * Bulk activate or deactivate schedules
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
          entityName: 'schedule',
          message: `${count} schedule(s) ${actionText}d successfully`,
          result: result.data,
        });
        
        return { success: true, data: count };
      }
      
      ConfirmDialog.toast.success(`${ids.length} schedule(s) ${actionText}d successfully`);
      return { success: true };
    } catch (error) {
      return {
        success: false,
        error: error.userMessage || error.message || `Failed to ${actionText}`,
      };
    }
  }

  /**
   * Generate schedules in bulk
   */
  async generate(data) {
    try {
      const result = await this.api.generate(data);
      
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          const errorMsg = result.data.errors?.join('\n') || result.data.message || 'Failed to generate';
          ConfirmDialog.toast.error(errorMsg);
          return { success: false };
        }
        
        const generated = result.data.data?.totalGenerated || 0;
        handleSuccess({
          operation: 'generate',
          entityName: 'schedule',
          message: `Generated ${generated} schedule(s) successfully`,
          result: result.data,
        });
        
        return { success: true, data: result.data.data };
      }
      
      ConfirmDialog.toast.success('Schedules generated successfully');
      return { success: true };
    } catch (error) {
      return {
        success: false,
        error: error.userMessage || error.message || 'Failed to generate',
      };
    }
  }

  getCreateMessage() { return 'Schedule created successfully'; }
  getUpdateMessage() { return 'Schedule updated successfully'; }
  getDeleteMessage() { return 'Schedule deleted successfully'; }
  getDeleteTitle() { return 'Delete Schedule'; }
  getDeleteText() { return 'Are you sure you want to delete this schedule?'; }
}

export default ScheduleMaintenanceData;