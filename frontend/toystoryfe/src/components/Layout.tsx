import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { LogOut, Package, Tag, User } from 'lucide-react';

const Layout: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { isAuthenticated, user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <div className="layout-root">
      <header className="glass-header">
        <nav style={{ maxWidth: '1200px', margin: '0 auto', padding: '16px 24px', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <Link to="/" style={{ display: 'flex', alignItems: 'center', gap: '8px', textDecoration: 'none', fontWeight: 800, fontSize: '24px', color: 'var(--primary)' }}>
            <Package size={28} />
            <span>ToyStory</span>
          </Link>
          <div style={{ display: 'flex', gap: '24px', alignItems: 'center' }}>
            <Link to="/products" style={{ textDecoration: 'none', color: 'var(--text-main)', display: 'flex', alignItems: 'center', gap: '4px' }}>
              <Package size={18} /> Products
            </Link>
            <Link to="/brands" style={{ textDecoration: 'none', color: 'var(--text-main)', display: 'flex', alignItems: 'center', gap: '4px' }}>
              <Tag size={18} /> Brands
            </Link>
            {isAuthenticated ? (
              <div style={{ display: 'flex', alignItems: 'center', gap: '16px' }}>
                <Link to="/profile" style={{ display: 'flex', alignItems: 'center', gap: '8px', color: 'var(--text-main)' }}>
                  <User size={18} /> {user?.email}
                </Link>
                <button onClick={handleLogout} style={{ background: 'transparent', border: 'none', color: 'var(--error)', cursor: 'pointer', display: 'flex', alignItems: 'center', gap: '4px' }}>
                  <LogOut size={18} /> Logout
                </button>
              </div>
            ) : (
              <Link to="/login" className="shimmer-button" style={{ textDecoration: 'none', padding: '8px 20px' }}>
                Login
              </Link>
            )}
          </div>
        </nav>
      </header>

      <main style={{ maxWidth: '1200px', margin: '0 auto', minHeight: 'calc(100vh - 73px)' }}>
        {children}
      </main>

      <footer style={{ borderTop: '1px solid var(--border)', padding: '40px 24px', marginTop: 'auto', textAlign: 'center', color: 'var(--text-dim)' }}>
        <p>&copy; 2026 ToyStory Marketplace. All rights reserved.</p>
      </footer>
    </div>
  );
};

export default Layout;
