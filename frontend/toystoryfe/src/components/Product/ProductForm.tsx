import React, { useState, useEffect } from 'react';
import { Package, Upload } from 'lucide-react';
import type { BrandOption, CategoryOption, Product, ProductFormState } from '../../types/types';

interface ProductFormProps {
  modalMode: 'create' | 'update';
  formState: ProductFormState;
  setFormState: React.Dispatch<React.SetStateAction<ProductFormState>>;
  submitting: boolean;
  lookupLoading: boolean;
  brandOptions: BrandOption[];
  categoryOptions: CategoryOption[];
  selectedProduct: Product | null;
  onSubmit: (event: React.FormEvent) => void;
  onCancel: () => void;
}

const ProductForm: React.FC<ProductFormProps> = ({
  modalMode,
  formState,
  setFormState,
  submitting,
  lookupLoading,
  brandOptions,
  categoryOptions,
  selectedProduct,
  onSubmit,
  onCancel,
}) => {
  const [categoryPickerOpen, setCategoryPickerOpen] = useState(false);
  const [imagePreviewUrl, setImagePreviewUrl] = useState('');
  const compactEditMode = modalMode === 'update';

  useEffect(() => {
    if (formState.image) {
      const objectUrl = URL.createObjectURL(formState.image);
      setImagePreviewUrl(objectUrl);
      return () => URL.revokeObjectURL(objectUrl);
    }
    setImagePreviewUrl(selectedProduct?.imageUrl || '');
    return undefined;
  }, [formState.image, selectedProduct]);

  const selectedBrand = brandOptions.find((item) => item.id.toString() === formState.brandId);
  const selectedCategories = categoryOptions.filter((item) => formState.categoryIds.includes(item.id));

  return (
    <form onSubmit={onSubmit}>
      <div style={{ display: 'grid', gridTemplateColumns: 'minmax(0, 1.6fr) minmax(260px, 0.9fr)', gap: compactEditMode ? '16px' : '20px', alignItems: 'start', fontSize: '14px', lineHeight: 1.45 }}>
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(240px, 1fr))', gap: compactEditMode ? '12px' : '18px' }}>
          <div>
            <label style={{ display: 'block', marginBottom: '5px', fontWeight: 600, fontSize: '13px' }}>Product Name</label>
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
            <label style={{ display: 'block', marginBottom: '5px', fontWeight: 600, fontSize: '13px' }}>Price</label>
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
            <label style={{ display: 'block', marginBottom: '5px', fontWeight: 600, fontSize: '13px' }}>Brand</label>
            <select
              className="form-input"
              value={formState.brandId}
              onChange={(event) => setFormState((current) => ({ ...current, brandId: event.target.value }))}
              disabled={submitting || lookupLoading}
              required
            >
              <option value="">{lookupLoading ? 'Loading brands...' : 'Select a brand'}</option>
              {brandOptions.map((brand) => (
                <option key={brand.id} value={brand.id}>
                  {brand.name}
                </option>
              ))}
            </select>
            {selectedBrand && (
              <div style={{ marginTop: '6px', fontSize: '12px', color: 'var(--text-dim)' }}>
                Selected brand: {selectedBrand.name}
              </div>
            )}
          </div>

          <div>
            <label style={{ display: 'block', marginBottom: '5px', fontWeight: 600, fontSize: '13px' }}>Origin</label>
            <input
              type="text"
              className="form-input"
              value={formState.origin}
              onChange={(event) => setFormState((current) => ({ ...current, origin: event.target.value }))}
              placeholder="Enter origin"
              disabled={submitting}
            />
          </div>

          <div>
            <label style={{ display: 'block', marginBottom: '5px', fontWeight: 600, fontSize: '13px' }}>Material</label>
            <input
              type="text"
              className="form-input"
              value={formState.material}
              onChange={(event) => setFormState((current) => ({ ...current, material: event.target.value }))}
              placeholder="Enter material"
              disabled={submitting}
            />
          </div>

          <div style={{ gridColumn: '1 / -1', position: 'relative' }}>
            <label style={{ display: 'block', marginBottom: '5px', fontWeight: 600, fontSize: '13px' }}>Categories</label>
            <button
              type="button"
              className="form-input"
              onClick={() => setCategoryPickerOpen((current) => !current)}
              disabled={submitting || lookupLoading}
              style={{ textAlign: 'left', display: 'flex', justifyContent: 'space-between', alignItems: 'center', cursor: 'pointer' }}
            >
              <span>
                {lookupLoading
                  ? 'Loading categories...'
                  : selectedCategories.length > 0
                    ? `${selectedCategories.length} category(s) selected`
                    : 'Click to select one or more categories'}
              </span>
              <span style={{ color: 'var(--text-dim)' }}>{categoryPickerOpen ? '▲' : '▼'}</span>
            </button>

            {categoryPickerOpen && !lookupLoading && (
              <div className="premium-card" style={{ marginTop: '10px', padding: compactEditMode ? '14px' : '16px', background: 'var(--background)' }}>
                <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 1fr))', gap: compactEditMode ? '10px' : '12px' }}>
                  {categoryOptions.map((category) => (
                    <label key={category.id} style={{ display: 'flex', alignItems: 'center', gap: '10px', cursor: 'pointer' }}>
                      <input
                        type="checkbox"
                        checked={formState.categoryIds.includes(category.id)}
                        onChange={(event) => {
                          const isChecked = event.target.checked;
                          setFormState((current) => ({
                            ...current,
                            categoryIds: isChecked
                              ? [...current.categoryIds, category.id]
                              : current.categoryIds.filter((item) => item !== category.id),
                          }));
                        }}
                        disabled={submitting}
                      />
                      <span>{category.name}</span>
                    </label>
                  ))}
                </div>
                {categoryOptions.length === 0 && (
                  <div style={{ color: 'var(--text-dim)', fontSize: '14px' }}>No categories available.</div>
                )}
              </div>
            )}

            {selectedCategories.length > 0 && (
              <div style={{ display: 'flex', flexWrap: 'wrap', gap: '8px', marginTop: '8px' }}>
                {selectedCategories.map((category) => (
                  <span key={category.id} style={{ padding: '6px 10px', borderRadius: '999px', background: 'var(--accent-bg)', color: 'var(--text-main)', fontSize: '13px' }}>
                    {category.name}
                  </span>
                ))}
              </div>
            )}
          </div>

          <div style={{ gridColumn: '1 / -1' }}>
            <label style={{ display: 'block', marginBottom: '5px', fontWeight: 600, fontSize: '13px' }}>Description</label>
            <textarea
              className="form-input"
              style={{ minHeight: compactEditMode ? '96px' : '120px', resize: 'vertical' }}
              value={formState.description}
              onChange={(event) => setFormState((current) => ({ ...current, description: event.target.value }))}
              placeholder="Describe the product"
              disabled={submitting}
              required
            />
          </div>
        </div>

        <div style={{ display: 'flex', flexDirection: 'column', gap: compactEditMode ? '10px' : '12px' }}>
          <label style={{ display: 'block', marginBottom: '5px', fontWeight: 600, fontSize: '13px' }}>Product Image</label>
          <div style={{ borderRadius: '20px', overflow: 'hidden', border: '1px solid var(--border)', background: 'var(--accent-bg)', minHeight: compactEditMode ? '240px' : '280px', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
            {imagePreviewUrl ? (
              <img
                src={imagePreviewUrl}
                alt={selectedProduct?.name || formState.name || 'Product preview'}
                style={{ width: '100%', height: '100%', objectFit: 'cover', minHeight: compactEditMode ? '240px' : '280px' }}
              />
            ) : (
              <div style={{ textAlign: 'center', color: 'var(--text-dim)', padding: '18px' }}>
                <Package size={36} style={{ marginBottom: '12px', opacity: 0.5 }} />
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
              padding: compactEditMode ? '9px 13px' : '10px 14px',
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
            {formState.image ? formState.image.name : selectedProduct?.imageUrl ? 'Current image will remain unless you upload a new one.' : 'JPEG, PNG, JPG, or WEBP up to 5MB.'}
          </div>
        </div>
      </div>

      <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '10px', marginTop: compactEditMode ? '22px' : '28px', flexWrap: 'wrap' }}>
        <button type="button" className="premium-card" onClick={onCancel} style={{ padding: '9px 15px', background: 'var(--background)', fontSize: '13px' }}>
          Cancel
        </button>
        <button type="submit" className="shimmer-button" disabled={submitting} style={{ minWidth: '160px', display: 'inline-flex', alignItems: 'center', justifyContent: 'center', gap: '8px', fontSize: '13px' }}>
          {submitting ? 'Saving...' : modalMode === 'create' ? 'Create Product' : 'Update Product'}
        </button>
      </div>
    </form>
  );
};

export default ProductForm;
