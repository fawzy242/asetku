// src/pages/EmployeeSummary/components/EmployeeSummaryProfileCard.jsx
import React from 'react';
import { Grid, Avatar, Typography, Chip } from '@mui/material';
import { FiBriefcase, FiMail, FiPhone, FiCalendar } from 'react-icons/fi';
import Card from '../../../components/atoms/Card/Card';
import StatusChip from '../../../components/atoms/StatusChip/StatusChip';
import InfoRow from '../../../components/molecules/InfoRow/InfoRow';
import { formatDate } from '../../../core/utils/date.utils';

const EmployeeSummaryProfileCard = ({ employeeDetail }) => {
  return (
    <Card className="employee-summary__profile-card">
      <Grid container spacing={3} alignItems="center">
        <Grid item>
          <Avatar className="employee-summary__profile-avatar">
            {employeeDetail?.fullName?.charAt(0) || '?'}
          </Avatar>
        </Grid>
        <Grid item xs>
          <Typography
            variant="h5"
            fontWeight={700}
            className="employee-summary__profile-name"
          >
            {employeeDetail?.fullName || '-'}
          </Typography>
          <div className="employee-summary__profile-tags">
            <Chip
              icon={<FiBriefcase />}
              label={employeeDetail?.employeeCode || '-'}
              size="small"
              variant="outlined"
            />
            <Chip
              icon={<FiBriefcase />}
              label={employeeDetail?.positionName || '-'}
              size="small"
              variant="outlined"
            />
            <Chip
              icon={<FiBriefcase />}
              label={employeeDetail?.departmentName || '-'}
              size="small"
              variant="outlined"
            />
            <Chip
              icon={<FiMail />}
              label={employeeDetail?.email || '-'}
              size="small"
              variant="outlined"
            />
            <Chip
              icon={<FiPhone />}
              label={employeeDetail?.phoneNumber || '-'}
              size="small"
              variant="outlined"
            />
            <StatusChip status={employeeDetail?.employmentStatusName} />
          </div>
        </Grid>
        <Grid item>
          <InfoRow
            icon={FiCalendar}
            label="Join Date"
            value={formatDate(employeeDetail?.joinDate)}
          />
          {employeeDetail?.resignDate && (
            <InfoRow
              icon={FiCalendar}
              label="Resign Date"
              value={formatDate(employeeDetail?.resignDate)}
            />
          )}
        </Grid>
      </Grid>
    </Card>
  );
};

export default EmployeeSummaryProfileCard;