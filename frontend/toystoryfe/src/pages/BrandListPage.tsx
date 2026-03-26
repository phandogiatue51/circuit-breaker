import React, { useCallback, useEffect, useState } from 'react';
import api from '../api/axios';
import { AnimatePresence } from 'framer-motion';
import { Tag } from 'lucide-react';
import { useAuth } from '../context/AuthContext';
import { getApiErrorMessage } from '../utils/errorUtils';
import { BrandCard } from '../components/Brand/BrandCard';
import { BrandHeader } from '../components/Brand/BrandHeader';
import { BrandModal } from '../components/Brand/BrandModal';
import type { Brand, BrandEventItem, BrandFormState, ModalMode } from '../types/types';

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

  const fetchBrands = useCallback(async () => {
    try {
      setLoading(true);
      const response = await api.get('/queries/brands', {
        params: { sortDesc }
      });
      const brandsData = response.data.data || [];
      setBrands(Array.isArray(brandsData) ? brandsData : []);
    } catch {
      setError('Brand Service might be temporarily unavailable.');
    } finally {
      setLoading(false);
    }
  }, [sortDesc]);

  useEffect(() => {
    void fetchBrands();
  }, [fetchBrands]);

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
      <BrandHeader
        searchTerm={searchTerm}
        onSearchTermChange={setSearchTerm}
        sortDesc={sortDesc}
        onToggleSort={() => setSortDesc(!sortDesc)}
        canCreate={isAuthenticated && user?.role === 1}
        onCreate={() => openModal('create')}
      />

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
              <BrandCard
                key={brand.id}
                brand={brand}
                index={idx}
                canManage={isAuthenticated && user?.role === 1}
                onView={(b) => openModal('view', b)}
                onEdit={(b) => openModal('update', b)}
                onDelete={(b) => void handleDeleteBrand(b)}
              />
            ))}
          </AnimatePresence>
        </div>
      )}
      <BrandModal
        mode={modalMode}
        selectedBrand={selectedBrand}
        formState={formState}
        setFormState={setFormState}
        submitting={submitting}
        formError={formError}
        formSuccess={formSuccess}
        imagePreviewUrl={imagePreviewUrl}
        onClose={closeModal}
        onSubmit={handleSubmitBrand}
        historyOpen={historyOpen}
        historyLoading={historyLoading}
        historyError={historyError}
        historyItems={historyItems}
        onLoadHistory={() => void handleLoadBrandHistory()}
        getHistoryFields={getBrandHistoryFields}
      />
    </div>
  );
};

export default BrandListPage;
