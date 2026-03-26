export interface Product {
  id: string;
  name: string;
  price: number;
  description: string;
  origin?: string;
  material?: string;
  brandId?: number;
  brandName: string;
  categories?: Array<{
    categoryId: number;
    categoryName: string;
  }>;
  imageUrl?: string;
}

export interface BrandOption {
  id: number;
  name: string;
}

export interface CategoryOption {
  id: number;
  name: string;
}

export interface ProductFormState {
  name: string;
  price: string;
  description: string;
  origin: string;
  material: string;
  brandId: string;
  categoryIds: number[];
  image: File | null;
}

export interface ProductEventItem {
  id: number;
  productId: number;
  eventType: string;
  payload: string;
  createdAt: string;
}

export type CircuitState = 'Closed' | 'Isolated' | 'Open' | 'HalfOpen';


export interface CircuitBreakerOptions {
  failureThreshold: number;      // Number of failures before opening circuit
  successThreshold: number;      // Number of successes needed to close circuit
  fallbackValue?: any;           // Fallback value to return when circuit is open
  onStateChange?: (state: CircuitState) => void; // State change callback
}

export interface CircuitBreakerResult<T> {
  execute: (fn: () => Promise<T>) => Promise<T>;
  state: CircuitState;
  failureCount: number;
  reset: () => void;
  getState: () => CircuitState;
}

export interface CircuitBreakerHookOptions {
    serviceName: 'brand' | 'category' | 'product';
    autoSync?: boolean;
    syncInterval?: number;
    onStateChange?: (state: CircuitState) => void;
}