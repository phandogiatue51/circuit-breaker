import React, { useEffect, useState } from 'react';
import api from '../api/axios';
import { motion, AnimatePresence } from 'framer-motion';
import { Tag, Building, Search, Plus, MapPin, ExternalLink, Globe, Filter } from 'lucide-react';
import { useAuth } from '../context/AuthContext';

interface Brand {
  id: string;
  name: string;
  description: string;
  country?: string;
  website?: string;
  logoUrl?: string;
}

const BrandListPage: React.FC = () => {
  const [brands, setBrands] = useState<Brand[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [searchTerm, setSearchTerm] = useState('');
  const [sortDesc, setSortDesc] = useState(false);
  const { isAuthenticated, user } = useAuth();

  useEffect(() => {
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

    fetchBrands();
  }, [sortDesc]);

  const filteredBrands = brands.filter(b => 
    (b.name?.toLowerCase() || '').includes(searchTerm.toLowerCase())
  );

  return (
    <div style={{ padding: '40px 24px' }}>
      <header style={{ marginBottom: '48px', display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: '24px' }}>
        <div>
          <h1 style={{ margin: 0 }}>Discover Brands</h1>
          <p style={{ color: 'var(--text-dim)' }}>Leading toy manufacturers from around the world</p>
        </div>

        <div style={{ display: 'flex', gap: '16px', flex: 1, maxWidth: '600px', alignItems: 'center' }}>
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
          {[1,2,3,4].map(i => (
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
                style={{ padding: '32px', display: 'flex', flexDirection: 'column', gap: '20px' }}
              >
                <div style={{ display: 'flex', alignItems: 'center', gap: '20px' }}>
                  <div style={{ width: '64px', height: '64px', background: 'var(--accent-bg)', borderRadius: '16px', display: 'flex', justifyContent: 'center', alignItems: 'center', color: 'var(--primary)' }}>
                    {brand.logoUrl ? (
                      <img src={brand.logoUrl} alt={brand.name} style={{ width: '100%', borderRadius: '16px' }} />
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
                  <button className="premium-card" style={{ flex: 1, padding: '10px', display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '8px', background: 'var(--background)' }}>
                    <Globe size={18} /> Visit Store
                  </button>
                  <button className="premium-card" style={{ padding: '10px', display: 'flex', alignItems: 'center', justifyContent: 'center', background: 'var(--primary)', color: 'white', border: 'none' }}>
                    <ExternalLink size={18} />
                  </button>
                </div>
              </motion.div>
            ))}
          </AnimatePresence>

          {isAuthenticated && user?.role === 1 && (
            <motion.div 
              className="premium-card"
              style={{ borderStyle: 'dashed', background: 'transparent', display: 'flex', flexDirection: 'column', justifyContent: 'center', alignItems: 'center', cursor: 'pointer', minHeight: '300px' }}
              whileHover={{ scale: 1.02 }}
            >
              <Plus size={48} style={{ color: 'var(--primary)', marginBottom: '16px' }} />
              <h3>Add New Brand</h3>
              <p style={{ color: 'var(--text-dim)' }}>Partner with toy creators</p>
            </motion.div>
          )}
        </div>
      )}
    </div>
  );
};

export default BrandListPage;
