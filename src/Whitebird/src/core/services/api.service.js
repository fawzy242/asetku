import axios from 'axios';
import Swal from 'sweetalert2';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api';
const SESSION_TOKEN_KEY = import.meta.env.VITE_SESSION_TOKEN_KEY || 'whitebird_session_token';

class ApiService {
  constructor() {
    this.instance = axios.create({
      baseURL: API_BASE_URL,
      timeout: 30000,
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
        const token = localStorage.getItem(SESSION_TOKEN_KEY);
        if (token) {
          config.headers['X-Session-Token'] = token;
        }
        return config;
      },
      (error) => {
        return Promise.reject(error);
      }
    );

    this.instance.interceptors.response.use(
      (response) => {
        return response;
      },
      (error) => {
        return this.handleError(error);
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
          Swal.fire({
            title: 'Session Expired',
            text: 'Please login again to continue.',
            icon: 'warning',
            confirmButtonColor: '#dc2626'
          });
          break;
          
        case 403:
          Swal.fire({
            title: 'Access Denied',
            text: 'You do not have permission to perform this action.',
            icon: 'error',
            confirmButtonColor: '#dc2626'
          });
          break;
          
        case 500:
          Swal.fire({
            title: 'Server Error',
            text: 'An unexpected error occurred. Please try again later.',
            icon: 'error',
            confirmButtonColor: '#dc2626'
          });
          break;
          
        default:
          if (data?.errors?.length > 0) {
            Swal.fire({
              title: data.message || 'Error',
              text: data.errors.join('\n'),
              icon: 'error',
              confirmButtonColor: '#dc2626'
            });
          }
      }
    } else if (error.request) {
      Swal.fire({
        title: 'Network Error',
        text: 'Unable to connect to the server. Please check your internet connection.',
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
    }
    
    return Promise.reject(error);
  }

  get(url, config = {}) {
    return this.instance.get(url, config);
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