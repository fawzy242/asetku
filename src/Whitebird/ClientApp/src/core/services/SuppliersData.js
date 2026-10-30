import { supplierApi } from '../api/api';
import BaseData from './BaseData';

class SuppliersData extends BaseData {
  constructor() {
    super(supplierApi);
    this.setEntityName('supplier');
  }

  async bulkActivate(ids, activate) {
    return super.bulkActivate(ids, activate);
  }

  getCreateMessage() { return 'Supplier created successfully'; }
  getUpdateMessage() { return 'Supplier updated successfully'; }
  getDeleteMessage() { return 'Supplier deleted successfully'; }
  getDeleteTitle() { return 'Delete Supplier'; }
  getDeleteText() { return 'Are you sure you want to delete this supplier?'; }
}

export default SuppliersData;