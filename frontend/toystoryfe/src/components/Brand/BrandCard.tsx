import React from 'react';
import { motion } from 'framer-motion';
import { Building, Eye, MapPin, Pencil, Trash2 } from 'lucide-react';
import type { Brand } from '../../types/types';

type Props = {
  brand: Brand;
  index: number;
  canManage: boolean;
  onView: (brand: Brand) => void;
  onEdit: (brand: Brand) => void;
  onDelete: (brand: Brand) => void;
};

export const BrandCard: React.FC<Props> = ({ brand, index, canManage, onView, onEdit, onDelete }) => {
  return (
    <motion.div
      key={brand.id}
      layout
      initial={{ opacity: 0, y: 10 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ delay: index * 0.05 }}
      className="premium-card"
      style={{ padding: '32px', display: 'flex', flexDirection: 'column', gap: '20px', position: 'relative' }}
    >
      {canManage && (
        <button
          type="button"
          onClick={() => onDelete(brand)}
          aria-label={`Delete ${brand.name}`}
          style={{
            position: 'absolute',
            top: '16px',
            right: '16px',
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

      <div style={{ display: 'flex', alignItems: 'center', gap: '20px' }}>
        <div style={{ width: '64px', height: '64px', background: 'var(--accent-bg)', borderRadius: '16px', display: 'flex', justifyContent: 'center', alignItems: 'center', color: 'var(--primary)' }}>
          {brand.imageUrl ? (
            <img src={brand.imageUrl} alt={brand.name} style={{ width: '100%', borderRadius: '16px' }} />
          ) : (
            <Building size={32} />
          )}
        </div>
        <div style={{ flex: 1 }}>
          <h3 style={{ margin: 0 }}>{brand.name}</h3>
          <div style={{ display: 'flex', alignItems: 'center', gap: '4px', fontSize: '13px', color: 'var(--text-dim)', marginTop: '4px' }}>
            <MapPin size={12} /> {brand.country || 'Global'}
          </div>
        </div>
      </div>

      <p style={{ color: 'var(--text-dim)', fontSize: '14px', flex: 1, overflow: 'hidden', display: '-webkit-box', WebkitLineClamp: 3, WebkitBoxOrient: 'vertical' }}>
        {brand.description}
      </p>

      <div style={{ display: 'flex', gap: '12px' }}>
        <button
          type="button"
          className="premium-card"
          onClick={() => onView(brand)}
          style={{ flex: 1, padding: '10px', display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '8px', background: 'var(--background)' }}
        >
          <Eye size={18} /> View
        </button>
        {canManage && (
          <button
            type="button"
            className="premium-card"
            onClick={() => onEdit(brand)}
            style={{ padding: '10px', display: 'flex', alignItems: 'center', justifyContent: 'center', background: 'var(--primary)', color: 'white', border: 'none' }}
          >
            <Pencil size={18} />
          </button>
        )}
      </div>
    </motion.div>
  );
};

