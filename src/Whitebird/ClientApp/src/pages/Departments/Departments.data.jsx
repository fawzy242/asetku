import DepartmentsApi from './Departments.api';
import BaseData from '../../core/services/BaseData';

class DepartmentsData extends BaseData {
  constructor() {
    super(DepartmentsApi);
  }

  getCreateMessage() { return 'Department created successfully'; }
  getUpdateMessage() { return 'Department updated successfully'; }
  getDeleteMessage() { return 'Department deleted successfully'; }
  getDeleteTitle() { return 'Delete Department'; }
  getDeleteText() { return 'Are you sure you want to delete this department? This may affect employees assigned to it.'; }
}

export default DepartmentsData;