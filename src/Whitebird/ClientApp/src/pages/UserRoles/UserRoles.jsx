// src/pages/UserRoles/UserRoles.jsx
import React, { useState, useMemo, useCallback, useEffect, useRef } from 'react';
import { Box } from '@mui/material';
import { useQueryClient } from '@tanstack/react-query';
import { FiPlus, FiUserMinus } from 'react-icons/fi';
import { UserRolesData } from '../../core/services';
import PageHeader from '../../components/atoms/PageHeader';
import RefreshButton from '../../components/atoms/RefreshButton';
import Button from '../../components/atoms/Button/Button';
import Spinner from '../../components/atoms/Spinner/Spinner';
import Tabs from '../../components/molecules/Tabs/Tabs';
import SearchToolbar from '../../components/molecules/SearchToolbar/SearchToolbar';
import DataTable from '../../components/molecules/DataTable/DataTable';
import ActionMenu from '../../components/molecules/ActionMenu';
import { useGridData } from '../../hooks/useGridData';
import { useGridActions } from '../../hooks/useGridActions';
import { useSweetAlert } from '../../hooks/useSweetAlert';
import { useReferenceData } from '../../hooks/useReferenceData';
import { getAssignmentColumns } from './columns';
import AssignRoleDialog from './components/AssignRoleDialog';
import ReplaceRolesDialog from './components/ReplaceRolesDialog';
import './UserRoles.scss';

const userRolesData = new UserRolesData();

const TABS = [
  { id: 'all', label: 'All' },
  { id: 'active', label: 'Active' },
  { id: 'inactive', label: 'Inactive' },
];

