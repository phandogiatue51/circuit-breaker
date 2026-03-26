import React, { useState } from 'react';
import axios from 'axios';
import { Package, History } from 'lucide-react';
import api from '../../api/axios';
import type { Product, ProductEventItem } from '../../types/types';

interface ProductViewProps {
  product: Product | null;
}

const ProductView: React.FC<ProductViewProps> = ({ product }) => {
  const [historyOpen, setHistoryOpen] = useState(false);
  const [historyLoading, setHistoryLoading] = useState(false);
  const [historyError, setHistoryError] = useState('');
  const [historyItems, setHistoryItems] = useState<ProductEventItem[]>([]);

  const getApiErrorMessage = (err: unknown, fallback: string) => {
    if (!axios.isAxiosError(err)) return fallback;
    const status = err.response?.status;
    const data = err.response?.data as any;
    const validationErrors = data?.errors;
    const validationMessage = Array.isArray(validationErrors)
      ? validationErrors.flatMap((item: any) => Object.values(item)).filter(Boolean).join(', ')
      : '';
    const rawMessage = typeof data?.message === 'string' ? data.message : '';
    const title = typeof data?.title === 'string' ? data.title : '';
    const detail = typeof data?.detail === 'string' ? data.detail : '';
    const message = [detail, validationMessage, title]
      .find((item) => item && item.trim().length > 0 && item !== 'Success')
      || (rawMessage && rawMessage !== 'Success' ? rawMessage : '');
    if (message) return message;
    if (status) return `${status} ${fallback}`;
    return fallback;
  };

  const handleLoadProductHistory = async () => {
    if (!product?.id) return;
    try {
      setHistoryLoading(true);
      setHistoryError('');
      const response = await api.get(`/queries/products/${product.id}/events`);
      const eventsData = response.data.data || [];
      setHistoryItems(Array.isArray(eventsData) ? eventsData : []);
      setHistoryOpen(true);
    } catch (err) {
      setHistoryError(getApiErrorMessage(err, 'Failed to load change history.'));
    } finally {
      setHistoryLoading(false);
    }
  };

  const parseEventPayload = (payload: string) => {
    try {
      const parsed = JSON.parse(payload);
      return parsed && typeof parsed === 'object' ? parsed as Record<string, unknown> : null;
    } catch {
      return null;
    }
  };

  const formatEventValue = (value: unknown): string => {
    if (value === null || value === undefined || value === '') return '—';
    if (Array.isArray(value)) return value.map((item) => formatEventValue(item)).join(', ');
    if (typeof value === 'object') return JSON.stringify(value);
    return String(value);
  };

  const getProductHistoryFields = (payload: string) => {
    const parsed = parseEventPayload(payload);
    if (!parsed) return [];
    const preferredKeys = ['Name', 'Price', 'BrandId', 'Origin', 'Material', 'UpdatedAt'];
    const keys = [
      ...preferredKeys.filter((key) => parsed[key] !== undefined),
      ...Object.keys(parsed).filter((key) => !preferredKeys.includes(key)).slice(0, 3),
    ];
    return keys.map((key) => ({ key, value: formatEventValue(parsed[key]) }));
  };

  return (
    <div style={{ display: 'grid', gridTemplateColumns: 'minmax(0, 1.6fr) minmax(260px, 0.9fr)', gap: '24px', alignItems: 'start' }}>
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))', gap: '16px' }}>
        <div className="premium-card" style={{ padding: '16px', background: 'var(--background)' }}>
          <div style={{ color: 'var(--text-dim)', fontSize: '12px', marginBottom: '6px' }}>Name</div>
          <div style={{ fontWeight: 700 }}>{product?.name}</div>
        </div>
        <div className="premium-card" style={{ padding: '16px', background: 'var(--background)' }}>
          <div style={{ color: 'var(--text-dim)', fontSize: '12px', marginBottom: '6px' }}>Price</div>
          <div style={{ fontWeight: 700 }}>${product?.price}</div>
        </div>
        <div className="premium-card" style={{ padding: '16px', background: 'var(--background)' }}>
          <div style={{ color: 'var(--text-dim)', fontSize: '12px', marginBottom: '6px' }}>Brand</div>
          <div style={{ fontWeight: 700 }}>{product?.brandName}</div>
        </div>
        <div className="premium-card" style={{ padding: '16px', background: 'var(--background)' }}>
          <div style={{ color: 'var(--text-dim)', fontSize: '12px', marginBottom: '6px' }}>Category</div>
          <div style={{ fontWeight: 700 }}>
            {product?.categories?.length
              ? product.categories.map((category) => category.categoryName).join(', ')
              : 'No category selected'}
          </div>
        </div>
        <div className="premium-card" style={{ padding: '16px', background: 'var(--background)' }}>
          <div style={{ color: 'var(--text-dim)', fontSize: '12px', marginBottom: '6px' }}>Origin</div>
          <div style={{ fontWeight: 700 }}>{product?.origin || 'Not provided'}</div>
        </div>
        <div className="premium-card" style={{ padding: '16px', background: 'var(--background)' }}>
          <div style={{ color: 'var(--text-dim)', fontSize: '12px', marginBottom: '6px' }}>Material</div>
          <div style={{ fontWeight: 700 }}>{product?.material || 'Not provided'}</div>
        </div>
        <div className="premium-card" style={{ padding: '16px', background: 'var(--background)', gridColumn: '1 / -1' }}>
          <div style={{ color: 'var(--text-dim)', fontSize: '12px', marginBottom: '6px' }}>Description</div>
          <div style={{ lineHeight: 1.6 }}>{product?.description}</div>
        </div>

        <div className="premium-card" style={{ padding: '16px', background: 'var(--background)', gridColumn: '1 / -1' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', gap: '12px', alignItems: 'center', flexWrap: 'wrap' }}>
            <div>
              <div style={{ color: 'var(--text-dim)', fontSize: '12px', marginBottom: '4px' }}>Audit trail</div>
              <div style={{ fontWeight: 700 }}>Change History</div>
            </div>
            <button
              type="button"
              className="premium-card"
              onClick={() => void handleLoadProductHistory()}
              style={{ display: 'inline-flex', alignItems: 'center', gap: '8px', padding: '10px 14px', background: 'var(--card-bg)' }}
            >
              <History size={16} /> {historyOpen ? 'Refresh history' : 'Load history'}
            </button>
          </div>

          {historyLoading && <div style={{ marginTop: '14px', color: 'var(--text-dim)' }}>Loading history...</div>}
          {historyError && <div style={{ marginTop: '14px', color: 'var(--error)' }}>{historyError}</div>}

          {historyOpen && !historyLoading && !historyError && (
            <div style={{ marginTop: '14px', display: 'grid', gap: '12px', maxHeight: '280px', overflowY: 'auto', paddingRight: '4px' }}>
              {historyItems.length > 0 ? historyItems.map((item, index) => (
                <div key={item.id} style={{ display: 'grid', gridTemplateColumns: '18px 1fr', gap: '12px', alignItems: 'start' }}>
                  <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', paddingTop: '8px' }}>
                    <div style={{ width: '10px', height: '10px', borderRadius: '999px', background: 'var(--primary)' }} />
                    {index < historyItems.length - 1 && (
                      <div style={{ width: '2px', flex: 1, minHeight: '44px', background: 'rgba(148, 163, 184, 0.25)', marginTop: '4px' }} />
                    )}
                  </div>

                  <div className="premium-card" style={{ padding: '14px', background: 'var(--background)', border: '1px solid var(--border)' }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', gap: '12px', flexWrap: 'wrap', marginBottom: '10px' }}>
                      <div style={{ display: 'flex', alignItems: 'center', gap: '8px', flexWrap: 'wrap' }}>
                        <span style={{ padding: '4px 10px', borderRadius: '999px', background: 'rgba(59, 130, 246, 0.12)', color: 'var(--primary)', fontSize: '12px', fontWeight: 700 }}>
                          {item.eventType}
                        </span>
                        <span style={{ color: 'var(--text-dim)', fontSize: '12px' }}>Event #{index + 1}</span>
                      </div>
                      <div style={{ color: 'var(--text-dim)', fontSize: '12px' }}>{new Date(item.createdAt).toLocaleString()}</div>
                    </div>

                    <div style={{ borderRadius: '14px', background: 'rgba(15, 23, 42, 0.04)', border: '1px solid rgba(148, 163, 184, 0.16)', padding: '12px' }}>
                      {getProductHistoryFields(item.payload).length > 0 ? (
                        <div style={{ display: 'grid', gap: '10px' }}>
                          {getProductHistoryFields(item.payload).map((field) => (
                            <div key={field.key} style={{ display: 'flex', justifyContent: 'space-between', gap: '12px', alignItems: 'flex-start', flexWrap: 'wrap' }}>
                              <div style={{ color: 'var(--text-dim)', fontSize: '12px', minWidth: '96px' }}>{field.key}</div>
                              <div style={{ fontWeight: 600, fontSize: '13px', textAlign: 'right', wordBreak: 'break-word' }}>{field.value}</div>
                            </div>
                          ))}
                        </div>
                      ) : (
                        <div style={{ color: 'var(--text-dim)', fontSize: '13px' }}>{item.payload}</div>
                      )}
                    </div>
                  </div>
                </div>
              )) : (
                <div style={{ color: 'var(--text-dim)', fontSize: '14px' }}>No history available.</div>
              )}
            </div>
          )}
        </div>
      </div>

      <div style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
        <div style={{ borderRadius: '20px', overflow: 'hidden', border: '1px solid var(--border)', background: 'var(--accent-bg)', minHeight: '280px', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
          {product?.imageUrl ? (
            <img
              src={product.imageUrl}
              alt={product?.name || 'Product preview'}
              style={{ width: '100%', height: '100%', objectFit: 'cover', minHeight: '280px' }}
            />
          ) : (
            <div style={{ textAlign: 'center', color: 'var(--text-dim)', padding: '20px' }}>
              <Package size={36} style={{ marginBottom: '12px', opacity: 0.5 }} />
              <div>No image selected</div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default ProductView;
