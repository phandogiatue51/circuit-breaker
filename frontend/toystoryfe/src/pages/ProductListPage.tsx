import React, { useEffect, useState } from 'react';
import axios from 'axios';
import api from '../api/axios';
import { motion, AnimatePresence } from 'framer-motion';
import { Package, Search, Filter, ShoppingCart, Star, Plus, Eye, Pencil, X, Upload } from 'lucide-react';
import { useAuth } from '../context/AuthContext';

interface Product {
  id: string;
  name: string;
  price: number;
  description: string;
  categoryName: string;
  brandName: string;
  imageUrl?: string;
}

type ModalMode = 'view' | 'create' | 'update';

interface ProductFormState {
  name: string;
  price: string;
  description: string;
  brandName: string;
  categoryName: string;
  image: File | null;
}

const emptyFormState: ProductFormState = {
  name: '',
  price: '',
  description: '',
  brandName: '',
  categoryName: '',
  image: null,
};

const ProductListPage: React.FC = () => {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [searchTerm, setSearchTerm] = useState('');
  const [sortDesc, setSortDesc] = useState(false);
  const [modalMode, setModalMode] = useState<ModalMode | null>(null);
  const [selectedProduct, setSelectedProduct] = useState<Product | null>(null);
  const [formState, setFormState] = useState<ProductFormState>(emptyFormState);
  const [formError, setFormError] = useState('');
  const [formSuccess, setFormSuccess] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [imagePreviewUrl, setImagePreviewUrl] = useState('');
  const { isAuthenticated, user } = useAuth();

  const isAdmin = isAuthenticated && user?.role === 1;

  const fetchProducts = async () => {
    try {
      setLoading(true);
      const response = await api.get('/queries/products', {
        params: {
          page: 1,
          pageSize: 20,
          sortBy: 'Id',
          sortDesc: sortDesc,
        },
      });
      const productsData = response.data.data || [];
      setProducts(Array.isArray(productsData) ? productsData : []);
    } catch {
      setError('Failed to load products. Service might be temporarily unavailable.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchProducts();
  }, [sortDesc]);

  useEffect(() => {
    if (formState.image) {
      const objectUrl = URL.createObjectURL(formState.image);
      setImagePreviewUrl(objectUrl);

      return () => URL.revokeObjectURL(objectUrl);
    }

    setImagePreviewUrl(selectedProduct?.imageUrl || '');
    return undefined;
  }, [formState.image, selectedProduct]);

  const openModal = (mode: ModalMode, product: Product | null = null) => {
    setFormError('');
    setFormSuccess('');
    setModalMode(mode);
    setSelectedProduct(product);

    if (product) {
      setFormState({
        name: product.name || '',
        price: product.price?.toString() || '',
        description: product.description || '',
        brandName: product.brandName || '',
        categoryName: product.categoryName || '',
        image: null,
      });
      return;
    }

    setFormState(emptyFormState);
  };

  const closeModal = () => {
    setModalMode(null);
    setSelectedProduct(null);
    setFormState(emptyFormState);
    setFormError('');
    setFormSuccess('');
    setSubmitting(false);
  };

  const getApiErrorMessage = (err: unknown, fallback: string) => {
    if (!axios.isAxiosError(err)) {
      return fallback;
    }

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
    payload.append('Price', formState.price.trim());
    payload.append('BrandName', formState.brandName.trim());
    payload.append('CategoryName', formState.categoryName.trim());

    if (formState.image) {
      payload.append('Image', formState.image);
    }

    return payload;
  };

  const handleSubmitProduct = async (event: React.FormEvent) => {
    event.preventDefault();

    if (!modalMode || modalMode === 'view') {
      return;
    }

    if (!formState.name.trim() || !formState.description.trim() || !formState.price.trim() || !formState.brandName.trim() || !formState.categoryName.trim()) {
      setFormError('Please fill in all required fields before saving.');
      return;
    }

    if (modalMode === 'create' && !formState.image) {
      setFormError('Please choose an image for the new product.');
      return;
    }

    try {
      setSubmitting(true);
      setFormError('');

      const payload = buildFormData();
      const requestConfig = { headers: { 'Content-Type': 'multipart/form-data' } };

      if (modalMode === 'create') {
        await api.post('/comment/products', payload, requestConfig);
        setFormSuccess('Product created successfully.');
      } else if (selectedProduct?.id) {
        await api.put(`/comment/products/${selectedProduct.id}`, payload, requestConfig);
        setFormSuccess('Product updated successfully.');
      }

      await fetchProducts();
      closeModal();
    } catch (err) {
      setFormError(getApiErrorMessage(err, modalMode === 'create' ? 'Failed to create product.' : 'Failed to update product.'));
    } finally {
      setSubmitting(false);
    }
  };

  const filteredProducts = products.filter(p =>
    (p.name?.toLowerCase() || '').includes(searchTerm.toLowerCase()) ||
    (p.brandName?.toLowerCase() || '').includes(searchTerm.toLowerCase())
  );

  return (
    <div >
      <header style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: '24px' }}>
        <div>
          <h1>Discover Products</h1>
        </div>

        <div style={{ display: 'flex', gap: '16px', flex: 1, maxWidth: '720px', alignItems: 'center' }}>
          <div style={{ position: 'relative', flex: 1 }}>
            <div style={{ position: 'absolute', left: '16px', top: '50%', transform: 'translateY(-50%)', color: 'var(--text-dim)' }}>
              <Search size={18} />
            </div>
            <input
              type="text"
              className="form-input"
              style={{ paddingLeft: '48px' }}
              placeholder="Search products or brands..."
              value={searchTerm}
              onChange={e => setSearchTerm(e.target.value)}
            />
          </div>
          {isAdmin && (
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
            type="button"
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
          {[1, 2, 3, 4, 5, 6].map(i => (
            <div key={i} className="premium-card" style={{ height: '400px', background: 'var(--border)', opacity: 0.3 }}></div>
          ))}
        </div>
      ) : error ? (
        <div style={{ textAlign: 'center', padding: '80px', color: 'var(--error)' }}>
          <Package size={64} style={{ opacity: 0.2, marginBottom: '24px' }} />
          <h3>{error}</h3>
          <button onClick={() => window.location.reload()} className="shimmer-button" style={{ marginTop: '24px' }}>Try Again</button>
        </div>
      ) : (
        <div className="dashboard-grid">
          <AnimatePresence>
            {filteredProducts.map((product, idx) => (
              <motion.div
                key={product.id}
                layout
                initial={{ opacity: 0, scale: 0.9 }}
                animate={{ opacity: 1, scale: 1 }}
                transition={{ delay: idx * 0.05 }}
                className="premium-card"
                style={{ overflow: 'hidden', display: 'flex', flexDirection: 'column' }}
              >
                <div style={{ height: '240px', background: 'var(--accent-bg)', position: 'relative', display: 'flex', justifyContent: 'center', alignItems: 'center' }}>
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
                          onClick={() => openModal('view', product)}
                          style={{ flex: 1, padding: '12px', display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '8px', background: 'var(--background)' }}
                        >
                          <Eye size={18} /> View
                        </button>
                        <button
                          type="button"
                          className="shimmer-button"
                          onClick={() => openModal('update', product)}
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

              <div style={{ display: 'flex', justifyContent: 'space-between', gap: '24px', flexWrap: 'wrap', marginBottom: '24px' }}>
                <div>
                  <h2 style={{ marginBottom: '8px' }}>
                    {modalMode === 'create' ? 'Create Product' : modalMode === 'update' ? 'Update Product' : 'View Product'}
                  </h2>
                  <p style={{ color: 'var(--text-dim)' }}>
                    {modalMode === 'create'
                      ? 'Add a new product to the catalog.'
                      : modalMode === 'update'
                        ? 'Edit the selected product and keep the catalog current.'
                        : 'Inspect product details in one place.'}
                  </p>
                </div>

                <div style={{ minWidth: '220px', maxWidth: '320px', flex: 1 }}>
                  <div style={{ borderRadius: '20px', overflow: 'hidden', border: '1px solid var(--border)', background: 'var(--accent-bg)', minHeight: '180px', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                    {imagePreviewUrl ? (
                      <img
                        src={imagePreviewUrl}
                        alt={selectedProduct?.name || formState.name || 'Product preview'}
                        style={{ width: '100%', height: '100%', objectFit: 'cover', minHeight: '180px' }}
                      />
                    ) : (
                      <div style={{ textAlign: 'center', color: 'var(--text-dim)', padding: '24px' }}>
                        <Package size={36} style={{ marginBottom: '12px', opacity: 0.5 }} />
                        <div>No image selected</div>
                      </div>
                    )}
                  </div>
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
                <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))', gap: '16px' }}>
                  <div className="premium-card" style={{ padding: '18px', background: 'var(--background)' }}>
                    <div style={{ color: 'var(--text-dim)', fontSize: '13px', marginBottom: '8px' }}>Name</div>
                    <div style={{ fontWeight: 700 }}>{selectedProduct?.name}</div>
                  </div>
                  <div className="premium-card" style={{ padding: '18px', background: 'var(--background)' }}>
                    <div style={{ color: 'var(--text-dim)', fontSize: '13px', marginBottom: '8px' }}>Price</div>
                    <div style={{ fontWeight: 700 }}>${selectedProduct?.price}</div>
                  </div>
                  <div className="premium-card" style={{ padding: '18px', background: 'var(--background)' }}>
                    <div style={{ color: 'var(--text-dim)', fontSize: '13px', marginBottom: '8px' }}>Brand</div>
                    <div style={{ fontWeight: 700 }}>{selectedProduct?.brandName}</div>
                  </div>
                  <div className="premium-card" style={{ padding: '18px', background: 'var(--background)' }}>
                    <div style={{ color: 'var(--text-dim)', fontSize: '13px', marginBottom: '8px' }}>Category</div>
                    <div style={{ fontWeight: 700 }}>{selectedProduct?.categoryName}</div>
                  </div>
                  <div className="premium-card" style={{ padding: '18px', background: 'var(--background)', gridColumn: '1 / -1' }}>
                    <div style={{ color: 'var(--text-dim)', fontSize: '13px', marginBottom: '8px' }}>Description</div>
                    <div style={{ lineHeight: 1.6 }}>{selectedProduct?.description}</div>
                  </div>
                </div>
              ) : (
                <form onSubmit={handleSubmitProduct}>
                  <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(240px, 1fr))', gap: '18px' }}>
                    <div>
                      <label style={{ display: 'block', marginBottom: '8px', fontWeight: 600 }}>Product Name</label>
                      <input
                        type="text"
                        className="form-input"
                        value={formState.name}
                        onChange={(event) => setFormState((current) => ({ ...current, name: event.target.value }))}
                        placeholder="Enter product name"
                        disabled={submitting}
                        required
                      />
                    </div>

                    <div>
                      <label style={{ display: 'block', marginBottom: '8px', fontWeight: 600 }}>Price</label>
                      <input
                        type="number"
                        className="form-input"
                        min="0"
                        step="0.01"
                        value={formState.price}
                        onChange={(event) => setFormState((current) => ({ ...current, price: event.target.value }))}
                        placeholder="0.00"
                        disabled={submitting}
                        required
                      />
                    </div>

                    <div>
                      <label style={{ display: 'block', marginBottom: '8px', fontWeight: 600 }}>Brand Name</label>
                      <input
                        type="text"
                        className="form-input"
                        value={formState.brandName}
                        onChange={(event) => setFormState((current) => ({ ...current, brandName: event.target.value }))}
                        placeholder="Enter brand name"
                        disabled={submitting}
                        required
                      />
                    </div>

                    <div>
                      <label style={{ display: 'block', marginBottom: '8px', fontWeight: 600 }}>Category Name</label>
                      <input
                        type="text"
                        className="form-input"
                        value={formState.categoryName}
                        onChange={(event) => setFormState((current) => ({ ...current, categoryName: event.target.value }))}
                        placeholder="Enter category name"
                        disabled={submitting}
                        required
                      />
                    </div>

                    <div style={{ gridColumn: '1 / -1' }}>
                      <label style={{ display: 'block', marginBottom: '8px', fontWeight: 600 }}>Description</label>
                      <textarea
                        className="form-input"
                        style={{ minHeight: '120px', resize: 'vertical' }}
                        value={formState.description}
                        onChange={(event) => setFormState((current) => ({ ...current, description: event.target.value }))}
                        placeholder="Describe the product"
                        disabled={submitting}
                        required
                      />
                    </div>

                    <div style={{ gridColumn: '1 / -1' }}>
                      <label style={{ display: 'block', marginBottom: '8px', fontWeight: 600 }}>Product Image</label>
                      <div style={{ display: 'flex', gap: '16px', alignItems: 'center', flexWrap: 'wrap' }}>
                        <label
                          className="premium-card"
                          style={{
                            display: 'inline-flex',
                            alignItems: 'center',
                            gap: '8px',
                            padding: '12px 16px',
                            cursor: submitting ? 'not-allowed' : 'pointer',
                            background: 'var(--background)',
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
                            required={modalMode === 'create'}
                          />
                        </label>

                        <div style={{ color: 'var(--text-dim)', fontSize: '14px' }}>
                          {formState.image ? formState.image.name : selectedProduct?.imageUrl ? 'Current image will remain unless you upload a new one.' : 'JPEG, PNG, JPG, or WEBP up to 5MB.'}
                        </div>
                      </div>
                    </div>
                  </div>

                  <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '12px', marginTop: '28px', flexWrap: 'wrap' }}>
                    <button type="button" className="premium-card" onClick={closeModal} style={{ padding: '12px 20px', background: 'var(--background)' }}>
                      Cancel
                    </button>
                    <button type="submit" className="shimmer-button" disabled={submitting} style={{ minWidth: '160px', display: 'inline-flex', alignItems: 'center', justifyContent: 'center', gap: '8px' }}>
                      {submitting ? 'Saving...' : modalMode === 'create' ? 'Create Product' : 'Update Product'}
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

export default ProductListPage;
