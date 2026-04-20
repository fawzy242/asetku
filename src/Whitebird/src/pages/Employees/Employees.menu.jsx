import React, { useState, useEffect, useCallback } from 'react';
import EmployeesData from './Employees.data';
import DataTable from '../../components/molecules/DataTable/DataTable';
import Pagination from '../../components/molecules/Pagination/Pagination';
import SearchBar from '../../components/molecules/SearchBar/SearchBar';
import Button from '../../components/atoms/Button/Button';
import Badge from '../../components/atoms/Badge/Badge';
import Modal from '../../components/molecules/Modal/Modal';
import Input from '../../components/atoms/Input/Input';
import Select from '../../components/atoms/Select/Select';
import Spinner from '../../components/atoms/Spinner/Spinner';
import utilsHelper from '../../core/utils/utils.helper';

const EmployeesMenu = () => {
  const [loading, setLoading] = useState(true);
  const [employees, setEmployees] = useState([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [search, setSearch] = useState('');
  const [sortBy, setSortBy] = useState('fullName');
  const [sortDescending, setSortDescending] = useState(false);
  
  const [showModal, setShowModal] = useState(false);
  const [editingEmployee, setEditingEmployee] = useState(null);
  const [formData, setFormData] = useState({
    fullName: '',
    department: '',
    position: '',
    division: '',
    branch: '',
    costCenter: '',
    phoneNumber: '',
    email: '',
    officeLocation: '',
    employmentStatus: 'Active',
    joinDate: ''
  });

  const employeesData = new EmployeesData();

  const loadData = useCallback(async () => {
    setLoading(true);
    
    const result = await employeesData.loadGridData(page, pageSize, search, sortBy, sortDescending);
    
    if (result.success) {
      setEmployees(result.data.data || []);
      setTotalCount(result.data.totalCount || 0);
    }
    
    setLoading(false);
  }, [page, pageSize, search, sortBy, sortDescending]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const handleSearch = (value) => {
    setSearch(value);
    setPage(1);
  };

  const handleCreate = () => {
    setEditingEmployee(null);
    setFormData({
      fullName: '',
      department: '',
      position: '',
      division: '',
      branch: '',
      costCenter: '',
      phoneNumber: '',
      email: '',
      officeLocation: '',
      employmentStatus: 'Active',
      joinDate: ''
    });
    setShowModal(true);
  };

  const handleEdit = async (employee) => {
    const result = await employeesData.loadEmployee(employee.employeeId);
    if (result.success) {
      setEditingEmployee(result.data);
      setFormData({
        fullName: result.data.fullName || '',
        department: result.data.department || '',
        position: result.data.position || '',
        division: result.data.division || '',
        branch: result.data.branch || '',
        costCenter: result.data.costCenter || '',
        phoneNumber: result.data.phoneNumber || '',
        email: result.data.email || '',
        officeLocation: result.data.officeLocation || '',
        employmentStatus: result.data.employmentStatus || 'Active',
        joinDate: result.data.joinDate ? result.data.joinDate.split('T')[0] : ''
      });
      setShowModal(true);
    }
  };

  const handleDelete = async (employee) => {
    const result = await employeesData.deleteEmployee(employee.employeeId);
    if (result.success) {
      loadData();
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    let result;
    if (editingEmployee) {
      result = await employeesData.updateEmployee(editingEmployee.employeeId, formData);
    } else {
      result = await employeesData.createEmployee(formData);
    }
    
    if (result.success) {
      setShowModal(false);
      loadData();
    }
  };

  const columns = [
    { field: 'employeeCode', headerName: 'Code', width: 120 },
    { field: 'fullName', headerName: 'Full Name', width: 200 },
    { field: 'department', headerName: 'Department', width: 150 },
    { field: 'position', headerName: 'Position', width: 150 },
    { field: 'division', headerName: 'Division', width: 120 },
    { field: 'branch', headerName: 'Branch', width: 120 },
    { field: 'email', headerName: 'Email', width: 200 },
    {
      field: 'employmentStatus',
      headerName: 'Status',
      width: 120,
      renderCell: (params) => (
        <Badge variant={utilsHelper.getStatusColor(params.value)}>
          {params.value}
        </Badge>
      )
    },
    {
      field: 'actions',
      headerName: 'Actions',
      width: 150,
      sortable: false,
      renderCell: (params) => (
        <div className="table-actions">
          <Button
            variant="text"
            size="sm"
            onClick={() => handleEdit(params.row)}
          >
            ✏️
          </Button>
          <Button
            variant="text"
            size="sm"
            onClick={() => handleDelete(params.row)}
          >
            🗑️
          </Button>
        </div>
      )
    }
  ];

  if (loading && employees.length === 0) {
    return (
      <div className="employees-loading">
        <Spinner size="lg" />
      </div>
    );
  }

  return (
    <div className="employees-menu">
      <div className="employees-menu__header">
        <h2 className="employees-menu__title">Employee Management</h2>
        <Button variant="primary" onClick={handleCreate}>
          + Add Employee
        </Button>
      </div>
      
      <div className="employees-menu__toolbar">
        <SearchBar
          placeholder="Search employees..."
          onSearch={handleSearch}
          debounceTime={300}
        />
      </div>
      
      <div className="employees-menu__table">
        <DataTable
          rows={employees}
          columns={columns}
          loading={loading}
          pageSize={pageSize}
          getRowId={(row) => row.employeeId}
        />
      </div>
      
      <Pagination
        currentPage={page}
        totalPages={Math.ceil(totalCount / pageSize)}
        pageSize={pageSize}
        totalItems={totalCount}
        onPageChange={setPage}
        onPageSizeChange={(size) => {
          setPageSize(size);
          setPage(1);
        }}
      />

      {/* Employee Form Modal */}
      <Modal
        isOpen={showModal}
        onClose={() => setShowModal(false)}
        title={editingEmployee ? 'Edit Employee' : 'Add New Employee'}
        size="lg"
      >
        <form className="employee-form" onSubmit={handleSubmit}>
          <div className="employee-form__grid">
            <Input
              label="Full Name"
              value={formData.fullName}
              onChange={(e) => setFormData({ ...formData, fullName: e.target.value })}
              required
            />
            
            <Input
              label="Department"
              value={formData.department}
              onChange={(e) => setFormData({ ...formData, department: e.target.value })}
            />
            
            <Input
              label="Position"
              value={formData.position}
              onChange={(e) => setFormData({ ...formData, position: e.target.value })}
            />
            
            <Input
              label="Division"
              value={formData.division}
              onChange={(e) => setFormData({ ...formData, division: e.target.value })}
            />
            
            <Input
              label="Branch"
              value={formData.branch}
              onChange={(e) => setFormData({ ...formData, branch: e.target.value })}
            />
            
            <Input
              label="Cost Center"
              value={formData.costCenter}
              onChange={(e) => setFormData({ ...formData, costCenter: e.target.value })}
            />
            
            <Input
              label="Phone Number"
              value={formData.phoneNumber}
              onChange={(e) => setFormData({ ...formData, phoneNumber: e.target.value })}
            />
            
            <Input
              label="Email"
              type="email"
              value={formData.email}
              onChange={(e) => setFormData({ ...formData, email: e.target.value })}
            />
            
            <Input
              label="Office Location"
              value={formData.officeLocation}
              onChange={(e) => setFormData({ ...formData, officeLocation: e.target.value })}
            />
            
            <Select
              label="Employment Status"
              value={formData.employmentStatus}
              onChange={(e) => setFormData({ ...formData, employmentStatus: e.target.value })}
              options={[
                { value: 'Active', label: 'Active' },
                { value: 'Resigned', label: 'Resigned' },
                { value: 'On Leave', label: 'On Leave' },
                { value: 'Terminated', label: 'Terminated' }
              ]}
            />
            
            <Input
              label="Join Date"
              type="date"
              value={formData.joinDate}
              onChange={(e) => setFormData({ ...formData, joinDate: e.target.value })}
            />
          </div>
          
          <div className="modal-actions">
            <Button variant="outline" onClick={() => setShowModal(false)}>
              Cancel
            </Button>
            <Button type="submit" variant="primary">
              {editingEmployee ? 'Update' : 'Create'} Employee
            </Button>
          </div>
        </form>
      </Modal>
    </div>
  );
};

export default EmployeesMenu;