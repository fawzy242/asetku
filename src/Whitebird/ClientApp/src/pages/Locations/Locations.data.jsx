import LocationsApi from './Locations.api';
import BaseData from '../../core/services/BaseData';

class LocationsData extends BaseData {
  constructor() {
    super(LocationsApi);
  }

  async fetchParentLocations() {
    try {
      const result = await this.api.getActiveOnly();
      return { success: true, data: result.data || [] };
    } catch {
      return { success: false, error: 'Failed to load parent locations' };
    }
  }

  getCreateMessage() { return 'Location created successfully'; }
  getUpdateMessage() { return 'Location updated successfully'; }
  getDeleteMessage() { return 'Location deleted successfully'; }
  getDeleteTitle() { return 'Delete Location'; }
  getDeleteText() { return 'Are you sure you want to delete this location?'; }
}

export default LocationsData;