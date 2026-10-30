import { departmentApi } from '../api/api';
import BaseData from './BaseData';

class DepartmentsData extends BaseData {
  constructor() {
    super(departmentApi);
    this.setEntityName('department');
  }

  async bulkActivate(ids, activate) {
    return super.bulkActivate(ids, activate);
  }

  getCreateMessage() { return 'Department created successfully'; }
  getUpdateMessage() { return 'Department updated successfully'; }
  getDeleteMessage() { return 'Department deleted successfully'; }
  getDeleteTitle() { return 'Delete Department'; }
  getDeleteText() { return 'Are you sure you want to delete this department? This may affect employees assigned to it.'; }
}

export default DepartmentsData;