import React, { useState, useEffect, useCallback } from 'react';
import { motion } from 'framer-motion';
import { Shield, AlertTriangle, Power, PowerOff, RefreshCw, CheckCircle, XCircle, Zap, Settings } from 'lucide-react';
import { useAuth } from '../context/AuthContext';
import api from '../api/axios';

interface ServiceStatus {
    state: 'Closed' | 'Isolated' | 'Open' | 'HalfOpen';
    isManuallyControlled: boolean;
    manualControlHash: number;
}

interface AllServicesStatus {
    brandService: ServiceStatus;
    categoryService: ServiceStatus;
    productService: ServiceStatus;
}

const CircuitBreakerDashboard: React.FC = () => {
    const [status, setStatus] = useState<AllServicesStatus | null>(null);
    const [loading, setLoading] = useState(true);
    const [actionLoading, setActionLoading] = useState<string | null>(null);
    const [error, setError] = useState('');
    const [successMessage, setSuccessMessage] = useState('');
    const { isAuthenticated, user } = useAuth();
    const isAdmin = isAuthenticated && user?.role === 1;

    // Single fetch function - only called when needed
    const fetchStatus = useCallback(async () => {
        try {
            setLoading(true);
            setError('');
            const response = await api.get('/../internal/CircuitBreaker/status');
            setStatus(response.data);
        } catch (err: any) {
            setError('Failed to fetch circuit breaker status');
            console.error(err);
        } finally {
            setLoading(false);
        }
    }, []);

    // Only fetch on initial mount
    useEffect(() => {
        fetchStatus();
    }, [fetchStatus]);

    const handleIsolate = async (service: string) => {
        if (!window.confirm(`Are you sure you want to isolate the ${service} service?`)) {
            return;
        }

        try {
            setActionLoading(service);
            setError('');
            await api.post(`/../internal/CircuitBreaker/isolate/${service}`);
            await fetchStatus(); // Refresh after isolating
            setSuccessMessage(`${service} service isolated successfully`);
            setTimeout(() => setSuccessMessage(''), 3000);
        } catch (err: any) {
            setError(`Failed to isolate ${service} service`);
            console.error(err);
        } finally {
            setActionLoading(null);
        }
    };

    const handleClose = async (service: string) => {
        if (!window.confirm(`Are you sure you want to close the circuit for ${service} service?`)) {
            return;
        }

        try {
            setActionLoading(service);
            setError('');
            await api.post(`/../internal/CircuitBreaker/close/${service}`);
            await fetchStatus(); // Refresh after closing
            setSuccessMessage(`${service} circuit closed successfully`);
            setTimeout(() => setSuccessMessage(''), 3000);
        } catch (err: any) {
            setError(`Failed to close circuit for ${service} service`);
            console.error(err);
        } finally {
            setActionLoading(null);
        }
    };

    const getServiceIcon = (service: string) => {
        switch (service) {
            case 'brand':
                return <Shield size={24} />;
            case 'category':
                return <Settings size={24} />;
            case 'product':
                return <Zap size={24} />;
            default:
                return <Shield size={24} />;
        }
    };

    const getStateConfig = (state: string) => {
        switch (state) {
            case 'Closed':
                return {
                    icon: <CheckCircle size={20} />,
                    color: '#10b981',
                    bgColor: 'rgba(16, 185, 129, 0.1)',
                    label: 'Closed',
                    description: 'Service is operating normally'
                };
            case 'Isolated':
                return {
                    icon: <XCircle size={20} />,
                    color: '#ef4444',
                    bgColor: 'rgba(239, 68, 68, 0.1)',
                    label: 'Isolated',
                    description: 'Service is manually isolated'
                };
            case 'Open':
                return {
                    icon: <AlertTriangle size={20} />,
                    color: '#f59e0b',
                    bgColor: 'rgba(245, 158, 11, 0.1)',
                    label: 'Open',
                    description: 'Circuit is open due to failures'
                };
            case 'HalfOpen':
                return {
                    icon: <RefreshCw size={20} />,
                    color: '#8b5cf6',
                    bgColor: 'rgba(139, 92, 246, 0.1)',
                    label: 'Half-Open',
                    description: 'Testing service recovery'
                };
            default:
                return {
                    icon: <Shield size={20} />,
                    color: '#64748b',
                    bgColor: 'rgba(100, 116, 139, 0.1)',
                    label: state,
                    description: 'Unknown state'
                };
        }
    };

    if (!isAdmin) {
        return (
            <div style={{ textAlign: 'center', padding: '80px' }}>
                <Shield size={64} style={{ opacity: 0.2, marginBottom: '24px' }} />
                <h2>Access Denied</h2>
                <p style={{ color: 'var(--text-dim)' }}>You don't have permission to view this page.</p>
            </div>
        );
    }

    if (loading && !status) {
        return (
            <div>
                <header style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '32px' }}>
                    <div>
                        <h1>Circuit Breaker Dashboard</h1>
                        <p style={{ color: 'var(--text-dim)' }}>Monitor and control service circuit breakers</p>
                    </div>
                    <button
                        onClick={fetchStatus}
                        className="premium-card"
                        style={{ padding: '10px 20px', display: 'flex', alignItems: 'center', gap: '8px' }}
                        disabled={loading}
                    >
                        <RefreshCw size={18} className={loading ? 'spin' : ''} />
                        Refresh
                    </button>
                </header>
                <div className="dashboard-grid">
                    {[1, 2, 3].map(i => (
                        <div key={i} className="premium-card" style={{ height: '280px', background: 'var(--border)', opacity: 0.3, animation: 'pulse 1.5s ease-in-out infinite' }} />
                    ))}
                </div>
            </div>
        );
    }

    return (
        <div>
            <header style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '32px' }}>
                <div>
                    <h1>Circuit Breaker Dashboard</h1>
                    <p style={{ color: 'var(--text-dim)' }}>Monitor and control service circuit breakers</p>
                </div>
                <button
                    onClick={fetchStatus}
                    className="premium-card"
                    style={{ padding: '10px 20px', display: 'flex', alignItems: 'center', gap: '8px' }}
                    disabled={loading}
                >
                    <RefreshCw size={18} className={loading ? 'spin' : ''} />
                    Refresh
                </button>
            </header>

            {error && (
                <div style={{
                    background: 'rgba(239, 68, 68, 0.1)',
                    border: '1px solid var(--error)',
                    padding: '12px 16px',
                    borderRadius: '12px',
                    color: 'var(--error)',
                    marginBottom: '24px',
                    display: 'flex',
                    alignItems: 'center',
                    gap: '8px'
                }}>
                    <AlertTriangle size={18} />
                    {error}
                </div>
            )}

            {successMessage && (
                <div style={{
                    background: 'rgba(16, 185, 129, 0.1)',
                    border: '1px solid #10b981',
                    padding: '12px 16px',
                    borderRadius: '12px',
                    color: '#10b981',
                    marginBottom: '24px',
                    display: 'flex',
                    alignItems: 'center',
                    gap: '8px'
                }}>
                    <CheckCircle size={18} />
                    {successMessage}
                </div>
            )}

            {status && (
                <div className="dashboard-grid" style={{ gridTemplateColumns: 'repeat(auto-fit, minmax(320px, 1fr))' }}>
                    {Object.entries(status).map(([service, serviceStatus]) => {
                        const serviceName = service.replace('Service', '');
                        const stateConfig = getStateConfig(serviceStatus.state);
                        const isIsolated = serviceStatus.state === 'Isolated' || serviceStatus.state === 'Open';
                        const isLoading = actionLoading === serviceName;

                        return (
                            <motion.div
                                key={service}
                                initial={{ opacity: 0, y: 20 }}
                                animate={{ opacity: 1, y: 0 }}
                                className="premium-card"
                                style={{ padding: '28px', position: 'relative', overflow: 'hidden' }}
                            >
                                <div style={{
                                    position: 'absolute',
                                    top: 0,
                                    left: 0,
                                    right: 0,
                                    height: '4px',
                                    background: stateConfig.color,
                                }} />

                                <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: '20px' }}>
                                    <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
                                        <div style={{
                                            width: '48px',
                                            height: '48px',
                                            borderRadius: '12px',
                                            background: stateConfig.bgColor,
                                            display: 'flex',
                                            alignItems: 'center',
                                            justifyContent: 'center',
                                            color: stateConfig.color
                                        }}>
                                            {getServiceIcon(serviceName)}
                                        </div>
                                        <div>
                                            <h3 style={{ margin: 0, textTransform: 'capitalize' }}>
                                                {serviceName}
                                            </h3>
                                            <div style={{ fontSize: '12px', color: 'var(--text-dim)' }}>
                                                Hash: {serviceStatus.manualControlHash}
                                            </div>
                                        </div>
                                    </div>
                                    <div style={{
                                        padding: '6px 12px',
                                        borderRadius: '100px',
                                        background: stateConfig.bgColor,
                                        color: stateConfig.color,
                                        fontWeight: 600,
                                        fontSize: '13px',
                                        display: 'flex',
                                        alignItems: 'center',
                                        gap: '6px'
                                    }}>
                                        {stateConfig.icon}
                                        {stateConfig.label}
                                    </div>
                                </div>

                                <div style={{ marginBottom: '24px' }}>
                                    <p style={{ color: 'var(--text-dim)', fontSize: '14px', margin: 0 }}>
                                        {stateConfig.description}
                                    </p>
                                    {serviceStatus.isManuallyControlled && (
                                        <div style={{
                                            marginTop: '8px',
                                            fontSize: '12px',
                                            color: '#f59e0b',
                                            display: 'flex',
                                            alignItems: 'center',
                                            gap: '4px'
                                        }}>
                                            <Settings size={12} />
                                            Manually controlled
                                        </div>
                                    )}
                                </div>

                                <div style={{ display: 'flex', gap: '12px' }}>
                                    {isIsolated ? (
                                        <button
                                            onClick={() => handleClose(serviceName)}
                                            disabled={!!actionLoading}
                                            className="shimmer-button"
                                            style={{
                                                flex: 1,
                                                display: 'flex',
                                                alignItems: 'center',
                                                justifyContent: 'center',
                                                gap: '8px',
                                                background: '#10b981',
                                                color: 'white',
                                                border: 'none'
                                            }}
                                        >
                                            {isLoading ? (
                                                <RefreshCw size={18} className="spin" />
                                            ) : (
                                                <Power size={18} />
                                            )}
                                            Close Circuit
                                        </button>
                                    ) : (
                                        <button
                                            onClick={() => handleIsolate(serviceName)}
                                            disabled={!!actionLoading}
                                            style={{
                                                flex: 1,
                                                display: 'flex',
                                                alignItems: 'center',
                                                justifyContent: 'center',
                                                gap: '8px',
                                                padding: '12px',
                                                borderRadius: '12px',
                                                border: '1px solid rgba(239, 68, 68, 0.3)',
                                                background: 'rgba(239, 68, 68, 0.05)',
                                                color: '#ef4444',
                                                cursor: 'pointer',
                                                fontWeight: 600,
                                                transition: 'all 0.2s'
                                            }}
                                            onMouseEnter={(e) => {
                                                e.currentTarget.style.background = 'rgba(239, 68, 68, 0.1)';
                                                e.currentTarget.style.borderColor = '#ef4444';
                                            }}
                                            onMouseLeave={(e) => {
                                                e.currentTarget.style.background = 'rgba(239, 68, 68, 0.05)';
                                                e.currentTarget.style.borderColor = 'rgba(239, 68, 68, 0.3)';
                                            }}
                                        >
                                            {isLoading ? (
                                                <RefreshCw size={18} className="spin" />
                                            ) : (
                                                <PowerOff size={18} />
                                            )}
                                            Isolate Service
                                        </button>
                                    )}
                                </div>

                                {serviceStatus.state === 'Open' && (
                                    <div style={{
                                        position: 'absolute',
                                        bottom: '16px',
                                        right: '16px',
                                        width: '8px',
                                        height: '8px',
                                        borderRadius: '50%',
                                        background: '#f59e0b',
                                        animation: 'pulse 1s ease-in-out infinite'
                                    }} />
                                )}
                                {serviceStatus.state === 'HalfOpen' && (
                                    <div style={{
                                        position: 'absolute',
                                        bottom: '16px',
                                        right: '16px',
                                        width: '8px',
                                        height: '8px',
                                        borderRadius: '50%',
                                        background: '#8b5cf6',
                                        animation: 'pulse 0.5s ease-in-out infinite'
                                    }} />
                                )}
                            </motion.div>
                        );
                    })}
                </div>
            )}

            <style>{`
        @keyframes pulse {
          0%, 100% {
            opacity: 1;
          }
          50% {
            opacity: 0.3;
          }
        }
        .spin {
          animation: spin 1s linear infinite;
        }
        @keyframes spin {
          from {
            transform: rotate(0deg);
          }
          to {
            transform: rotate(360deg);
          }
        }
      `}</style>
        </div>
    );
};

export default CircuitBreakerDashboard;