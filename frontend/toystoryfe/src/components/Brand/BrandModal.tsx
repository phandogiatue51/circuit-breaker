import React from 'react';
import { AnimatePresence, motion } from 'framer-motion';
import { Building, History, Upload, X } from 'lucide-react';
import type { Brand, BrandEventItem, BrandFormState, ModalMode } from '../../types/types';
import { BrandHistoryTimeline } from './BrandHistoryTimeline';

type Props = {
  mode: ModalMode | null;
  selectedBrand: Brand | null;
  formState: BrandFormState;
  setFormState: React.Dispatch<React.SetStateAction<BrandFormState>>;
  submitting: boolean;
  formError: string;
  formSuccess: string;
  imagePreviewUrl: string;
  onClose: () => void;
  onSubmit: (event: React.FormEvent) => void;
  historyOpen: boolean;
  historyLoading: boolean;
  historyError: string;
  historyItems: BrandEventItem[];
  onLoadHistory: () => void;
  getHistoryFields: (payload: string) => Array<{ key: string; value: string }>;
};

export const BrandModal: React.FC<Props> = ({
  mode,
  selectedBrand,
  formState,
  setFormState,
  submitting,
  formError,
  formSuccess,
  imagePreviewUrl,
  onClose,
  onSubmit,
  historyOpen,
  historyLoading,
  historyError,
  historyItems,
  onLoadHistory,
  getHistoryFields,
}) => {
  return (
    <AnimatePresence>
      {mode && (
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
                  {mode === 'create' ? 'Create Brand' : mode === 'update' ? 'Update Brand' : 'View Brand'}
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

            {mode === 'view' ? (
              <div style={{ display: 'grid', gridTemplateColumns: 'minmax(0, 1.7fr) minmax(220px, 0.75fr)', gap: '20px', alignItems: 'start' }}>
                <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))', gap: '16px' }}>
                  <div className="premium-card" style={{ padding: '16px', background: 'var(--background)' }}>
                    <div style={{ color: 'var(--text-dim)', fontSize: '12px', marginBottom: '6px' }}>Name</div>
                    <div style={{ fontWeight: 700 }}>{selectedBrand?.name}</div>
                  </div>
                  <div className="premium-card" style={{ padding: '16px', background: 'var(--background)', gridColumn: '1 / -1' }}>
                    <div style={{ color: 'var(--text-dim)', fontSize: '12px', marginBottom: '6px' }}>Description</div>
                    <div style={{ lineHeight: 1.6 }}>{selectedBrand?.description}</div>
                  </div>

                  <BrandHistoryTimeline
                    historyOpen={historyOpen}
                    historyLoading={historyLoading}
                    historyError={historyError}
                    historyItems={historyItems}
                    onLoadHistory={onLoadHistory}
                    getFields={getHistoryFields}
                  />

                  <div style={{ marginTop: '-8px', color: 'var(--text-dim)', fontSize: '12px', gridColumn: '1 / -1' }}>
                    <span style={{ display: 'inline-flex', alignItems: 'center', gap: '8px' }}>
                      <History size={14} /> History is loaded on demand per brand.
                    </span>
                  </div>
                </div>

                <div style={{ display: 'flex', flexDirection: 'column', gap: '10px' }}>
                  <div style={{ borderRadius: '20px', overflow: 'hidden', border: '1px solid var(--border)', background: 'var(--accent-bg)', minHeight: '220px', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                    {imagePreviewUrl ? (
                      <img
                        src={imagePreviewUrl}
                        alt={selectedBrand?.name || formState.name || 'Brand preview'}
                        style={{ width: '100%', height: '100%', objectFit: 'cover', minHeight: '220px' }}
                      />
                    ) : (
                      <div style={{ textAlign: 'center', color: 'var(--text-dim)', padding: '16px' }}>
                        <Building size={36} style={{ marginBottom: '12px', opacity: 0.5 }} />
                        <div>No image selected</div>
                      </div>
                    )}
                  </div>
                </div>
              </div>
            ) : (
              <form onSubmit={onSubmit}>
                <div style={{ display: 'grid', gridTemplateColumns: 'minmax(0, 1.7fr) minmax(220px, 0.75fr)', gap: mode === 'update' ? '14px' : '18px', alignItems: 'start', fontSize: '14px', lineHeight: 1.45 }}>
                  <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(240px, 1fr))', gap: mode === 'update' ? '12px' : '18px' }}>
                    <div>
                      <label style={{ display: 'block', marginBottom: '5px', fontWeight: 600, fontSize: '13px' }}>Brand Name</label>
                      <input
                        type="text"
                        className="form-input"
                        value={formState.name}
                        onChange={(event) => setFormState((current) => ({ ...current, name: event.target.value }))}
                        placeholder="Enter brand name"
                        disabled={submitting}
                        required
                      />
                    </div>

                    <div style={{ gridColumn: '1 / -1' }}>
                      <label style={{ display: 'block', marginBottom: '5px', fontWeight: 600, fontSize: '13px' }}>Description</label>
                      <textarea
                        className="form-input"
                        style={{ minHeight: mode === 'update' ? '96px' : '120px', resize: 'vertical' }}
                        value={formState.description}
                        onChange={(event) => setFormState((current) => ({ ...current, description: event.target.value }))}
                        placeholder="Describe the brand"
                        disabled={submitting}
                        required
                      />
                    </div>
                  </div>

                  <div style={{ display: 'flex', flexDirection: 'column', gap: mode === 'update' ? '8px' : '10px' }}>
                    <label style={{ display: 'block', marginBottom: '5px', fontWeight: 600, fontSize: '13px' }}>Brand Image</label>
                    <div style={{ borderRadius: '20px', overflow: 'hidden', border: '1px solid var(--border)', background: 'var(--accent-bg)', minHeight: mode === 'update' ? '200px' : '220px', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                      {imagePreviewUrl ? (
                        <img
                          src={imagePreviewUrl}
                          alt={selectedBrand?.name || formState.name || 'Brand preview'}
                          style={{ width: '100%', height: '100%', objectFit: 'cover', minHeight: mode === 'update' ? '200px' : '220px' }}
                        />
                      ) : (
                        <div style={{ textAlign: 'center', color: 'var(--text-dim)', padding: '14px' }}>
                          <Building size={36} style={{ marginBottom: '12px', opacity: 0.5 }} />
                          <div>No image selected</div>
                        </div>
                      )}
                    </div>

                    <label
                      className="premium-card"
                      style={{
                        display: 'inline-flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        gap: '8px',
                        padding: mode === 'update' ? '9px 13px' : '10px 14px',
                        cursor: submitting ? 'not-allowed' : 'pointer',
                        background: 'var(--background)',
                        fontSize: '13px',
                      }}
                    >
                      <Upload size={18} />
                      <span>{formState.image ? 'Change image' : 'Choose image'}</span>
                      <input
                        type="file"
                        accept="image/jpeg,image/png,image/jpg,image/webp"
                        onChange={(event) => setFormState((current) => ({ ...current, image: event.target.files?.[0] || null }))}
                        disabled={submitting}
                        style={{ display: 'none' }}
                      />
                    </label>

                    <div style={{ color: 'var(--text-dim)', fontSize: '12px' }}>
                      {formState.image ? formState.image.name : selectedBrand?.imageUrl ? 'Current image will remain unless you upload a new one.' : 'JPEG, PNG, JPG, or WEBP up to 5MB.'}
                    </div>
                  </div>
                </div>

                <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '10px', marginTop: mode === 'update' ? '22px' : '28px', flexWrap: 'wrap' }}>
                  <button type="button" className="premium-card" onClick={onClose} style={{ padding: '9px 15px', background: 'var(--background)', fontSize: '13px' }}>
                    Cancel
                  </button>
                  <button type="submit" className="shimmer-button" disabled={submitting} style={{ minWidth: '160px', display: 'inline-flex', alignItems: 'center', justifyContent: 'center', gap: '8px', fontSize: '13px' }}>
                    {submitting ? 'Saving...' : mode === 'create' ? 'Create Brand' : 'Update Brand'}
                  </button>
                </div>
              </form>
            )}
          </motion.div>
        </motion.div>
      )}
    </AnimatePresence>
  );
};

