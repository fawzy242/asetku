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
        await ConfirmDialog.showSuccess('Success', `${result.data} asset(s) ${activate ? 'activated' : 'deactivated'} successfully`);
        return { success: true };
      }
      await ConfirmDialog.showError('Failed', result.message || 'Failed');
      return { success: false };
    } catch (error) {
      console.error('bulkActivate error:', error);
      await ConfirmDialog.showError('Failed', 'Failed to process');
      return { success: false };
    }
  }

  async importAssets(file) {
    try {
      const result = await this.api.import(file);
      if (result.isSuccess && result.data) {
        const importResult = result.data;
        if (importResult.errorCount > 0) {
          await ConfirmDialog.showWarning('Import Completed', `${importResult.successCount} success, ${importResult.errorCount} errors`);
        } else {
          await ConfirmDialog.showSuccess('Import Completed', `${importResult.successCount} assets imported successfully`);
        }
        return { success: true, data: importResult };
      }
      await ConfirmDialog.showError('Failed', result.message || 'Import failed');
      return { success: false };
    } catch (error) {
      console.error('importAssets error:', error);
      await ConfirmDialog.showError('Failed', 'Failed to import');
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
      await ConfirmDialog.showSuccess('Success', 'Template downloaded successfully');
      return { success: true };
    } catch (error) {
      console.error('downloadTemplate error:', error);
      await ConfirmDialog.showError('Failed', 'Failed to download template');
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