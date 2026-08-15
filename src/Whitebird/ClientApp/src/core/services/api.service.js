import axios from 'axios';
import { API_CONFIG } from '../constants/appConstants';
import { STORAGE_KEYS } from '../constants/storageKeys';
import { handleApiError, shouldIgnoreError } from './apiErrorHandler';

class ApiService {
  constructor() {
    this.pendingRequests = new Map();
    this.instance = axios.create({
      baseURL: API_CONFIG.BASE_URL,
      timeout: API_CONFIG.TIMEOUT,
      headers: { 
        'Content-Type': 'application/json', 
        'Accept': 'application/json' 
      }
    });
    this.setupInterceptors();
  }

  setupInterceptors() {
    this.instance.interceptors.request.use(
      (config) => {
        const token = localStorage.getItem(STORAGE_KEYS.SESSION_TOKEN);
        if (token) {
          config.headers['X-Session-Token'] = token;
        }
        return config;
      },
      (error) => Promise.reject(error)
    );

    this.instance.interceptors.response.use(
      (response) => response,
      (error) => {
        if (axios.isCancel(error) || shouldIgnoreError(error)) {
          return Promise.resolve({
            data: null,
            isCanceled: true,
            status: 0,
          });
        }

        if (error.config?.responseType === 'blob') {
          if (error.response?.status === 200) {
            return Promise.resolve({
              data: null,
              status: 200,
            });
          }
          return Promise.reject({
            ...error,
            __skipToast: true,
          });
        }

        if (error.response?.status === 404 && 
            error.config?.url?.includes('/User/profile-photo')) {
          return Promise.resolve({
            data: null,
            isNotFound: true,
            status: 404,
          });
        }

        const handledError = handleApiError(error, {
          showToast: false, // UI handled by page
        });

        return Promise.reject({
          ...error,
          handled: handledError.handled,
          userMessage: handledError.message,
          status: handledError.status,
          apiData: handledError.data,
        });
      }
    );
  }

  handleError(error) {
    if (error.handled) {
      return error.userMessage;
    }
    return error.response?.data?.message || error.message || 'An unexpected error occurred';
  }

  cancelPendingRequest(key) {
    if (this.pendingRequests.has(key)) {
      this.pendingRequests.get(key).abort();
      this.pendingRequests.delete(key);
    }
  }

  cancelAllPendingRequests() {
    this.pendingRequests.forEach((controller, key) => {
      controller.abort();
      this.pendingRequests.delete(key);
    });
  }

  get(url, config = {}) {
    const controller = new AbortController();
    const requestKey = `GET:${url}:${JSON.stringify(config.params || {})}`;
    
    this.cancelPendingRequest(requestKey);
    this.pendingRequests.set(requestKey, controller);

    return this.instance.get(url, { 
      ...config, 
      signal: controller.signal 
    }).finally(() => {
      if (this.pendingRequests.get(requestKey) === controller) {
        this.pendingRequests.delete(requestKey);
      }
    });
  }

  post(url, data = {}, config = {}) {
    const controller = new AbortController();
    const requestKey = `POST:${url}`;
    
    this.pendingRequests.set(requestKey, controller);

    return this.instance.post(url, data, { 
      ...config, 
      signal: controller.signal 
    }).finally(() => {
      if (this.pendingRequests.get(requestKey) === controller) {
        this.pendingRequests.delete(requestKey);
      }
    });
  }

  put(url, data = {}, config = {}) {
    const controller = new AbortController();
    const requestKey = `PUT:${url}`;
    
    this.pendingRequests.set(requestKey, controller);

    return this.instance.put(url, data, { 
      ...config, 
      signal: controller.signal 
    }).finally(() => {
      if (this.pendingRequests.get(requestKey) === controller) {
        this.pendingRequests.delete(requestKey);
      }
    });
  }

  delete(url, config = {}) {
    const controller = new AbortController();
    const requestKey = `DELETE:${url}`;
    
    this.pendingRequests.set(requestKey, controller);

    return this.instance.delete(url, { 
      ...config, 
      signal: controller.signal 
    }).finally(() => {
      if (this.pendingRequests.get(requestKey) === controller) {
        this.pendingRequests.delete(requestKey);
      }
    });
  }

  patch(url, data = {}, config = {}) {
    const controller = new AbortController();
    const requestKey = `PATCH:${url}`;
    
    this.pendingRequests.set(requestKey, controller);

    return this.instance.patch(url, data, { 
      ...config, 
      signal: controller.signal 
    }).finally(() => {
      if (this.pendingRequests.get(requestKey) === controller) {
        this.pendingRequests.delete(requestKey);
      }
    });
  }
}

export default new ApiService();