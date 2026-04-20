import AssetTransactionsApi from './AssetTransactions.api';
import Swal from 'sweetalert2';
import utilsHelper from '../../core/utils/utils.helper';

class AssetTransactionsData {
  constructor() {
    this.api = AssetTransactionsApi;
  }

  async loadGridData(page, pageSize, search, status, assetId) {
    try {
      const params = { page, pageSize };
      if (search) params.search = search;
      if (status) params.status = status;
      if (assetId) params.assetId = assetId;

      const result = await this.api.getGridData(params);
      
      return {
        success: true,
        data: result.data
      };
    } catch (error) {
      return {
        success: false,
        error: 'Failed to load transactions'
      };
    }
  }

  async loadTransaction(id) {
    try {
      const result = await this.api.getById(id);
      
      if (result.isSuccess) {
        return {
          success: true,
          data: result.data
        };
      }
      
      return {
        success: false,
        error: result.message
      };
    } catch (error) {
      return {
        success: false,
        error: 'Failed to load transaction'
      };
    }
  }

  async createTransaction(data) {
    try {
      const result = await this.api.create(data);
      
      if (result.isSuccess) {
        Swal.fire({
          title: 'Success',
          text: 'Transaction created successfully',
          icon: 'success',
          confirmButtonColor: '#dc2626'
        });
        return { success: true, data: result.data };
      }
      
      Swal.fire({
        title: 'Error',
        text: result.message || 'Failed to create transaction',
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
      
      return { success: false, error: result.message };
    } catch (error) {
      Swal.fire({
        title: 'Error',
        text: 'Failed to create transaction',
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
      return { success: false, error: 'Failed to create transaction' };
    }
  }

  async updateTransaction(id, data) {
    try {
      const result = await this.api.update(id, data);
      
      if (result.isSuccess) {
        Swal.fire({
          title: 'Success',
          text: 'Transaction updated successfully',
          icon: 'success',
          confirmButtonColor: '#dc2626'
        });
        return { success: true, data: result.data };
      }
      
      Swal.fire({
        title: 'Error',
        text: result.message || 'Failed to update transaction',
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
      
      return { success: false, error: result.message };
    } catch (error) {
      Swal.fire({
        title: 'Error',
        text: 'Failed to update transaction',
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
      return { success: false, error: 'Failed to update transaction' };
    }
  }

  async approveTransaction(id, isApproved, notes) {
    try {
      const result = await this.api.approve(id, { 
        assetTransactionId: id,
        isApproved,
        approvalNotes: notes 
      });
      
      if (result.isSuccess) {
        Swal.fire({
          title: 'Success',
          text: isApproved ? 'Transaction approved' : 'Transaction rejected',
          icon: 'success',
          confirmButtonColor: '#dc2626'
        });
        return { success: true };
      }
      
      Swal.fire({
        title: 'Error',
        text: result.message || 'Failed to process approval',
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
      
      return { success: false };
    } catch (error) {
      Swal.fire({
        title: 'Error',
        text: 'Failed to process approval',
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
      return { success: false };
    }
  }

  async returnAsset(transactionId, actualReturnDate, conditionAfter, notes) {
    try {
      const result = await this.api.returnAsset({
        assetTransactionId: transactionId,
        actualReturnDate,
        conditionAfter,
        notes
      });
      
      if (result.isSuccess) {
        Swal.fire({
          title: 'Success',
          text: 'Asset returned successfully',
          icon: 'success',
          confirmButtonColor: '#dc2626'
        });
        return { success: true };
      }
      
      Swal.fire({
        title: 'Error',
        text: result.message || 'Failed to return asset',
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
      
      return { success: false };
    } catch (error) {
      Swal.fire({
        title: 'Error',
        text: 'Failed to return asset',
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
      return { success: false };
    }
  }

  async cancelTransaction(id) {
    try {
      const result = await Swal.fire({
        title: 'Cancel Transaction?',
        text: 'This transaction will be cancelled.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#dc2626',
        confirmButtonText: 'Yes, cancel it'
      });
      
      if (!result.isConfirmed) {
        return { success: false, cancelled: true };
      }
      
      const cancelResult = await this.api.cancel(id);
      
      if (cancelResult.isSuccess) {
        Swal.fire({
          title: 'Cancelled',
          text: 'Transaction has been cancelled.',
          icon: 'success',
          confirmButtonColor: '#dc2626'
        });
        return { success: true };
      }
      
      Swal.fire({
        title: 'Error',
        text: cancelResult.message || 'Failed to cancel transaction',
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
      
      return { success: false };
    } catch (error) {
      Swal.fire({
        title: 'Error',
        text: 'Failed to cancel transaction',
        icon: 'error',
        confirmButtonColor: '#dc2626'
      });
      return { success: false };
    }
  }

  async loadDropdownData() {
    try {
      const [assets, employees, locations] = await Promise.all([
        this.api.getAssets(),
        this.api.getEmployees(),
        this.api.getLocations()
      ]);
      
      return {
        success: true,
        data: {
          assets: assets.data || [],
          employees: employees.data || [],
          locations: locations.data || []
        }
      };
    } catch (error) {
      return {
        success: false,
        error: 'Failed to load dropdown data'
      };
    }
  }
}

export default AssetTransactionsData;