import { Shield, AlertTriangle, RefreshCw, CheckCircle, XCircle, Zap, Settings } from 'lucide-react';

export const getServiceIcon = (service: string) => {
    switch (service.toLowerCase()) {
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

export const getStateConfig = (state: string) => {
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