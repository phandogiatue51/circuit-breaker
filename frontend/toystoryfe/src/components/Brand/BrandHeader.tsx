import React from 'react';
import { Filter, Plus, Search } from 'lucide-react';

type Props = {
  searchTerm: string;
  onSearchTermChange: (value: string) => void;
  sortDesc: boolean;
  onToggleSort: () => void;
  canCreate: boolean;
  onCreate: () => void;
};

export const BrandHeader: React.FC<Props> = ({
  searchTerm,
  onSearchTermChange,
  sortDesc,
  onToggleSort,
  canCreate,
  onCreate,
}) => {
  return (
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
            onChange={(e) => onSearchTermChange(e.target.value)}
          />
        </div>

        {canCreate && (
          <button
            type="button"
            className="shimmer-button"
            onClick={onCreate}
            style={{ display: 'flex', alignItems: 'center', gap: '8px', whiteSpace: 'nowrap' }}
          >
            <Plus size={18} /> Create
          </button>
        )}

        <button
          className="premium-card"
          onClick={onToggleSort}
          style={{ padding: '8px 16px', display: 'flex', alignItems: 'center', gap: '8px', background: 'var(--card-bg)' }}
        >
          <Filter size={18} /> <span>{sortDesc ? 'Descending' : 'Ascending'}</span>
        </button>
      </div>
    </header>
  );
};

