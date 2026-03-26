import axios from 'axios';

const defaultBaseUrl = 'http://localhost:7246/api';
const configuredBaseUrl = import.meta.env.VITE_API_BASE_URL || defaultBaseUrl;

const api = axios.create({
  baseURL: configuredBaseUrl,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add a request interceptor to add the auth token to headers
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

export default api;
