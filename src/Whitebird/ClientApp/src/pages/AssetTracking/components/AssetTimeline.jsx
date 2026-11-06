// src/pages/AssetTracking/components/AssetTimeline.jsx
import React from 'react';
import { FiActivity } from 'react-icons/fi';
import { formatDateTime } from '../../../core/utils/date.utils';
import { TRANSACTION_TYPE_COLORS } from '../constants';
import './AssetTimeline.scss';

const getColor = (activityType) => {
  const key = (activityType || '').toUpperCase();
  return TRANSACTION_TYPE_COLORS[key] || '#6b7280';
};

const hexToRgba = (hex, alpha) => {
  if (!hex || typeof hex !== 'string') return `rgba(107, 114, 128, ${alpha})`;
  const normalized = hex.startsWith('#') ? hex.slice(1) : hex;
  if (normalized.length !== 6) return `rgba(107, 114, 128, ${alpha})`;
  const r = parseInt(normalized.slice(0, 2), 16);
  const g = parseInt(normalized.slice(2, 4), 16);
  const b = parseInt(normalized.slice(4, 6), 16);
  return `rgba(${r}, ${g}, ${b}, ${alpha})`;
};

/**
 * Vertical timeline for asset tracking events.
 *
 * Layout: grid-based two-column structure per item —
 *   [rail (dot + connector)]  [card (content)]
 * The connector renders only when a subsequent item exists, so no orphan
 * line appears below the last event.
 *
 * @param {Array} items - { id, date, activityType, description,
 *                          employeeName, status, notes }
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
        const isLast = idx === sortedItems.length - 1;

        return (
          <div
            className="asset-timeline__item"
            key={item.id ?? `${item.date}-${item.activityType}-${idx}`}
            role="listitem"
          >
            <div className="asset-timeline__rail" aria-hidden="true">
              <span
                className="asset-timeline__dot"
                style={{
                  backgroundColor: color,
                  boxShadow: `0 0 0 3px var(--card-bg), 0 0 0 4px ${hexToRgba(color, 0.25)}`,
                }}
              />
              {!isLast && <span className="asset-timeline__connector" />}
            </div>

            <div className="asset-timeline__card">
              <div className="asset-timeline__header">
                <span
                  className="asset-timeline__badge"
                  style={{
                    backgroundColor: hexToRgba(color, 0.12),
                    color,
                  }}
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
                  <span className="asset-timeline__meta-item">
                    <strong>Employee</strong>
                    {item.employeeName}
                  </span>
                )}
                {item.status && item.status !== '-' && (
                  <span className="asset-timeline__meta-item">
                    <strong>Status</strong>
                    {item.status}
                  </span>
                )}
                {item.notes && (
                  <span className="asset-timeline__meta-item">
                    <strong>Notes</strong>
                    {item.notes}
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