const UserRoles = () => {
  const [activeTab, setActiveTab] = useState('all');
  const [searchTerm, setSearchTerm] = useState('');

  const [showAssignModal, setShowAssignModal] = useState(false);
  const [assignFormData, setAssignFormData] = useState({
    userId: '',
    roleId: '',
  });
  const [isAssignSubmitting, setIsAssignSubmitting] = useState(false);

  const [showReplaceModal, setShowReplaceModal] = useState(false);
  const [replaceUserId, setReplaceUserId] = useState(null);
  const [isReplaceSubmitting, setIsReplaceSubmitting] = useState(false);

  const isMountedRef = useRef(true);
  const queryClient = useQueryClient();
  const { toast, confirm, confirmDelete } = useSweetAlert();
  const { roles } = useReferenceData();

  const roleOptions = useMemo(
    () => [
      { value: '', label: 'Select Role' },
      ...(roles || []).map((r) => ({ value: r.value, label: r.label })),
    ],
    [roles]
  );

  const setAssignFormField = useCallback(
    (field) => (value) => {
      setAssignFormData((prev) => ({ ...prev, [field]: value }));
    },
    []
  );

  // ============================================================
  // GRID — DATA
  // ============================================================
  const buildFilters = useCallback(() => {
    const filters = {};
    if (activeTab === 'active') filters.isActive = true;
    else if (activeTab === 'inactive') filters.isActive = false;
    if (searchTerm) filters.search = searchTerm;
    return filters;
  }, [activeTab, searchTerm]);

  const fetchGridData = useCallback(
    async (params) => {
      const filters = buildFilters();
      const requestParams = {
        page: params.page || 1,
        pageSize: params.pageSize || 10,
        ...filters,
        ...params,
      };
      return await userRolesData.fetchGridData(requestParams);
    },
    [buildFilters]
  );

  const {
    data,
    totalCount,
    loading,
    page,
    setPage,
    pageSize,
    setPageSize,
    sortModel,
    setSortModel,
    reload,
  } = useGridData(['userRoles', activeTab, searchTerm], fetchGridData);

  // Augment rows with a composite id for stable row keys / menu selection.
  const augmentedData = useMemo(() => {
    return (data || []).map((r) => ({
      ...r,
      id: `${r.userId}-${r.roleId}`,
    }));
  }, [data]);

  useEffect(() => {
    isMountedRef.current = true;
    reload();
    return () => {
      isMountedRef.current = false;
    };
  }, [activeTab, reload]);

  // ============================================================
  // HANDLERS
  // ============================================================
  const handleSearch = useCallback(
    (search) => {
      setSearchTerm(search);
      setPage(1);
    },
    [setPage]
  );

  const handleRefresh = useCallback(() => {
    reload();
  }, [reload]);

  const handleTabChange = useCallback(
    (tab) => {
      setActiveTab(tab);
      setPage(1);
      setSearchTerm('');
    },
    [setPage]
  );

  // Assign
  const openAssign = useCallback(() => {
    setAssignFormData({ userId: '', roleId: '' });
    setShowAssignModal(true);
  }, []);

  const handleAssignSubmit = useCallback(
    async (e) => {
      if (e && typeof e.preventDefault === 'function') e.preventDefault();
      if (isAssignSubmitting) return;

      const userId = parseInt(assignFormData.userId, 10);
      const roleId = parseInt(assignFormData.roleId, 10);

      if (!userId || userId < 1) {
        toast.error('Please enter a valid User ID');
        return;
      }
      if (!roleId || roleId < 1) {
        toast.error('Please select a role');
        return;
      }

      setIsAssignSubmitting(true);
      const r = await userRolesData.assignRole(userId, roleId);
      setIsAssignSubmitting(false);

      if (r.success && isMountedRef.current) {
        toast.success('Role assigned successfully');
        setShowAssignModal(false);
        setAssignFormData({ userId: '', roleId: '' });
        queryClient.invalidateQueries({ queryKey: ['userRoles'] });
        reload();
      } else if (r.error) {
        toast.error(r.error);
      }
    },
    [isAssignSubmitting, assignFormData, queryClient, reload, toast]
  );

  // Unassign
  const handleUnassign = useCallback(
    async (row) => {
      const confirmed = await confirmDelete(
        'Remove Role Assignment',
        `Remove role #${row.roleId} from user #${row.userId}?`
      );
      if (!confirmed) return;
      const r = await userRolesData.unassignRole(row.userId, row.roleId);
      if (r.success && isMountedRef.current) {
        toast.success('Role assignment removed');
        queryClient.invalidateQueries({ queryKey: ['userRoles'] });
        reload();
      } else if (r.error) {
        toast.error(r.error);
      }
    },
    [queryClient, reload, toast, confirmDelete]
  );

  // Replace
  const openReplace = useCallback((row) => {
    setReplaceUserId(row?.userId ?? null);
    setShowReplaceModal(true);
  }, []);

  const handleReplaceSubmit = useCallback(
    async (roleIds) => {
      if (isReplaceSubmitting || !replaceUserId) return;
      setIsReplaceSubmitting(true);
      const r = await userRolesData.replaceUserRoles(replaceUserId, roleIds);
      setIsReplaceSubmitting(false);
      if (r.success && isMountedRef.current) {
        toast.success('Roles updated successfully');
        setShowReplaceModal(false);
        setReplaceUserId(null);
        queryClient.invalidateQueries({ queryKey: ['userRoles'] });
        reload();
      } else if (r.error) {
        toast.error(r.error);
      }
    },
    [isReplaceSubmitting, replaceUserId, queryClient, reload, toast]
  );

  // ============================================================
  // GRID ACTIONS
  // ============================================================
  const getRowActions = useCallback((row) => {
    const actions = [];
    actions.push('replaceRoles');
    if (row.roleId !== undefined && row.roleId !== null) {
      actions.push('unassign');
    }
    return actions;
  }, []);

  const handleRowAction = useCallback(
    (actionType, row) => {
      if (actionType === 'replaceRoles') {
        openReplace(row);
      } else if (actionType === 'unassign') {
        handleUnassign(row);
      }
    },
    [openReplace, handleUnassign]
  );

  const {
    actionColumn,
    menuAnchorEl,
    menuOpen,
    menuActions,
    selectedRow,
    handleMenuClose,
    handleActionClick,
  } = useGridActions({
    actions: [],
    onAction: handleRowAction,
    getConditionalActions: getRowActions,
    rowIdField: 'id',
  });

  const columns = useMemo(
    () => getAssignmentColumns(actionColumn),
    [actionColumn]
  );

  if (loading && !data.length) {
    return (
      <div className="page-loading">
        <Spinner size="lg" />
      </div>
    );
  }

  return (
    <div className="user-roles-menu">
      <PageHeader
        title="User Role Assignments"
        description="Assign roles to user accounts and manage existing assignments"
        actions={
          <>
            <RefreshButton onClick={handleRefresh} loading={loading} />
            <Button variant="primary" onClick={openAssign} startIcon={<FiPlus />}>
              Assign Role
            </Button>
          </>
        }
      />

      <Tabs tabs={TABS} activeTab={activeTab} onTabChange={handleTabChange} />

      <SearchToolbar onSearch={handleSearch} placeholder="Search assignments..." />

      <Box
        sx={{
          backgroundColor: 'var(--card-bg)',
          borderRadius: '12px',
          overflow: 'hidden',
          mb: 2,
          border: '1px solid var(--border)',
          width: '100%',
          minWidth: 0,
        }}
      >
        <DataTable
          rows={augmentedData}
          columns={columns}
          loading={loading}
          pageSize={pageSize}
          page={page}
          totalRowCount={totalCount}
          onPageChange={setPage}
          onPageSizeChange={setPageSize}
          getRowId={(row) => row?.id || `row-${Math.random()}`}
          checkboxSelection={false}
          onRowClick={openReplace}
          sortModel={sortModel}
          onSortModelChange={setSortModel}
          paginationMode="server"
          sortingMode="server"
          ariaLabel="User role assignments table"
        />
      </Box>

      <ActionMenu
        anchorEl={menuAnchorEl}
        open={menuOpen}
        onClose={handleMenuClose}
        actions={menuActions.map((a) =>
          a.type === 'unassign'
            ? { ...a, label: 'Remove Role', icon: FiUserMinus, color: 'error' }
            : a
        )}
        row={selectedRow}
        onAction={handleActionClick}
      />

      <AssignRoleDialog
        isOpen={showAssignModal}
        onClose={() => setShowAssignModal(false)}
        onSubmit={handleAssignSubmit}
        isSubmitting={isAssignSubmitting}
        formData={assignFormData}
        setFormField={setAssignFormField}
        roleOptions={roleOptions}
      />

      <ReplaceRolesDialog
        isOpen={showReplaceModal}
        onClose={() => {
          setShowReplaceModal(false);
          setReplaceUserId(null);
        }}
        onSubmit={handleReplaceSubmit}
        isSubmitting={isReplaceSubmitting}
        userId={replaceUserId}
        allRoles={roles || []}
        onLoadError={(msg) => toast.error(msg)}
      />
    </div>
  );
};

export default UserRoles;