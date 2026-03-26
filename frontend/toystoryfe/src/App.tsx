import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import Layout from './components/Layout';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import ProductListPage from './pages/ProductListPage';
import BrandListPage from './pages/BrandListPage';
import './premium.css';
import CircuitBreakerDashboard from './pages/CircuitBreakerDashboard';

const App: React.FC = () => {
  return (
    <AuthProvider>
      <Router>
        <Layout>
          <Routes>
            {/* Redirect root to products */}
            <Route path="/" element={<Navigate to="/products" replace />} />

            {/* Auth Routes */}
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />

            {/* Data Routes */}
            <Route path="/products" element={<ProductListPage />} />
            <Route path="/brands" element={<BrandListPage />} />
            <Route path="/circuit" element={<CircuitBreakerDashboard />} />


            {/* Catch-all */}
            <Route path="*" element={<Navigate to="/products" replace />} />
          </Routes>
        </Layout>
      </Router>
    </AuthProvider>
  );
};

export default App;
