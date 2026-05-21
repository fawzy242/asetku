import AssetsApi from './Assets.api';
import BaseData from '../../core/services/BaseData';
import ConfirmDialog from '../../components/molecules/ConfirmDialog/ConfirmDialog';

class AssetsData extends BaseData {
  constructor() {
    super(AssetsApi);
  }

  async bulkActivate(ids, activate) {
    const confirmed = await ConfirmDialog.show({
      title: activate ? 'Activate Assets' : 'Deactivate Assets',
      text: `Are you sure you want to ${activate ? 'activate' : 'deactivate'} ${ids.length} asset(s)?`,
      icon: 'warning',
      confirmButtonText: activate ? 'Yes, Activate' : 'Yes, Deactivate',
    });
    if (!confirmed) return { success: false, cancelled: true };

    try {
      const result = await this.api.bulkActivate(ids, activate);
      if (result.isSuccess) {
        ConfirmDialog.toast.success(`${result.data} asset(s) ${activate ? 'activated' : 'deactivated'} successfully`);
        return { success: true };
      }
      ConfirmDialog.toast.error(result.message || 'Failed');
      return { success: false };
    } catch {
      ConfirmDialog.toast.error('Failed to process');
      return { success: false };
    }
  }

  async importAssets(file) {
    try {
      const result = await this.api.import(file);
      if (result.isSuccess && result.data) {
        const importResult = result.data;
        if (importResult.errorCount > 0) {
          ConfirmDialog.toast.warning(`Import completed: ${importResult.successCount} success, ${importResult.errorCount} errors`);
        } else {
          ConfirmDialog.toast.success(`Import completed: ${importResult.successCount} assets imported successfully`);
        }
        return { success: true, data: importResult };
      }
      ConfirmDialog.toast.error(result.message || 'Import failed');
      return { success: false };
    } catch {
      ConfirmDialog.toast.error('Failed to import');
      return { success: false };
    }
  }

  async downloadTemplate() {
    try {
      const blob = await this.api.downloadImportTemplate();
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', 'Asset_Import_Template.xlsx');
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(url);
      ConfirmDialog.toast.success('Template downloaded');
      return { success: true };
    } catch {
      ConfirmDialog.toast.error('Failed to download template');
      return { success: false };
    }
  }

  getCreateMessage() { return 'Asset created successfully'; }
  getUpdateMessage() { return 'Asset updated successfully'; }
  getDeleteMessage() { return 'Asset deleted successfully'; }
  getDeleteTitle() { return 'Delete Asset'; }
  getDeleteText() { return 'Are you sure you want to delete this asset? This action cannot be undone.'; }
}

export default AssetsData;