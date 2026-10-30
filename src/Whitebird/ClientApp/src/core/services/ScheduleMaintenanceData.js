import { scheduleMaintenanceApi } from '../api/api';
import BaseData from './BaseData';

class ScheduleMaintenanceData extends BaseData {
  constructor() {
    super(scheduleMaintenanceApi);
    this.setEntityName('schedule');
  }

  async bulkActivate(ids, activate) {
    return super.bulkActivate(ids, activate);
  }

  async generate(data) {
    try {
      const result = await this.api.generate(data);

      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          return {
            success: false,
            error: result.data.errors?.join('\n') || result.data.message || 'Failed to generate',
            errorType: result.data.errorType,
          };
        }
        return { success: true, data: result.data.data };
      }

      return { success: true, data: result.data };
    } catch (error) {
      if (this.isCanceledError(error)) {
        return { success: false, error: 'canceled', isCanceled: true };
      }
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