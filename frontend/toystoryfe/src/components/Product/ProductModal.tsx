import React from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { X } from 'lucide-react';
import ProductView from './ProductView';
import ProductForm from './ProductForm';
import type { BrandOption, CategoryOption, Product, ProductFormState } from '../../types/types';

interface ProductModalProps {
  modalMode: 'view' | 'create' | 'update' | null;
  onClose: () => void;
  selectedProduct: Product | null;
  formState: ProductFormState;
  setFormState: React.Dispatch<React.SetStateAction<ProductFormState>>;
  submitting: boolean;
  lookupLoading: boolean;
  brandOptions: BrandOption[];
  categoryOptions: CategoryOption[];
  formError: string;
  formSuccess: string;
  onSubmit: (event: React.FormEvent) => void;
}

const ProductModal: React.FC<ProductModalProps> = ({
  modalMode,
  onClose,
  selectedProduct,
  formState,
  setFormState,
  submitting,
  lookupLoading,
  brandOptions,
  categoryOptions,
  formError,
  formSuccess,
  onSubmit,
}) => {
  return (
    <AnimatePresence>
      {modalMode && (
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          exit={{ opacity: 0 }}
          transition={{ duration: 0.2 }}
          onClick={onClose}
          style={{
            position: 'fixed',
            inset: 0,
            background: 'rgba(255, 255, 255, 0.6)',
            backdropFilter: 'blur(8px)',
            zIndex: 200,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            padding: '24px',
          }}
        >
          <motion.div
            initial={{ opacity: 0, y: 24, scale: 0.98 }}
            animate={{ opacity: 1, y: 0, scale: 1 }}
            exit={{ opacity: 0, y: 24, scale: 0.98 }}
            transition={{ type: 'spring', stiffness: 280, damping: 24 }}
            className="premium-card"
            onClick={(event) => event.stopPropagation()}
            style={{
              width: '100%',
              maxWidth: '860px',
              maxHeight: '90vh',
              overflowY: 'auto',
              padding: '32px',
              position: 'relative',
            }}
          >
            <button
              type="button"
              onClick={onClose}
              aria-label="Close modal"
              style={{
                position: 'absolute',
                top: '20px',
                right: '20px',
                border: 'none',
                background: 'transparent',
                color: 'var(--text-dim)',
                cursor: 'pointer',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
              }}
            >
              <X size={22} />
            </button>

            <div style={{ display: 'flex', justifyContent: 'space-between', gap: '24px', flexWrap: 'wrap', marginBottom: '24px', fontSize: '14px', lineHeight: 1.45 }}>
              <div>
                <h2 style={{ marginBottom: '8px', fontSize: '22px' }}>
                  {modalMode === 'create' ? 'Create Product' : modalMode === 'update' ? 'Update Product' : 'View Product'}
                </h2>
              </div>
            </div>

            {formError && (
              <div style={{ marginBottom: '20px', borderRadius: '14px', border: '1px solid rgba(239, 68, 68, 0.35)', background: 'rgba(239, 68, 68, 0.08)', color: 'var(--error)', padding: '12px 16px' }}>
                {formError}
              </div>
            )}

            {formSuccess && (
              <div style={{ marginBottom: '20px', borderRadius: '14px', border: '1px solid rgba(16, 185, 129, 0.35)', background: 'rgba(16, 185, 129, 0.08)', color: 'var(--success)', padding: '12px 16px' }}>
                {formSuccess}
              </div>
            )}

            {modalMode === 'view' ? (
              <ProductView product={selectedProduct} />
            ) : (
              <ProductForm
                modalMode={modalMode as 'create' | 'update'}
                formState={formState}
                setFormState={setFormState}
                submitting={submitting}
                lookupLoading={lookupLoading}
                brandOptions={brandOptions}
                categoryOptions={categoryOptions}
                selectedProduct={selectedProduct}
                onSubmit={onSubmit}
                onCancel={onClose}
              />
            )}
          </motion.div>
        </motion.div>
      )}
    </AnimatePresence>
  );
};

export default ProductModal;
