import { homeApi } from '../api/home.api';
import { userApi } from '../api/user.api';
import BaseData from './BaseData';

class HomeData extends BaseData {
  constructor() {
    super({
      getGrid: () => Promise.reject('Not implemented'),
      getById: () => Promise.reject('Not implemented'),
      create: () => Promise.reject('Not implemented'),
      update: () => Promise.reject('Not implemented'),
      delete: () => Promise.reject('Not implemented'),
      softDelete: () => Promise.reject('Not implemented'),
    });
    this.setEntityName('search');
    this.api = homeApi;
  }

  async search(keyword) {
    try {
      const result = await this.api.search(keyword);
      
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          return { 
            success: false, 
            error: result.data.errors?.join('\n') || result.data.message || 'Search failed',
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
        error: error.userMessage || error.message || 'Search failed' 
      };
    }
  }

  async getUserPhoto() {
    try {
      const response = await userApi.getPhoto();
      if (response.data && response.data.size > 0) {
        return { success: true, data: response.data };
      }
      return { success: false, error: 'No photo found', isNotFound: true };
    } catch (error) {
      if (this.isCanceledError(error)) {
        return { success: false, error: 'canceled', isCanceled: true };
      }
      if (error.response?.status === 404) {
        return { success: false, error: 'not found', isNotFound: true };
      }
      return { 
        success: false, 
        error: error.message || 'Failed to load photo' 
      };
    }
  }
}

export default HomeData;