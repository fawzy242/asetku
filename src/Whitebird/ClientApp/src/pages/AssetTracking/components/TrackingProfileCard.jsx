// src/pages/AssetTracking/components/TrackingProfileCard.jsx
import React from 'react';
import { Grid, Avatar, Typography, Chip } from '@mui/material';
import {
  FiBox,
  FiUser,
  FiMapPin,
  FiRefreshCw,
  FiCalendar,
  FiClock,
  FiAlertTriangle,
  FiActivity,
} from 'react-icons/fi';
import Card from '../../../components/atoms/Card/Card';
import StatusChip from '../../../components/atoms/StatusChip/StatusChip';
import InfoRow from '../../../components/molecules/InfoRow/InfoRow';
import { formatDate } from '../../../core/utils/date.utils';

const TrackingProfileCard = ({ currentAsset }) => {
  return (
    <Card className="asset-tracking__profile-card">
      <Grid container spacing={3} alignItems="center">
        <Grid item>
          <Avatar className="asset-tracking__profile-avatar">
            <FiBox size={36} />
          </Avatar>
        </Grid>
        <Grid item xs>
          <Typography
            variant="h5"
            fontWeight={700}
            className="asset-tracking__profile-name"
          >
            {currentAsset.assetName || '-'}
          </Typography>
          <div className="asset-tracking__profile-tags">
            <Chip
              label={`Code: ${currentAsset.assetCode || '-'}`}
              size="small"
              variant="outlined"
            />
            <Chip
              label={`Category: ${currentAsset.categoryName || '-'}`}
              size="small"
              variant="outlined"
            />
            <StatusChip status={currentAsset.currentStatus || 'Available'} />
            {currentAsset.isOnLoan && (
              <Chip
                icon={<FiClock />}
                label="On Loan"
                size="small"
                color="warning"
                variant="outlined"
              />
            )}
            {currentAsset.isInMaintenance && (
              <Chip
                icon={<FiActivity />}
                label="In Maintenance"
                size="small"
                color="info"
                variant="outlined"
              />
            )}
            {currentAsset.isOverdue && (
              <Chip
                icon={<FiAlertTriangle />}
                label="Overdue"
                size="small"
                color="error"
              />
            )}
          </div>
        </Grid>
        <Grid item>
          <InfoRow
            icon={FiUser}
            label="Holder"
            value={currentAsset.currentHolderName || '-'}
          />
          <InfoRow
            icon={FiMapPin}
            label="Location"
            value={currentAsset.currentLocation || '-'}
          />
          <InfoRow
            icon={FiActivity}
            label="Condition"
            value={currentAsset.conditionName || '-'}
          />
          {currentAsset.loanDueDate && (
            <InfoRow
              icon={FiCalendar}
              label="Due Date"
              value={formatDate(currentAsset.loanDueDate)}
              className={currentAsset.isOverdue ? 'u-text-error' : ''}
            />
          )}
          <InfoRow
            icon={FiRefreshCw}
            label="Total Transactions"
            value={currentAsset.totalTransactions || 0}
          />
        </Grid>
      </Grid>
    </Card>
  );
};

export default TrackingProfileCard;