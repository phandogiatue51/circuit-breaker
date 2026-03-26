import React from 'react';
import { motion } from 'framer-motion';
import { Package, Eye, Pencil, ShoppingCart, Star, Trash2 } from 'lucide-react';
import type { Product } from '../../types/types';

interface ProductCardProps {
  product: Product;
  idx: number;
  isAdmin: boolean | null;
  onDelete: (product: Product) => void;
  onView: (product: Product) => void;
  onEdit: (product: Product) => void;
}

const ProductCard: React.FC<ProductCardProps> = ({
  product,
  idx,
  isAdmin,
  onDelete,
  onView,
  onEdit,
}) => {
  return (
    <motion.div
      layout
      initial={{ opacity: 0, scale: 0.9 }}
      animate={{ opacity: 1, scale: 1 }}
      transition={{ delay: idx * 0.05 }}
      className="premium-card"
      style={{ overflow: 'hidden', display: 'flex', flexDirection: 'column' }}
    >
      <div style={{ height: '240px', background: 'var(--accent-bg)', position: 'relative', display: 'flex', justifyContent: 'center', alignItems: 'center' }}>
        {isAdmin && (
          <button
            type="button"
            onClick={(event) => {
              event.stopPropagation();
              onDelete(product);
            }}
            aria-label={`Delete ${product.name}`}
            style={{
              position: 'absolute',
              top: '16px',
              left: '16px',
              zIndex: 2,
              width: '34px',
              height: '34px',
              borderRadius: '999px',
              border: 'none',
              background: 'rgba(239, 68, 68, 0.92)',
              color: 'white',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              cursor: 'pointer',
              boxShadow: '0 8px 24px rgba(239, 68, 68, 0.35)',
            }}
          >
            <Trash2 size={16} />
          </button>
        )}
        {product.imageUrl ? (
          <img src={product.imageUrl} alt={product.name} style={{ width: '100%', height: '100%', objectFit: 'cover' }} />
        ) : (
          <Package size={64} style={{ color: 'var(--primary)', opacity: 0.5 }} />
        )}
        <div style={{ position: 'absolute', top: '16px', right: '16px', background: 'var(--glass)', backdropFilter: 'blur(8px)', padding: '4px 12px', borderRadius: '100px', fontWeight: 600, fontSize: '14px' }}>
          {product.brandName}
        </div>
      </div>

      <div style={{ padding: '24px', flex: 1, display: 'flex', flexDirection: 'column' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '8px' }}>
          <h3 style={{ margin: 0, fontSize: '20px' }}>{product.name}</h3>
          <div style={{ color: 'var(--primary)', fontWeight: 800, fontSize: '18px' }}>
            ${product.price}
          </div>
        </div>
        <p style={{ color: 'var(--text-dim)', fontSize: '14px', marginBottom: '20px', flex: 1 }}>{product.description}</p>

        <div style={{ display: 'flex', gap: '12px' }}>
          {isAdmin ? (
            <>
              <button
                type="button"
                className="premium-card"
                onClick={() => onView(product)}
                style={{ flex: 1, padding: '12px', display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '8px', background: 'var(--background)' }}
              >
                <Eye size={18} /> View
              </button>
              <button
                type="button"
                className="shimmer-button"
                onClick={() => onEdit(product)}
                style={{ flex: 1, display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '8px' }}
              >
                <Pencil size={18} /> Edit
              </button>
            </>
          ) : (
            <>
              <button className="shimmer-button" style={{ flex: 1, display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '8px' }}>
                <ShoppingCart size={18} /> Buy Now
              </button>
              <button className="premium-card" style={{ padding: '12px', display: 'flex', alignItems: 'center', justifyContent: 'center', background: 'var(--background)' }}>
                <Star size={18} />
              </button>
            </>
          )}
        </div>
      </div>
    </motion.div>
  );
};

export default ProductCard;
