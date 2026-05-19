import AssetsApi from './Assets.api';
import BaseData from '../../core/services/BaseData';

class AssetsData extends BaseData {
  constructor() {
    super(AssetsApi);
  }

  getCreateMessage() { return 'Asset created successfully'; }
  getUpdateMessage() { return 'Asset updated successfully'; }
  getDeleteMessage() { return 'Asset deleted successfully'; }
  getDeleteTitle() { return 'Delete Asset'; }
  getDeleteText() { return 'Are you sure you want to delete this asset?'; }
}

export default AssetsData;