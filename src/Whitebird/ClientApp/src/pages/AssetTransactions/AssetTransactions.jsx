// src/pages/AssetTransactions/AssetTransactions.jsx
import React from 'react';
import { Navigate } from 'react-router-dom';

const AssetTransactions = () => {
  // Redirect ke monitoring sebagai default
  return <Navigate to="/transactions/monitoring" replace />;
};

export default AssetTransactions;