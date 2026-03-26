import React from 'react';
import type { BrandEventItem } from '../../types/types';

type HistoryField = { key: string; value: string };

type Props = {
  historyOpen: boolean;
  historyLoading: boolean;
  historyError: string;
  historyItems: BrandEventItem[];
  onLoadHistory: () => void;
  getFields: (payload: string) => HistoryField[];
};

export const BrandHistoryTimeline: React.FC<Props> = ({
  historyOpen,
  historyLoading,
  historyError,
  historyItems,
  onLoadHistory,
  getFields,
}) => {
  return (
    <div className="premium-card" style={{ padding: '16px', background: 'var(--background)', gridColumn: '1 / -1' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', gap: '12px', alignItems: 'center', flexWrap: 'wrap' }}>
        <div>
          <div style={{ color: 'var(--text-dim)', fontSize: '12px', marginBottom: '4px' }}>Audit trail</div>
          <div style={{ fontWeight: 700 }}>Change History</div>
        </div>

        <button
          type="button"
          className="premium-card"
          onClick={onLoadHistory}
          style={{ display: 'inline-flex', alignItems: 'center', gap: '8px', padding: '10px 14px', background: 'var(--card-bg)' }}
        >
          <span style={{ fontWeight: 700, fontSize: '13px' }}>{historyOpen ? 'Refresh history' : 'Load history'}</span>
        </button>
      </div>

      {historyLoading && (
        <div style={{ marginTop: '14px', color: 'var(--text-dim)' }}>Loading history...</div>
      )}

      {historyError && (
        <div style={{ marginTop: '14px', color: 'var(--error)' }}>{historyError}</div>
      )}

      {historyOpen && !historyLoading && !historyError && (
        <div style={{ marginTop: '14px', display: 'grid', gap: '12px', maxHeight: '280px', overflowY: 'auto', paddingRight: '4px' }}>
          {historyItems.length > 0 ? historyItems.map((item, index) => (
            <div key={item.id} style={{ display: 'grid', gridTemplateColumns: '18px 1fr', gap: '12px', alignItems: 'start' }}>
              <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', paddingTop: '8px' }}>
                <div style={{ width: '10px', height: '10px', borderRadius: '999px', background: 'var(--primary)' }} />
                {index < historyItems.length - 1 && (
                  <div style={{ width: '2px', flex: 1, minHeight: '44px', background: 'rgba(148, 163, 184, 0.25)', marginTop: '4px' }} />
                )}
              </div>

              <div className="premium-card" style={{ padding: '14px', background: 'var(--background)', border: '1px solid var(--border)' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', gap: '12px', flexWrap: 'wrap', marginBottom: '10px' }}>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '8px', flexWrap: 'wrap' }}>
                    <span style={{ padding: '4px 10px', borderRadius: '999px', background: 'rgba(59, 130, 246, 0.12)', color: 'var(--primary)', fontSize: '12px', fontWeight: 700 }}>
                      {item.eventType}
                    </span>
                    <span style={{ color: 'var(--text-dim)', fontSize: '12px' }}>Event #{index + 1}</span>
                  </div>
                  <div style={{ color: 'var(--text-dim)', fontSize: '12px' }}>{new Date(item.createdAt).toLocaleString()}</div>
                </div>

                <div style={{ borderRadius: '14px', background: 'rgba(15, 23, 42, 0.04)', border: '1px solid rgba(148, 163, 184, 0.16)', padding: '12px' }}>
                  {getFields(item.payload).length > 0 ? (
                    <div style={{ display: 'grid', gap: '10px' }}>
                      {getFields(item.payload).map((field) => (
                        <div key={field.key} style={{ display: 'flex', justifyContent: 'space-between', gap: '12px', alignItems: 'flex-start', flexWrap: 'wrap' }}>
                          <div style={{ color: 'var(--text-dim)', fontSize: '12px', minWidth: '96px' }}>{field.key}</div>
                          <div style={{ fontWeight: 600, fontSize: '13px', textAlign: 'right', wordBreak: 'break-word' }}>{field.value}</div>
                        </div>
                      ))}
                    </div>
                  ) : (
                    <div style={{ color: 'var(--text-dim)', fontSize: '13px' }}>{item.payload}</div>
                  )}
                </div>
              </div>
            </div>
          )) : (
            <div style={{ color: 'var(--text-dim)', fontSize: '14px' }}>No history available.</div>
          )}
        </div>
      )}
    </div>
  );
};

