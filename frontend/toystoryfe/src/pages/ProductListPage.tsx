import React, { useEffect, useState } from 'react';
import api from '../api/axios';
import { motion, AnimatePresence } from 'framer-motion';
import { Package, Search, Filter, ShoppingCart, Star, Plus } from 'lucide-react';
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

const ProductListPage: React.FC = () => {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [searchTerm, setSearchTerm] = useState('');
  const [sortDesc, setSortDesc] = useState(false);
  const { isAuthenticated, user } = useAuth();

  useEffect(() => {
    const fetchProducts = async () => {
      try {
        setLoading(true);
        const response = await api.get('/queries/products', {
          params: {
            page: 1,
            pageSize: 20,
            sortBy: 'Id',
            sortDesc: sortDesc
          }
        });
        // Extract from the .data.data property as per ApiResponse<T>
        const productsData = response.data.data || [];
        setProducts(Array.isArray(productsData) ? productsData : []);
      } catch (err: any) {
        setError('Failed to load products. Service might be temporarily unavailable.');
      } finally {
        setLoading(false);
      }
    };

    fetchProducts();
  }, [sortDesc]);

  const filteredProducts = products.filter(p => 
    (p.name?.toLowerCase() || '').includes(searchTerm.toLowerCase()) ||
    (p.brandName?.toLowerCase() || '').includes(searchTerm.toLowerCase())
  );

  return (
    <div style={{ padding: '40px 24px' }}>
      <header style={{ marginBottom: '48px', display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: '24px' }}>
        <div>
          <h1 style={{ margin: 0 }}>Available Toys</h1>
          <p style={{ color: 'var(--text-dim)' }}>Discover the best toys from top brands</p>
        </div>

        <div style={{ display: 'flex', gap: '16px', flex: 1, maxWidth: '600px' }}>
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
          {[1,2,3,4,5,6].map(i => (
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
                    <button className="shimmer-button" style={{ flex: 1, display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '8px' }}>
                      <ShoppingCart size={18} /> Buy Now
                    </button>
                    <button className="premium-card" style={{ padding: '12px', display: 'flex', alignItems: 'center', justifyContent: 'center', background: 'var(--background)' }}>
                      <Star size={18} />
                    </button>
                  </div>
                </div>
              </motion.div>
            ))}
          </AnimatePresence>

          {isAuthenticated && user?.role === 1 && (
            <motion.div 
              className="premium-card"
              style={{ borderStyle: 'dashed', background: 'transparent', display: 'flex', flexDirection: 'column', justifyContent: 'center', alignItems: 'center', cursor: 'pointer', minHeight: '400px' }}
              whileHover={{ scale: 1.02 }}
            >
              <Plus size={48} style={{ color: 'var(--primary)', marginBottom: '16px' }} />
              <h3>Add New Product</h3>
              <p style={{ color: 'var(--text-dim)' }}>Expand our toy collection</p>
            </motion.div>
          )}
        </div>
      )}
    </div>
  );
};

export default ProductListPage;
