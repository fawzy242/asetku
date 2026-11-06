// src/pages/Assets/AssetManagement.jsx
import React from 'react';
import Assets from './Assets';

/**
 * Asset Management — CRUD view.
 *
 * Renders the CRUD grid with tabs (All / Active / Inactive), category +
 * supplier filters, bulk actions, import, and Add Asset. Full create /
 * update / delete flows.
 *
 * Route: /assets/management
 */
const AssetManagement = () => {
  return <Assets initialMainTab="management" />;
};

export default AssetManagement;