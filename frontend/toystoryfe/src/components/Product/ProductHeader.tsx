import React from 'react';
import { Search, Plus, Filter } from 'lucide-react';

interface ProductHeaderProps {
  searchTerm: string;
  onSearchChange: (value: string) => void;
  isAdmin: boolean;
  onCreateClick: () => void;
  sortDesc: boolean;
  onSortChange: () => void;
}

const ProductHeader: React.FC<ProductHeaderProps> = ({
  searchTerm,
  onSearchChange,
  isAdmin,
  onCreateClick,
  sortDesc,
  onSortChange,
}) => {
  return (
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
            onChange={(e) => onSearchChange(e.target.value)}
          />
        </div>
        {isAdmin && (
          <button
            type="button"
            className="shimmer-button"
            onClick={onCreateClick}
            style={{ display: 'flex', alignItems: 'center', gap: '8px', whiteSpace: 'nowrap' }}
          >
            <Plus size={18} /> Create
          </button>
        )}
        <button
          type="button"
          className="premium-card"
          onClick={onSortChange}
          style={{ padding: '8px 16px', display: 'flex', alignItems: 'center', gap: '8px', background: 'var(--card-bg)' }}
        >
          <Filter size={18} /> <span>{sortDesc ? 'Descending' : 'Ascending'}</span>
        </button>
      </div>
    </header>
  );
};

export default ProductHeader;
