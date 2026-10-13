// src/pages/AssetTracking/components/AssetTimeline.jsx
import React from 'react';
import { FiActivity } from 'react-icons/fi';
import { formatDateTime } from '../../../core/utils/date.utils';
import './AssetTimeline.scss';

const TRANSACTION_TYPE_COLORS = {
  HANDOVER: '#3b82f6',
  TRANSFER: '#8b5cf6',
  LOAN: '#f59e0b',
  RETURN: '#10b981',
  LOAN_RETURN: '#34d399',
  MAINTENANCE: '#f97316',
  POST_MAINTENANCE: '#14b8a6',
  DISPOSAL: '#ef4444',
  OPERATIONAL_OFFICE: '#6366f1',
};

const getColor = (activityType) => {
  const key = (activityType || '').toUpperCase();
  return TRANSACTION_TYPE_COLORS[key] || '#6b7280';
};

/**
 * Visual timeline component for asset tracking events.
 * Renders a vertical connecting line with a colored dot per event.
 *
 * @param {Array} items - Timeline entries: { id, date, activityType, description, employeeName, status, notes }
 */
const AssetTimeline = ({ items = [] }) => {
  if (!items || items.length === 0) {
    return (
      <div className="asset-timeline asset-timeline--empty">
        <FiActivity size={40} />
        <p>No transaction history available</p>
      </div>
    );
  }

  // Chronological order (oldest first)
  const sortedItems = [...items].sort((a, b) => {
    const da = new Date(a.date).getTime();
    const db = new Date(b.date).getTime();
    if (isNaN(da) && isNaN(db)) return 0;
    if (isNaN(da)) return 1;
    if (isNaN(db)) return -1;
    return da - db;
  });

  return (
    <div className="asset-timeline" role="list">
      {sortedItems.map((item, idx) => {
        const color = getColor(item.activityType);
        return (
          <div
            className="asset-timeline__item"
            key={item.id ?? `${item.date}-${item.activityType}-${idx}`}
            role="listitem"
          >
            <div className="asset-timeline__marker">
              <span
                className="asset-timeline__dot"
                style={{ backgroundColor: color, boxShadow: `0 0 0 2px var(--border), 0 0 0 4px ${color}25` }}
              />
            </div>

            <div className="asset-timeline__card">
              <div className="asset-timeline__header">
                <span
                  className="asset-timeline__badge"
                  style={{ backgroundColor: `${color}1f`, color }}
                >
                  {item.activityType || 'Activity'}
                </span>
                <span className="asset-timeline__date">
                  {formatDateTime(item.date)}
                </span>
              </div>

              {item.description && (
                <p className="asset-timeline__description">{item.description}</p>
              )}

              <div className="asset-timeline__meta">
                {item.employeeName && item.employeeName !== '-' && (
                  <span>
                    <strong>Employee:</strong> {item.employeeName}
                  </span>
                )}
                {item.status && item.status !== '-' && (
                  <span>
                    <strong>Status:</strong> {item.status}
                  </span>
                )}
                {item.notes && (
                  <span>
                    <strong>Notes:</strong> {item.notes}
                  </span>
                )}
              </div>
            </div>
          </div>
        );
      })}
    </div>
  );
};

export default AssetTimeline;