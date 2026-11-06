// src/core/services/api.service.js
import axios from 'axios';
import { API_CONFIG } from '../constants/appConstants';
import { STORAGE_KEYS } from '../constants/storageKeys';
import { handleApiError, shouldIgnoreError } from './apiErrorHandler';

// Endpoints that serve a file (photo / attachment). Used to detect the
// "no file exists" 404 case per backend change log §2.4 — resolve as null
// rather than reject, so callers can render a default avatar.
const isFileEndpointUrl = (url) => {
  if (typeof url !== 'string') return false;
  return /\/photo$/.test(url) || /\/download$/.test(url);
};

class ApiService {
  constructor() {
    this.pendingRequests = new Map();
    this.instance = axios.create({
      baseURL: API_CONFIG.BASE_URL,
      timeout: API_CONFIG.TIMEOUT,
      headers: {
        'Content-Type': 'application/json',
        Accept: 'application/json',
      },
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
          // Blob error — 404 on a file endpoint means "no file exists".
          if (
            error.response?.status === 404 &&
            isFileEndpointUrl(error.config?.url)
          ) {
            return Promise.resolve({
              data: null,
              isNotFound: true,
              status: 404,
            });
          }
          return Promise.reject({
            ...error,
            __skipToast: true,
          });
        }

        const handledError = handleApiError(error, {
          showToast: false,
        });

        return Promise.reject({
          ...error,
          handled: handledError.handled,
          userMessage: handledError.message,
          status: handledError.status,
          apiData: handledError.data,
          retryAfter: handledError.retryAfter ?? null,
        });
      }
    );
  }

  handleError(error) {
    if (error.handled) {
      return error.userMessage;
    }
    return (
      error.response?.data?.message ||
      error.message ||
      'An unexpected error occurred'
    );
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

  // ============================================================
  // FILE FETCHING (blob via authenticated axios call)
  // ============================================================
  //
  // Backend file URLs already include the "/api/..." prefix
  // (e.g. "/api/User/profile-photo", "/api/Employee/1/attachments/62/download").
  //
  // These URLs CANNOT be rendered via <img src>, <iframe src>, or <a href>
  // because the backend requires the X-Session-Token header — browsers do
  // not send custom headers on those requests, resulting in 401.
  //
  // Instead, callers use getFileBlob() which routes the fetch through the
  // authenticated axios instance, then render the resulting Blob via
  // URL.createObjectURL() — the browser serves the blob locally, no auth
  // needed.
  //
  // `baseURL: ''` override: axios normally prepends baseURL ("/api"), which
  // would produce "/api/api/..." because the path already contains "/api".
  // Overriding to '' uses the resolved path verbatim.

  _resolveFileUrl(backendPath) {
    if (!backendPath) return backendPath;
    const base = (API_CONFIG.BASE_URL || '').trim();

    // Same-origin (relative base or empty) — path is already a valid
    // same-origin URL. Dev: proxy forwards to the real backend.
    // Prod: same host serves both SPA and API.
    if (!base || base.startsWith('/')) {
      return backendPath;
    }

    // Cross-origin — prepend only the origin of the base URL. The path
    // already carries its own "/api/..." prefix.
    try {
      return `${new URL(base).origin}${backendPath}`;
    } catch {
      return backendPath;
    }
  }

  async getFileBlob(backendPath) {
    if (!backendPath) return null;
    const url = this._resolveFileUrl(backendPath);
    const response = await this.instance.get(url, {
      responseType: 'blob',
      baseURL: '', // url is fully resolved above
    });
    return response?.data ?? null;
  }

  // ============================================================
  // STANDARD HTTP VERBS
  // ============================================================
  get(url, config = {}) {
    const controller = new AbortController();
    const requestKey = `GET:${url}:${JSON.stringify(config.params || {})}`;

    this.cancelPendingRequest(requestKey);
    this.pendingRequests.set(requestKey, controller);

    return this.instance
      .get(url, { ...config, signal: controller.signal })
      .finally(() => {
        if (this.pendingRequests.get(requestKey) === controller) {
          this.pendingRequests.delete(requestKey);
        }
      });
  }

  post(url, data = {}, config = {}) {
    const controller = new AbortController();
    const requestKey = `POST:${url}`;

    this.pendingRequests.set(requestKey, controller);

    return this.instance
      .post(url, data, { ...config, signal: controller.signal })
      .finally(() => {
        if (this.pendingRequests.get(requestKey) === controller) {
          this.pendingRequests.delete(requestKey);
        }
      });
  }

  put(url, data = {}, config = {}) {
    const controller = new AbortController();
    const requestKey = `PUT:${url}`;

    this.pendingRequests.set(requestKey, controller);

    return this.instance
      .put(url, data, { ...config, signal: controller.signal })
      .finally(() => {
        if (this.pendingRequests.get(requestKey) === controller) {
          this.pendingRequests.delete(requestKey);
        }
      });
  }

  delete(url, config = {}) {
    const controller = new AbortController();
    const requestKey = `DELETE:${url}`;

    this.pendingRequests.set(requestKey, controller);

    return this.instance
      .delete(url, { ...config, signal: controller.signal })
      .finally(() => {
        if (this.pendingRequests.get(requestKey) === controller) {
          this.pendingRequests.delete(requestKey);
        }
      });
  }

  patch(url, data = {}, config = {}) {
    const controller = new AbortController();
    const requestKey = `PATCH:${url}`;

    this.pendingRequests.set(requestKey, controller);

    return this.instance
      .patch(url, data, { ...config, signal: controller.signal })
      .finally(() => {
        if (this.pendingRequests.get(requestKey) === controller) {
          this.pendingRequests.delete(requestKey);
        }
      });
  }
}

export default new ApiService();