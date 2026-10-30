// src/pages/Dashboard/chartConfigs.js
import { ASSET_STATUS_CHART_COLORS } from './constants';

export const buildDoughnutChartData = (stats = {}) => {
  const statusLabels = [
    'Available',
    'Assigned',
    'On Loan',
    'In Maintenance',
    'Damaged',
  ];
  const statusDataValues = [
    stats.availableAssets || 0,
    stats.assignedAssets || 0,
    stats.assetsOnLoan || 0,
    stats.assetsInMaintenance || 0,
    stats.damagedAssets || 0,
  ];
  const statusBackgroundColors = statusLabels.map(
    (s) => ASSET_STATUS_CHART_COLORS[s] || '#6b7280'
  );
  return {
    labels: statusLabels,
    datasets: [
      {
        data: statusDataValues,
        backgroundColor: statusBackgroundColors,
        borderWidth: 0,
        hoverOffset: 10,
        borderRadius: 8,
        spacing: 2,
      },
    ],
  };
};

export const buildDoughnutChartOptions = ({ isDark, textColor }) => ({
  responsive: true,
  maintainAspectRatio: false,
  cutout: '65%',
  plugins: {
    legend: {
      position: 'bottom',
      labels: {
        color: textColor,
        padding: 16,
        usePointStyle: true,
        pointStyleWidth: 12,
        pointStyleHeight: 12,
        font: { size: 12, family: "'Inter', sans-serif" },
        boxWidth: 12,
        boxHeight: 12,
      },
    },
    tooltip: {
      backgroundColor: isDark ? '#1f2937' : '#ffffff',
      titleColor: textColor,
      bodyColor: textColor,
      borderColor: isDark ? '#4b5563' : '#d1d5db',
      borderWidth: 1,
      padding: 10,
      cornerRadius: 8,
      callbacks: {
        label: (context) => {
          const label = context.label || '';
          const value = context.raw || 0;
          const total = context.dataset.data.reduce((a, b) => a + b, 0);
          const percentage = total > 0 ? ((value / total) * 100).toFixed(1) : 0;
          return `${label}: ${value} (${percentage}%)`;
        },
      },
    },
  },
});

export const buildLineChartData = (monthlyLabels, monthlyChartData) => ({
  labels: monthlyLabels,
  datasets: [
    {
      label: 'Transactions',
      data: monthlyChartData,
      borderColor: '#dc2626',
      backgroundColor: 'rgba(220, 38, 38, 0.1)',
      fill: true,
      tension: 0.4,
      pointBackgroundColor: '#dc2626',
      pointBorderColor: '#ffffff',
      pointBorderWidth: 2,
      pointRadius: 4,
      pointHoverRadius: 6,
      borderWidth: 2,
    },
  ],
});

export const buildLineChartOptions = ({
  isDark,
  textColor,
  gridColor,
  monthlyChartData,
}) => ({
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: { display: false },
    tooltip: {
      backgroundColor: isDark ? '#1f2937' : '#ffffff',
      titleColor: textColor,
      bodyColor: textColor,
      borderColor: isDark ? '#4b5563' : '#d1d5db',
      borderWidth: 1,
      padding: 10,
      cornerRadius: 8,
      callbacks: { label: (context) => `Transactions: ${context.raw}` },
    },
  },
  scales: {
    y: {
      grid: { color: gridColor },
      ticks: {
        color: textColor,
        stepSize: Math.max(
          1,
          Math.ceil(Math.max(...monthlyChartData, 10) / 5)
        ),
      },
      title: {
        display: true,
        text: 'Number of Transactions',
        color: textColor,
        font: { size: 12 },
      },
    },
    x: { grid: { display: false }, ticks: { color: textColor } },
  },
});

export const buildBarChartData = ({ categoryLabels, categoryValues, categoryColors }) => ({
  labels: categoryLabels,
  datasets: [
    {
      label: 'Asset Value (IDR)',
      data: categoryValues,
      backgroundColor: categoryColors.slice(0, categoryLabels.length),
      borderRadius: 8,
      barPercentage: 0.7,
      categoryPercentage: 0.8,
      hoverBackgroundColor: '#dc2626',
    },
  ],
});

export const buildBarChartOptions = ({ isDark, textColor, gridColor, formatCurrency }) => ({
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: { display: false },
    tooltip: {
      backgroundColor: isDark ? '#1f2937' : '#ffffff',
      titleColor: textColor,
      bodyColor: textColor,
      borderColor: isDark ? '#4b5563' : '#d1d5db',
      borderWidth: 1,
      padding: 10,
      cornerRadius: 8,
      callbacks: {
        label: (context) => `Value: ${formatCurrency(context.raw)}`,
      },
    },
  },
  scales: {
    y: {
      grid: { color: gridColor },
      ticks: { color: textColor, callback: (value) => formatCurrency(value) },
      title: {
        display: true,
        text: 'Total Value (IDR)',
        color: textColor,
        font: { size: 12 },
      },
    },
    x: {
      grid: { display: false },
      ticks: { color: textColor, font: { size: 11 } },
    },
  },
});