import React from 'react';
import { AnimatePresence } from 'framer-motion';
import { Package } from 'lucide-react';
import ProductCard from './ProductCard';
import type { Product } from '../../types/types';

interface ProductListProps {
  loading: boolean;
  error: string;
  filteredProducts: Product[];
  isAdmin: boolean;
  onDelete: (product: Product) => void;
  onView: (product: Product) => void;
  onEdit: (product: Product) => void;
}

const ProductList: React.FC<ProductListProps> = ({
  loading,
  error,
  filteredProducts,
  isAdmin,
  onDelete,
  onView,
  onEdit,
}) => {
  if (loading) {
    return (
      <div className="dashboard-grid">
        {[1, 2, 3, 4, 5, 6].map((i) => (
          <div key={i} className="premium-card" style={{ height: '400px', background: 'var(--border)', opacity: 0.3 }}></div>
        ))}
      </div>
    );
  }

  if (error) {
    return (
      <div style={{ textAlign: 'center', padding: '80px', color: 'var(--error)' }}>
        <Package size={64} style={{ opacity: 0.2, marginBottom: '24px' }} />
        <h3>{error}</h3>
        <button onClick={() => window.location.reload()} className="shimmer-button" style={{ marginTop: '24px' }}>
          Try Again
        </button>
      </div>
    );
  }

  return (
    <div className="dashboard-grid">
      <AnimatePresence>
        {filteredProducts.map((product, idx) => (
          <ProductCard
            key={product.id}
            product={product}
            idx={idx}
            isAdmin={isAdmin}
            onDelete={onDelete}
            onView={onView}
            onEdit={onEdit}
          />
        ))}
      </AnimatePresence>
      {filteredProducts.length === 0 && (
        <div style={{ textAlign: 'center', padding: '80px', color: 'var(--text-dim)', gridColumn: '1 / -1' }}>
          <Package size={64} style={{ opacity: 0.1, marginBottom: '24px' }} />
          <h3>No products found matching your search.</h3>
        </div>
      )}
    </div>
  );
};

export default ProductList;
