// src/pages/Dashboard/components/ChartsTab.jsx
import React from 'react';
import { Doughnut, Line, Bar } from 'react-chartjs-2';
import { Typography } from '@mui/material';
import { FiTrendingUp } from 'react-icons/fi';
import Card from '../../../components/atoms/Card/Card';
import DataTable from '../../../components/molecules/DataTable/DataTable';
import { MONTHLY_LABELS } from '../constants';
import { categoryBreakdownColumns } from '../columns';
import {
  buildDoughnutChartData,
  buildDoughnutChartOptions,
  buildLineChartData,
  buildLineChartOptions,
  buildBarChartData,
  buildBarChartOptions,
} from '../chartConfigs';
import { formatCurrency } from '../../../core/utils/number.utils';

const ChartsTab = ({
  stats,
  monthlyStats,
  categoryBreakdown,
  categoryBreakdownPage,
  onCategoryBreakdownPageChange,
  theme,
}) => {
  const isDark = theme === 'dark';
  const textColor = isDark ? '#f9fafb' : '#111827';
  const gridColor = isDark ? '#374151' : '#e5e7eb';

  const monthlyDataMap = {};
  (monthlyStats || []).forEach((item) => {
    monthlyDataMap[item.month - 1] = item.transactionCount;
  });
  const monthlyChartData = MONTHLY_LABELS.map((_, idx) => monthlyDataMap[idx] || 0);

  const categoryLabels = (categoryBreakdown || []).map(
    (item) => item.category || 'Uncategorized'
  );
  const categoryValues = (categoryBreakdown || []).map(
    (item) => item.totalValue || 0
  );
  const categoryColors = [
    '#dc2626',
    '#3b82f6',
    '#10b981',
    '#f59e0b',
    '#8b5cf6',
    '#06b6d4',
    '#ef4444',
    '#6b7280',
  ];

  const doughnutChartData = buildDoughnutChartData(stats);
  const doughnutChartOptions = buildDoughnutChartOptions({ isDark, textColor });
  const lineChartData = buildLineChartData(MONTHLY_LABELS, monthlyChartData);
  const lineChartOptions = buildLineChartOptions({
    isDark,
    textColor,
    gridColor,
    monthlyChartData,
  });
  const barChartData = buildBarChartData({
    categoryLabels,
    categoryValues,
    categoryColors,
  });
  const barChartOptions = buildBarChartOptions({
    isDark,
    textColor,
    gridColor,
    formatCurrency,
  });

  const paginatedCategoryBreakdown = (categoryBreakdown || []).slice(
    (categoryBreakdownPage - 1) * 10,
    categoryBreakdownPage * 10
  );

  return (
    <div className="dashboard__tab-content">
      <div className="dashboard__charts-row">
        <Card title="Asset Distribution" className="dashboard__chart-card">
          <div className="dashboard__chart-container">
            <Doughnut data={doughnutChartData} options={doughnutChartOptions} />
          </div>
        </Card>

        <Card title="Transaction Trend" className="dashboard__chart-card">
          <div className="dashboard__chart-container">
            <Line data={lineChartData} options={lineChartOptions} />
          </div>
          <div className="dashboard__chart-insight">
            <Typography variant="caption" color="text.secondary">
              <FiTrendingUp size={14} />
              {monthlyChartData.length > 1 &&
              monthlyChartData[monthlyChartData.length - 1] >
                monthlyChartData[monthlyChartData.length - 2]
                ? '↑ Increasing trend compared to last month'
                : '↓ Decreasing trend compared to last month'}
            </Typography>
          </div>
        </Card>
      </div>

      <div className="dashboard__full-width">
        <Card title="Asset Value by Category" className="dashboard__chart-card">
          <div className="dashboard__chart-container--large">
            <Bar data={barChartData} options={barChartOptions} />
          </div>
        </Card>
      </div>

      <div className="dashboard__summary-tables">
        {categoryBreakdown.length > 0 && (
          <Card
            title="Category Breakdown Details"
            className="dashboard__summary-card"
          >
            <DataTable
              rows={paginatedCategoryBreakdown}
              columns={categoryBreakdownColumns}
              pageSize={10}
              page={categoryBreakdownPage}
              onPageChange={onCategoryBreakdownPageChange}
              getRowId={(row) => row?.category || `cat-${Math.random()}`}
              hideFooter={false}
              ariaLabel="Category breakdown table"
              paginationMode="client"
              autoHeight={false}
            />
          </Card>
        )}
      </div>
    </div>
  );
};

export default ChartsTab;