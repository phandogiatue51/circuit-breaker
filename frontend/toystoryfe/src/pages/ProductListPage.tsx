import React, { useEffect, useState } from 'react';
import api from '../api/axios';
import { getApiErrorMessage } from '../utils/errorUtils';
import { useAuth } from '../context/AuthContext';
import type { BrandOption, CategoryOption, Product, ProductFormState } from '../types/types';
import ProductHeader from '../components/Product/ProductHeader';
import ProductModal from '../components/Product/ProductModal';
import ProductList from '../components/Product/ProductList';
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
      <ProductHeader
        searchTerm={searchTerm}
        onSearchChange={setSearchTerm}
        isAdmin={isAdmin}
        onCreateClick={() => openModal('create')}
        sortDesc={sortDesc}
        onSortChange={() => setSortDesc(!sortDesc)}
      />

      <ProductList
        loading={loading}
        error={error}
        filteredProducts={filteredProducts}
        isAdmin={isAdmin}
        onDelete={(p) => void handleDeleteProduct(p)}
        onView={(p) => openModal('view', p)}
        onEdit={(p) => openModal('update', p)}
      />

      <ProductModal
        modalMode={modalMode}
        onClose={closeModal}
        selectedProduct={selectedProduct}
        formState={formState}
        setFormState={setFormState}
        submitting={submitting}
        lookupLoading={lookupLoading}
        brandOptions={brandOptions}
        categoryOptions={categoryOptions}
        formError={formError}
        formSuccess={formSuccess}
        onSubmit={handleSubmitProduct}
      />
    </div>
  );
};

export default ProductListPage;
