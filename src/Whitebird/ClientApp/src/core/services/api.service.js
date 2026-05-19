import axios from 'axios';
import toast from 'react-hot-toast';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api';
const SESSION_TOKEN_KEY = import.meta.env.VITE_SESSION_TOKEN_KEY || 'whitebird_session_token';

class ApiService {
  constructor() {
    this.pendingRequests = new Map();
    this.instance = axios.create({
      baseURL: API_BASE_URL,
      timeout: 30000,
      headers: { 'Content-Type': 'application/json', 'Accept': 'application/json' }
    });
    this.setupInterceptors();
  }

  setupInterceptors() {
    this.instance.interceptors.request.use(
      (config) => {
        const token = localStorage.getItem(SESSION_TOKEN_KEY);
        if (token) config.headers['X-Session-Token'] = token;
        return config;
      },
      (error) => Promise.reject(error)
    );

    this.instance.interceptors.response.use(
      (response) => response,
      (error) => {
        if (axios.isCancel(error)) {
          return Promise.reject(error);
        }
        this.handleError(error);
        return Promise.reject(error);
      }
    );
  }

  handleError(error) {
    if (error.response) {
      const { status, data } = error.response;
      switch (status) {
        case 401:
          localStorage.removeItem(SESSION_TOKEN_KEY);
          localStorage.removeItem('user');
          if (window.location.pathname !== '/login') {
            window.location.href = '/login';
          }
          toast.error('Session expired. Please login again.');
          break;
        case 403:
          toast.error('Access denied.');
          break;
        case 500:
          toast.error('Server error. Please try again later.');
          break;
        default:
          if (data?.errors?.length > 0) {
            toast.error(data.errors.join('\n'));
          }
      }
    } else if (error.request) {
      toast.error('Network error. Please check your connection.');
    }
  }

  cancelPendingRequest(key) {
    if (this.pendingRequests.has(key)) {
      this.pendingRequests.get(key).abort();
      this.pendingRequests.delete(key);
    }
  }

  get(url, config = {}) {
    const controller = new AbortController();
    const requestKey = `GET:${url}`;
    this.cancelPendingRequest(requestKey);
    this.pendingRequests.set(requestKey, controller);

    return this.instance.get(url, { ...config, signal: controller.signal })
      .finally(() => {
        if (this.pendingRequests.get(requestKey) === controller) {
          this.pendingRequests.delete(requestKey);
        }
      });
  }

  post(url, data = {}, config = {}) {
    return this.instance.post(url, data, config);
  }

  put(url, data = {}, config = {}) {
    return this.instance.put(url, data, config);
  }

  delete(url, config = {}) {
    return this.instance.delete(url, config);
  }

  patch(url, data = {}, config = {}) {
    return this.instance.patch(url, data, config);
  }
}

export default new ApiService();