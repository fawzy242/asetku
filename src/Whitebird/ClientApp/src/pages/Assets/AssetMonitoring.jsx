// src/pages/Assets/AssetMonitoring.jsx
import React from 'react';
import Assets from './Assets';

/**
 * Asset Monitoring — read-only status view.
 *
 * Renders the status grid (Available / Assigned / On Loan / In Maintenance /
 * Damaged) with no CRUD actions, no import, no bulk operations. Row click
 * opens the AssetFormDialog in view mode.
 *
 * Route: /assets/monitoring
 */
const AssetMonitoring = () => {
  return <Assets initialMainTab="monitoring" />;
};

export default AssetMonitoring;