import { useState, useCallback, useEffect } from 'react';
import api from '../api/axios';
import type { CircuitBreakerHookOptions, CircuitState } from '../types/types';

interface ServiceStatus {
  state: 'Closed' | 'Isolated' | 'Open' | 'HalfOpen';
  isManuallyControlled: boolean;
  manualControlHash: number;
}

interface AllServicesStatus {
  brandService: ServiceStatus;
  categoryService: ServiceStatus;
  productService: ServiceStatus;
}

export function useCircuitBreaker(options: CircuitBreakerHookOptions) {
  const {
    serviceName,
    autoSync = true,
    syncInterval = 30000,
    onStateChange
  } = options;

  const [state, setState] = useState<CircuitState>('Closed');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [allStatus, setAllStatus] = useState<AllServicesStatus | null>(null);

  // Update local state and notify
  const updateState = useCallback((newState: CircuitState) => {
    setState(newState);
    onStateChange?.(newState);
  }, [onStateChange]);

  // Get current circuit breaker status for all services
  const fetchStatus = useCallback(async () => {
    try {
      const response = await api.get('/../internal/CircuitBreaker/status');
      const data = response.data as AllServicesStatus;
      setAllStatus(data);
      
      // Update this service's state
      const serviceKey = `${serviceName}Service` as keyof AllServicesStatus;
      const serviceStatus = data[serviceKey];
      if (serviceStatus) {
        updateState(serviceStatus.state);
      }
      
      return data;
    } catch (err) {
      console.error('Failed to fetch circuit breaker status:', err);
      setError('Failed to fetch status');
      return null;
    }
  }, [serviceName, updateState]);

  // Isolate the circuit breaker (manually open)
  const isolate = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await api.post(`/../internal/CircuitBreaker/isolate/${serviceName}`);
      // Update state based on response
      if (response.data.finalState === 'Isolated') {
        updateState('Isolated');
      }
      // Fetch fresh status to confirm
      await fetchStatus();
      return response.data;
    } catch (err: any) {
      setError(err.response?.data?.message || `Failed to isolate ${serviceName} service`);
      throw err;
    } finally {
      setLoading(false);
    }
  }, [serviceName, updateState, fetchStatus]);

  // Close the circuit breaker (manually close)
  const close = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await api.post(`/../internal/CircuitBreaker/close/${serviceName}`);
      // Update state based on response
      if (response.data.finalState === 'Closed') {
        updateState('Closed');
      }
      // Fetch fresh status to confirm
      await fetchStatus();
      return response.data;
    } catch (err: any) {
      setError(err.response?.data?.message || `Failed to close ${serviceName} circuit`);
      throw err;
    } finally {
      setLoading(false);
    }
  }, [serviceName, updateState, fetchStatus]);

  // Ping the service to check if it's healthy
  const ping = useCallback(async () => {
    try {
      const response = await api.get('/../internal/CircuitBreaker/ping');
      return response.data;
    } catch (err) {
      return false;
    }
  }, []);

  // Get debug info
  const debug = useCallback(async () => {
    try {
      const response = await api.get('/../internal/CircuitBreaker/debug');
      return response.data;
    } catch (err) {
      return null;
    }
  }, []);

  // Reset circuit breaker (alias for close)
  const reset = useCallback(async () => {
    return close();
  }, [close]);

  // Auto-sync status from backend
  useEffect(() => {
    if (autoSync) {
      fetchStatus();
      const interval = setInterval(fetchStatus, syncInterval);
      return () => clearInterval(interval);
    }
  }, [autoSync, fetchStatus, syncInterval]);

  return {
    state,
    loading,
    error,
    allStatus,
    isolate,
    close,
    reset,
    ping,
    debug,
    fetchStatus
  };
}