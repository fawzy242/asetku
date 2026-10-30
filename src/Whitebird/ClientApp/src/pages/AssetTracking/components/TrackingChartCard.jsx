// src/pages/AssetTracking/components/TrackingChartCard.jsx
import React from 'react';
import {
  Chart as ChartJS,
  ArcElement,
  Tooltip as ChartTooltip,
  Legend,
} from 'chart.js';
import { Doughnut } from 'react-chartjs-2';
import { Typography } from '@mui/material';
import { FiPieChart } from 'react-icons/fi';
import Card from '../../../components/atoms/Card/Card';
import { useUIStore } from '../../../stores/uiStore';

ChartJS.register(ArcElement, ChartTooltip, Legend);

const TrackingChartCard = ({ transactionTypeChartData }) => {
  const theme = useUIStore((s) => s.theme);
  const isDark = theme === 'dark';
  const textColor = isDark ? '#f9fafb' : '#111827';

  const hasChartData =
    transactionTypeChartData.values.length > 0 &&
    transactionTypeChartData.values.some((v) => v > 0);

  const doughnutData = {
    labels: transactionTypeChartData.labels,
    datasets: [
      {
        data: transactionTypeChartData.values,
        backgroundColor: transactionTypeChartData.colors,
        borderWidth: 0,
        hoverOffset: 10,
        borderRadius: 6,
        spacing: 2,
      },
    ],
  };

  const doughnutOptions = {
    responsive: true,
    maintainAspectRatio: false,
    cutout: '62%',
    plugins: {
      legend: {
        position: 'bottom',
        labels: {
          color: textColor,
          padding: 12,
          usePointStyle: true,
          boxWidth: 10,
          boxHeight: 10,
          font: { size: 11, family: "'Inter', sans-serif" },
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
            const pct = total > 0 ? ((value / total) * 100).toFixed(1) : 0;
            return `${label}: ${value} (${pct}%)`;
          },
        },
      },
    },
  };

  return (
    <Card title="Transaction Types" subtitle="Distribution of transactions">
      {!hasChartData ? (
        <div className="asset-tracking__empty-chart">
          <FiPieChart size={32} />
          <Typography variant="body2" color="text.secondary">
            No transaction data
          </Typography>
        </div>
      ) : (
        <>
          <div className="asset-tracking__chart-wrapper">
            <Doughnut data={doughnutData} options={doughnutOptions} />
          </div>
          <div className="asset-tracking__chart-list">
            {transactionTypeChartData.details.map((item, index) => (
              <div key={index} className="asset-tracking__chart-item">
                <div className="asset-tracking__chart-label">
                  <span
                    className="asset-tracking__chart-dot"
                    style={{ backgroundColor: item.color }}
                  />
                  <span>{item.label}</span>
                </div>
                <div className="asset-tracking__chart-value">
                  <span>{item.count}</span>
                  <span className="asset-tracking__chart-percentage">
                    ({item.percentage}%)
                  </span>
                </div>
              </div>
            ))}
          </div>
        </>
      )}
    </Card>
  );
};

export default TrackingChartCard;