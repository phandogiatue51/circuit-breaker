import React, { useEffect, useState } from 'react';
import axios from 'axios';
import api from '../api/axios';
import { motion, AnimatePresence } from 'framer-motion';
import { Package, Search, Filter, Plus, X } from 'lucide-react';
import { useAuth } from '../context/AuthContext';
import type { BrandOption, CategoryOption, Product, ProductFormState } from '../types/types';
import ProductCard from '../components/Product/ProductCard';
import ProductView from '../components/Product/ProductView';
import ProductForm from '../components/Product/ProductForm';
type ModalMode = 'view' | 'create' | 'update';

const emptyFormState: ProductFormState = {
  name: '',
  price: '',
  description: '',
  origin: '',
  material: '',
  brandId: '',
  categoryIds: [],
  image: null,
};

const ProductListPage: React.FC = () => {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [searchTerm, setSearchTerm] = useState('');
  const [sortDesc, setSortDesc] = useState(false);
  const [brandOptions, setBrandOptions] = useState<BrandOption[]>([]);
  const [categoryOptions, setCategoryOptions] = useState<CategoryOption[]>([]);
  const [lookupLoading, setLookupLoading] = useState(false);
  const [modalMode, setModalMode] = useState<ModalMode | null>(null);
  const [selectedProduct, setSelectedProduct] = useState<Product | null>(null);
  const [formState, setFormState] = useState<ProductFormState>(emptyFormState);
  const [formError, setFormError] = useState('');
  const [formSuccess, setFormSuccess] = useState('');
  const [submitting, setSubmitting] = useState(false);
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
      setError('Product Service might be temporarily unavailable.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchProducts();
  }, [sortDesc]);

  useEffect(() => {
    const fetchLookups = async () => {
      try {
        setLookupLoading(true);

        const [brandsResponse, categoriesResponse] = await Promise.all([
          api.get('/queries/brands'),
          api.get('/queries/categories'),
        ]);

        const brandsData = brandsResponse.data.data || [];
        const categoriesData = categoriesResponse.data.data || [];

        setBrandOptions(Array.isArray(brandsData)
          ? brandsData.map((item: any) => ({ id: Number(item.id), name: item.name ?? '' })).filter((item: BrandOption) => item.id && item.name)
          : []);
        setCategoryOptions(Array.isArray(categoriesData)
          ? categoriesData.map((item: any) => ({ id: Number(item.id), name: item.name ?? '' })).filter((item: CategoryOption) => item.id && item.name)
          : []);
      } catch {
        setBrandOptions([]);
        setCategoryOptions([]);
      } finally {
        setLookupLoading(false);
      }
    };

    fetchLookups();
  }, []);



  useEffect(() => {
    if (!selectedProduct || !modalMode || modalMode === 'create') {
      return;
    }

    setFormState((current) => ({
      ...current,
      origin: selectedProduct.origin ?? current.origin,
      material: selectedProduct.material ?? current.material,
      brandId: selectedProduct.brandId?.toString() || current.brandId,
      categoryIds: current.categoryIds.length > 0
        ? current.categoryIds
        : selectedProduct.categories?.map((category) => category.categoryId) ?? [],
    }));
  }, [modalMode, selectedProduct]);

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
        origin: product.origin || '',
        material: product.material || '',
        brandId: product.brandId?.toString() || '',
        categoryIds: product.categories?.map((category) => category.categoryId) || [],
        image: null
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
    payload.append('Origin', formState.origin.trim());
    payload.append('Material', formState.material.trim());

    if (formState.brandId.trim()) {
      payload.append('BrandId', formState.brandId.trim());
    }

    formState.categoryIds.forEach((categoryId) => {
      payload.append('CategoryIds', categoryId.toString());
    });

    if (formState.image) {
      payload.append('imageFile', formState.image);
    }

    return payload;
  };

  const handleSubmitProduct = async (event: React.FormEvent) => {
    event.preventDefault();

    if (!modalMode || modalMode === 'view') {
      return;
    }

    if (!formState.name.trim() || !formState.description.trim() || !formState.price.trim() || !formState.brandId.trim()) {
      setFormError('Please fill in all required fields before saving.');
      return;
    }

    try {
      setSubmitting(true);
      setFormError('');

      const payload = buildFormData();
      const requestConfig = { headers: { 'Content-Type': 'multipart/form-data' } };

      if (modalMode === 'create') {
        await api.post('/commands/products', payload, requestConfig);
        setFormSuccess('Product created successfully.');
      } else if (selectedProduct?.id) {
        await api.put(`/commands/products/${selectedProduct.id}`, payload, requestConfig);
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

  const handleDeleteProduct = async (product: Product) => {
    if (!window.confirm(`Delete product "${product.name}"? This cannot be undone.`)) {
      return;
    }

    try {
      await api.delete(`/commands/products/${product.id}`);
      await fetchProducts();
    } catch (err) {
      setError(getApiErrorMessage(err, 'Failed to delete product.'));
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
              <ProductCard
                key={product.id}
                product={product}
                idx={idx}
                isAdmin={isAdmin}
                onDelete={(p) => void handleDeleteProduct(p)}
                onView={(p) => openModal('view', p)}
                onEdit={(p) => openModal('update', p)}
              />
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
                  onSubmit={handleSubmitProduct}
                  onCancel={closeModal}
                />
              )}
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
};

export default ProductListPage;
