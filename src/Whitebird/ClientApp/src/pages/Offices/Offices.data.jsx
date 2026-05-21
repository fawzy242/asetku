import OfficesApi from './Offices.api';
import BaseData from '../../core/services/BaseData';

class OfficesData extends BaseData {
  constructor() {
    super(OfficesApi);
  }

  getCreateMessage() { return 'Office created successfully'; }
  getUpdateMessage() { return 'Office updated successfully'; }
  getDeleteMessage() { return 'Office deleted successfully'; }
  getDeleteTitle() { return 'Delete Office'; }
  getDeleteText() { return 'Are you sure you want to delete this office? This may affect assets and employees assigned to it.'; }
}

export default OfficesData;