import React, { useEffect, useState } from 'react';
import axios from 'axios';
import api from '../api/axios';
import { motion, AnimatePresence } from 'framer-motion';
import { Tag, Building, Search, Plus, MapPin, Filter, Eye, Pencil, X, Upload, Trash2, History } from 'lucide-react';
import { useAuth } from '../context/AuthContext';

interface Brand {
  id: string;
  name: string;
  description: string;
  country?: string;
  website?: string;
  imageUrl?: string;
}

type ModalMode = 'view' | 'create' | 'update';

interface BrandFormState {
  name: string;
  description: string;
  image: File | null;
}

interface BrandEventItem {
  id: number;
  brandId: number;
  eventType: string;
  payload: string;
  createdAt: string;
}

const emptyFormState: BrandFormState = {
  name: '',
  description: '',
  image: null,
};

const BrandListPage: React.FC = () => {
  const [brands, setBrands] = useState<Brand[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [searchTerm, setSearchTerm] = useState('');
  const [sortDesc, setSortDesc] = useState(false);
  const [modalMode, setModalMode] = useState<ModalMode | null>(null);
  const [selectedBrand, setSelectedBrand] = useState<Brand | null>(null);
  const [formState, setFormState] = useState<BrandFormState>(emptyFormState);
  const [formError, setFormError] = useState('');
  const [formSuccess, setFormSuccess] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [imagePreviewUrl, setImagePreviewUrl] = useState('');
  const [historyOpen, setHistoryOpen] = useState(false);
  const [historyLoading, setHistoryLoading] = useState(false);
  const [historyError, setHistoryError] = useState('');
  const [historyItems, setHistoryItems] = useState<BrandEventItem[]>([]);
  const { isAuthenticated, user } = useAuth();

  const fetchBrands = async () => {
    try {
      setLoading(true);
      const response = await api.get('/queries/brands', {
        params: { sortDesc }
      });
      const brandsData = response.data.data || [];
      setBrands(Array.isArray(brandsData) ? brandsData : []);
    } catch (err: any) {
      setError('Failed to load brands. Service might be temporarily unavailable.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchBrands();
  }, [sortDesc]);

  const filteredBrands = brands.filter(b =>
    (b.name?.toLowerCase() || '').includes(searchTerm.toLowerCase())
  );

  useEffect(() => {
    if (formState.image) {
      const objectUrl = URL.createObjectURL(formState.image);
      setImagePreviewUrl(objectUrl);

      return () => URL.revokeObjectURL(objectUrl);
    }

    setImagePreviewUrl(selectedBrand?.imageUrl || '');
    return undefined;
  }, [formState.image, selectedBrand]);

  const openModal = (mode: ModalMode, brand: Brand | null = null) => {
    setFormError('');
    setFormSuccess('');
    setHistoryOpen(false);
    setHistoryError('');
    setHistoryItems([]);
    setModalMode(mode);
    setSelectedBrand(brand);

    if (brand) {
      setFormState({
        name: brand.name || '',
        description: brand.description || '',
        image: null,
      });
      return;
    }

    setFormState(emptyFormState);
  };

  const closeModal = () => {
    setModalMode(null);
    setSelectedBrand(null);
    setFormState(emptyFormState);
    setFormError('');
    setFormSuccess('');
    setSubmitting(false);
    setHistoryOpen(false);
    setHistoryLoading(false);
    setHistoryError('');
    setHistoryItems([]);
  };

  const getApiErrorMessage = (err: unknown, fallback: string) => {
    if (!axios.isAxiosError(err)) {
      return fallback;
    }

    const status = err.response?.status;
    const data = err.response?.data as any;
    const rawMessage = typeof data?.message === 'string' ? data.message : '';
    const title = typeof data?.title === 'string' ? data.title : '';
    const detail = typeof data?.detail === 'string' ? data.detail : '';

    const message = [detail, title]
      .find((item) => item && item.trim().length > 0 && item !== 'Success')
      || (rawMessage && rawMessage !== 'Success' ? rawMessage : '');

    if (message) {
      return message;
    }

    if (status) {
      return `${status} ${fallback}`;
    }

    return fallback;
  };

  const buildFormData = () => {
    const payload = new FormData();
    payload.append('Name', formState.name.trim());
    payload.append('Description', formState.description.trim());

    if (formState.image) {
      payload.append('imageFile', formState.image);
    }

    return payload;
  };

  const handleSubmitBrand = async (event: React.FormEvent) => {
    event.preventDefault();

    if (!modalMode || modalMode === 'view') {
      return;
    }

    if (!formState.name.trim() || !formState.description.trim()) {
      setFormError('Please fill in all required fields before saving.');
      return;
    }

    try {
      setSubmitting(true);
      setFormError('');

      const payload = buildFormData();
      const requestConfig = { headers: { 'Content-Type': 'multipart/form-data' } };

      if (modalMode === 'create') {
        await api.post('/commands/brands', payload, requestConfig);
        setFormSuccess('Brand created successfully.');
      } else if (selectedBrand?.id) {
        await api.put(`/commands/brands/${selectedBrand.id}`, payload, requestConfig);
        setFormSuccess('Brand updated successfully.');
      }

      await new Promise((resolve) => setTimeout(resolve, 150));
      await fetchBrands();
      closeModal();
    } catch (err) {
      setFormError(getApiErrorMessage(err, modalMode === 'create' ? 'Failed to create brand.' : 'Failed to update brand.'));
    } finally {
      setSubmitting(false);
    }
  };

  const handleDeleteBrand = async (brand: Brand) => {
    if (!window.confirm(`Delete brand "${brand.name}"? This cannot be undone.`)) {
      return;
    }

    try {
      await api.delete(`/commands/brands/${brand.id}`);
      await fetchBrands();
    } catch (err) {
      setError(getApiErrorMessage(err, 'Failed to delete brand.'));
    }
  };

  const handleLoadBrandHistory = async () => {
    if (!selectedBrand?.id) {
      return;
    }

    try {
      setHistoryLoading(true);
      setHistoryError('');

      const response = await api.get(`/queries/brands/${selectedBrand.id}/events`);
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
    if (value === null || value === undefined || value === '') {
      return '—';
    }

    if (Array.isArray(value)) {
      return value.map((item) => formatEventValue(item)).join(', ');
    }

    if (typeof value === 'object') {
      return JSON.stringify(value);
    }

    return String(value);
  };

  const getBrandHistoryFields = (payload: string) => {
    const parsed = parseEventPayload(payload);

    if (!parsed) {
      return [];
    }

    const preferredKeys = ['Name', 'Country', 'Website', 'Description', 'UpdatedAt'];
    const keys = [
      ...preferredKeys.filter((key) => parsed[key] !== undefined),
      ...Object.keys(parsed).filter((key) => !preferredKeys.includes(key)).slice(0, 3),
    ];

    return keys.map((key) => ({ key, value: formatEventValue(parsed[key]) }));
  };

  return (
    <div>
      <header style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: '24px' }}>
        <div>
          <h1>Discover Brands</h1>
        </div>

        <div style={{ display: 'flex', gap: '16px', flex: 1, maxWidth: '720px', alignItems: 'center' }}>
          <div style={{ flex: 1, position: 'relative' }}>
            <div style={{ position: 'absolute', left: '16px', top: '50%', transform: 'translateY(-50%)', color: 'var(--text-dim)' }}>
              <Search size={18} />
            </div>
            <input
              type="text"
              className="form-input"
              style={{ paddingLeft: '48px' }}
              placeholder="Search brands..."
              value={searchTerm}
              onChange={e => setSearchTerm(e.target.value)}
            />
          </div>
          {isAuthenticated && user?.role === 1 && (
            <button
              type="button"
              className="shimmer-button"
              onClick={() => openModal('create')}
              style={{ display: 'flex', alignItems: 'center', gap: '8px', whiteSpace: 'nowrap' }}
            >
              <Plus size={18} /> Create
            </button>
          )}
          <button
            className="premium-card"
            onClick={() => setSortDesc(!sortDesc)}
            style={{ padding: '8px 16px', display: 'flex', alignItems: 'center', gap: '8px', background: 'var(--card-bg)' }}
          >
            <Filter size={18} /> <span>{sortDesc ? 'Descending' : 'Ascending'}</span>
          </button>
        </div>
      </header>

      {loading ? (
        <div className="dashboard-grid">
          {[1, 2, 3, 4].map(i => (
            <div key={i} className="premium-card" style={{ height: '300px', background: 'var(--border)', opacity: 0.3 }}></div>
          ))}
        </div>
      ) : error ? (
        <div style={{ textAlign: 'center', padding: '80px', color: 'var(--error)' }}>
          <Tag size={64} style={{ opacity: 0.2, marginBottom: '24px' }} />
          <h3>{error}</h3>
          <button onClick={() => window.location.reload()} className="shimmer-button" style={{ marginTop: '24px' }}>Try Again</button>
        </div>
      ) : (
        <div className="dashboard-grid" style={{ gridTemplateColumns: 'repeat(auto-fill, minmax(320px, 1fr))' }}>
          <AnimatePresence>
            {filteredBrands.map((brand, idx) => (
              <motion.div
                key={brand.id}
                layout
                initial={{ opacity: 0, y: 10 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: idx * 0.05 }}
                className="premium-card"
                style={{ padding: '32px', display: 'flex', flexDirection: 'column', gap: '20px', position: 'relative' }}
              >
                {isAuthenticated && user?.role === 1 && (
                  <button
                    type="button"
                    onClick={() => void handleDeleteBrand(brand)}
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
                    onClick={() => openModal('view', brand)}
                    style={{ flex: 1, padding: '10px', display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '8px', background: 'var(--background)' }}
                  >
                    <Eye size={18} /> View
                  </button>
                  <button
                    type="button"
                    className="premium-card"
                    onClick={() => openModal('update', brand)}
                    style={{ padding: '10px', display: 'flex', alignItems: 'center', justifyContent: 'center', background: 'var(--primary)', color: 'white', border: 'none' }}
                  >
                    <Pencil size={18} />
                  </button>
                </div>
              </motion.div>
            ))}
          </AnimatePresence>
        </div>
      )}

      <AnimatePresence>
        {modalMode && (
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            transition={{ duration: 0.2 }}
            onClick={closeModal}
            style={{
              position: 'fixed',
              inset: 0,
              background: 'rgba(15, 23, 42, 0.65)',
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
                onClick={closeModal}
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
                    {modalMode === 'create' ? 'Create Brand' : modalMode === 'update' ? 'Update Brand' : 'View Brand'}
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

                    <div className="premium-card" style={{ padding: '16px', background: 'var(--background)', gridColumn: '1 / -1' }}>
                      <div style={{ display: 'flex', justifyContent: 'space-between', gap: '12px', alignItems: 'center', flexWrap: 'wrap' }}>
                        <div>
                          <div style={{ color: 'var(--text-dim)', fontSize: '12px', marginBottom: '4px' }}>Audit trail</div>
                          <div style={{ fontWeight: 700 }}>Change History</div>
                        </div>

                        <button
                          type="button"
                          className="premium-card"
                          onClick={() => void handleLoadBrandHistory()}
                          style={{ display: 'inline-flex', alignItems: 'center', gap: '8px', padding: '10px 14px', background: 'var(--card-bg)' }}
                        >
                          <History size={16} /> {historyOpen ? 'Refresh history' : 'Load history'}
                        </button>
                      </div>

                      {historyLoading && (
                        <div style={{ marginTop: '14px', color: 'var(--text-dim)' }}>Loading history...</div>
                      )}

                      {historyError && (
                        <div style={{ marginTop: '14px', color: 'var(--error)' }}>{historyError}</div>
                      )}

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
                                  {getBrandHistoryFields(item.payload).length > 0 ? (
                                    <div style={{ display: 'grid', gap: '10px' }}>
                                      {getBrandHistoryFields(item.payload).map((field) => (
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
                <form onSubmit={handleSubmitBrand}>
                  <div style={{ display: 'grid', gridTemplateColumns: 'minmax(0, 1.7fr) minmax(220px, 0.75fr)', gap: modalMode === 'update' ? '14px' : '18px', alignItems: 'start', fontSize: '14px', lineHeight: 1.45 }}>
                    <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(240px, 1fr))', gap: modalMode === 'update' ? '12px' : '18px' }}>
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
                          style={{ minHeight: modalMode === 'update' ? '96px' : '120px', resize: 'vertical' }}
                          value={formState.description}
                          onChange={(event) => setFormState((current) => ({ ...current, description: event.target.value }))}
                          placeholder="Describe the brand"
                          disabled={submitting}
                          required
                        />
                      </div>
                    </div>

                    <div style={{ display: 'flex', flexDirection: 'column', gap: modalMode === 'update' ? '8px' : '10px' }}>
                      <label style={{ display: 'block', marginBottom: '5px', fontWeight: 600, fontSize: '13px' }}>Brand Image</label>
                      <div style={{ borderRadius: '20px', overflow: 'hidden', border: '1px solid var(--border)', background: 'var(--accent-bg)', minHeight: modalMode === 'update' ? '200px' : '220px', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                        {imagePreviewUrl ? (
                          <img
                            src={imagePreviewUrl}
                            alt={selectedBrand?.name || formState.name || 'Brand preview'}
                            style={{ width: '100%', height: '100%', objectFit: 'cover', minHeight: modalMode === 'update' ? '200px' : '220px' }}
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
                          padding: modalMode === 'update' ? '9px 13px' : '10px 14px',
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

                  <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '10px', marginTop: modalMode === 'update' ? '22px' : '28px', flexWrap: 'wrap' }}>
                    <button type="button" className="premium-card" onClick={closeModal} style={{ padding: '9px 15px', background: 'var(--background)', fontSize: '13px' }}>
                      Cancel
                    </button>
                    <button type="submit" className="shimmer-button" disabled={submitting} style={{ minWidth: '160px', display: 'inline-flex', alignItems: 'center', justifyContent: 'center', gap: '8px', fontSize: '13px' }}>
                      {submitting ? 'Saving...' : modalMode === 'create' ? 'Create Brand' : 'Update Brand'}
                    </button>
                  </div>
                </form>
              )}
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
};

export default BrandListPage;
