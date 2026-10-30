// src/pages/UserRoles/components/ReplaceRolesDialog.jsx
import React, { useState, useEffect } from 'react';
import { Box, Typography, Checkbox, FormControlLabel } from '@mui/material';
import CrudModal from '../../../components/molecules/CrudModal/CrudModal';
import FormSection from '../../../components/atoms/FormSection/FormSection';
import Spinner from '../../../components/atoms/Spinner/Spinner';
import { UserRolesData } from '../../../core/services';

const userRolesData = new UserRolesData();

const ReplaceRolesDialog = ({
  isOpen,
  onClose,
  onSubmit,
  isSubmitting,
  userId,
  allRoles = [],
  onLoadError,
}) => {
  const [loading, setLoading] = useState(false);
  const [selectedRoleIds, setSelectedRoleIds] = useState([]);

  useEffect(() => {
    if (!isOpen || !userId) {
      setSelectedRoleIds([]);
      return;
    }

    let cancelled = false;
    setLoading(true);

    (async () => {
      const result = await userRolesData.fetchByUser(userId);
      if (cancelled) return;
      setLoading(false);

      if (result.success) {
        const currentRoleIds = (result.data || [])
          .map((r) => r.roleId)
          .filter((id) => id !== undefined && id !== null);
        setSelectedRoleIds(currentRoleIds);
      } else {
        setSelectedRoleIds([]);
        if (onLoadError) onLoadError(result.error);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [isOpen, userId, onLoadError]);

  const handleToggleRole = (roleId) => {
    setSelectedRoleIds((prev) =>
      prev.includes(roleId)
        ? prev.filter((id) => id !== roleId)
        : [...prev, roleId]
    );
  };

  const handleSubmit = (e) => {
    if (e && typeof e.preventDefault === 'function') e.preventDefault();
    onSubmit(selectedRoleIds);
  };

  return (
    <CrudModal
      isOpen={isOpen}
      onClose={onClose}
      title={`Replace Roles for User #${userId ?? '-'}`}
      onSubmit={handleSubmit}
      isSubmitting={isSubmitting}
      submitText="Replace Roles"
      cancelText="Cancel"
      disableSubmit={loading}
      size="md"
    >
      <FormSection
        title="Role Assignments"
        description="Check the roles this user should have. Unchecked roles will be removed."
      >
        {loading ? (
          <Spinner size="sm" />
        ) : allRoles.length === 0 ? (
          <Typography variant="body2" color="text.secondary">
            No roles available. Create roles first.
          </Typography>
        ) : (
          <Box
            sx={{
              display: 'flex',
              flexDirection: 'column',
              gap: 0.5,
              maxHeight: 340,
              overflowY: 'auto',
            }}
          >
            {allRoles.map((role) => (
              <FormControlLabel
                key={role.value}
                control={
                  <Checkbox
                    checked={selectedRoleIds.includes(role.value)}
                    onChange={() => handleToggleRole(role.value)}
                  />
                }
                label={role.label}
                sx={{
                  p: 1,
                  borderRadius: 1,
                  '&:hover': { bgcolor: 'var(--surface)' },
                }}
              />
            ))}
          </Box>
        )}
      </FormSection>
    </CrudModal>
  );
};

export default ReplaceRolesDialog;