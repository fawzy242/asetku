import apiService from './api.service';

class MasterDataService {
  async getAllGrouped() {
    return (await apiService.get('/MasterData')).data;
  }

  async getByReferenceName(referenceName) {
    return (await apiService.get(`/MasterData/${referenceName}`)).data;
  }

  async getTransactionTypes() {
    return (await apiService.get('/MasterData/transaction-types')).data;
  }

  async getAssetConditions() {
    return (await apiService.get('/MasterData/asset-conditions')).data;
  }

  async getEmployeePositions() {
    return (await apiService.get('/MasterData/employee-positions')).data;
  }

  async getEmployeeStatuses() {
    return (await apiService.get('/MasterData/employee-statuses')).data;
  }

  async getOfficeTypes() {
    return (await apiService.get('/MasterData/office-types')).data;
  }

  async getMaintenanceTypes() {
    return (await apiService.get('/MasterData/maintenance-types')).data;
  }

  async getAssetConditionPurchases() {
    return (await apiService.get('/MasterData/asset-condition-purchases')).data;
  }
}

export default new MasterDataService();