import SuppliersApi from './Suppliers.api';
import BaseData from '../../core/services/BaseData';

class SuppliersData extends BaseData {
  constructor() {
    super(SuppliersApi);
  }

  getCreateMessage() { return 'Supplier created successfully'; }
  getUpdateMessage() { return 'Supplier updated successfully'; }
  getDeleteMessage() { return 'Supplier deleted successfully'; }
  getDeleteTitle() { return 'Delete Supplier'; }
  getDeleteText() { return 'Are you sure you want to delete this supplier?'; }
}

export default SuppliersData;