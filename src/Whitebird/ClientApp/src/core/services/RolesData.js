// src/core/services/RolesData.js
import { rolesApi } from '../api/api';
import BaseData from './BaseData';

class RolesData extends BaseData {
  constructor() {
    super(rolesApi);
    this.setEntityName('role');
  }

  async bulkActivate(ids, activate) {
    return super.bulkActivate(ids, activate);
  }

  getCreateMessage() {
    return 'Role created successfully';
  }
  getUpdateMessage() {
    return 'Role updated successfully';
  }
  getDeleteMessage() {
    return 'Role deleted successfully';
  }
  getDeleteTitle() {
    return 'Delete Role';
  }
  getDeleteText() {
    return 'Are you sure you want to delete this role? This may affect users assigned to it.';
  }
}

export default RolesData;