// src/pages/Assets/Assets.data.js
import apiClient from '../../core/api/apiClient';
import BaseData from '../../core/services/BaseData';
import ConfirmDialog from '../../components/molecules/ConfirmDialog/ConfirmDialog';
import { handleSuccess, handleApiError } from '../../core/services';

class AssetsData extends BaseData {
  constructor() {
    super(apiClient.asset);
    this.setEntityName('asset');
  }

  /**
   * Bulk activate or deactivate assets
   */
  async bulkActivate(ids, activate) {
    const actionText = activate ? 'activate' : 'deactivate';
    
    try {
      const endpoint = activate ? this.api.activate : this.api.deactivate;
      const result = await endpoint({ ids });
      
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          const errorMsg = result.data.errors?.join('\n') || result.data.message || `Failed to ${actionText}`;
          ConfirmDialog.toast.error(errorMsg);
          return { success: false };
        }
        
        const count = result.data.data || ids.length;
        handleSuccess({
          operation: activate ? 'activate' : 'deactivate',
          entityName: 'asset',
          message: `${count} asset(s) ${actionText}d successfully`,
          result: result.data,
        });
        
        return { success: true, data: count };
      }
      
      ConfirmDialog.toast.success(`${ids.length} asset(s) ${actionText}d successfully`);
      return { success: true };
    } catch (error) {
      return {
        success: false,
        error: error.userMessage || error.message || `Failed to ${actionText}`,
      };
    }
  }

  /**
   * Import assets from Excel file
   */
  async importAssets(file) {
    try {
      const result = await this.api.import(file);
      
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          const errorMsg = result.data.errors?.join('\n') || result.data.message || 'Import failed';
          ConfirmDialog.toast.error(errorMsg);
          return { success: false };
        }
        
        const importResult = result.data.data;
        if (importResult?.errorCount > 0) {
          ConfirmDialog.toast.warning(
            `Import completed: ${importResult.successCount} success, ${importResult.errorCount} errors`
          );
        } else {
          handleSuccess({
            operation: 'import',
            entityName: 'asset',
            message: `Import completed: ${importResult?.successCount || 0} assets imported successfully`,
            result: result.data,
          });
        }
        return { success: true, data: importResult };
      }
      
      ConfirmDialog.toast.success('Assets imported successfully');
      return { success: true };
    } catch (error) {
      return {
        success: false,
        error: error.userMessage || error.message || 'Failed to import',
      };
    }
  }

  /**
   * Download import template
   */
  async downloadTemplate() {
    try {
      const response = await this.api.getImportTemplate();
      const blob = response?.data;
      
      if (!blob || !(blob instanceof Blob) || blob.size === 0) {
        ConfirmDialog.toast.error('Template file is empty. Please try again.');
        return { success: false };
      }
      
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', 'Asset_Import_Template.xlsx');
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(url);
      
      ConfirmDialog.toast.success('Template downloaded successfully');
      return { success: true };
    } catch (error) {
      if (this.isCanceledError(error)) {
        return { success: false, error: 'canceled', isCanceled: true };
      }
      
      if (error.response?.data instanceof Blob) {
        try {
          const text = await error.response.data.text();
          const parsed = JSON.parse(text);
          if (parsed.message) {
            ConfirmDialog.toast.error(parsed.message);
            return { success: false, error: parsed.message };
          }
        } catch {
          // Not JSON, continue
        }
      }
      
      const errorMsg = error.userMessage || error.message || 'Failed to download template';
      ConfirmDialog.toast.error(errorMsg);
      return { success: false, error: errorMsg };
    }
  }

  /**
   * Helper: convert file to base64
   */
  fileToBase64(file) {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = () => {
        const base64 = reader.result.split(',')[1];
        resolve(base64);
      };
      reader.onerror = reject;
      reader.readAsDataURL(file);
    });
  }

  /**
   * UPDATE — kirim file sebagai base64 dalam JSON
   */
  async update(id, data) {
    try {
      // Base data
      const submitData = {
        assetCode: data.assetCode || '',
        assetName: data.assetName || '',
        categoryId: data.categoryId ? parseInt(data.categoryId, 10) : 0,
        brand: data.brand || null,
        model: data.model || null,
        serialNumber: data.serialNumber || null,
        imei: data.imei || null,
        macAddress: data.macAddress || null,
        hostname: data.hostname || null,
        ipAddress: data.ipAddress || null,
        purchaseDate: data.purchaseDate || null,
        purchasePrice: data.purchasePrice ? parseFloat(data.purchasePrice) : null,
        invoiceNumber: data.invoiceNumber || null,
        supplierId: data.supplierId ? parseInt(data.supplierId, 10) : null,
        warrantyPeriod: data.warrantyPeriod ? parseInt(data.warrantyPeriod, 10) : null,
        assetConditionPurchase: data.assetConditionPurchase ? parseInt(data.assetConditionPurchase, 10) : null,
        notes: data.notes || null,
        isActive: data.isActive === true || data.isActive === 'true' || data.isActive === 1,
      };

      // Konversi photoFile ke base64
      if (data.photoFile instanceof File && data.photoFile.size > 0) {
        try {
          const base64 = await this.fileToBase64(data.photoFile);
          submitData.photoFileBase64 = base64;
          submitData.photoFileName = data.photoFile.name;
          console.log('📸 Photo converted to base64, size:', base64.length);
        } catch (err) {
          console.error('Failed to convert photo to base64:', err);
        }
      }

      // Konversi attachmentFiles ke base64
      if (data.attachmentFiles && data.attachmentFiles.length > 0) {
        submitData.attachmentFilesBase64 = [];
        submitData.attachmentFileNames = [];
        for (const file of data.attachmentFiles) {
          if (file instanceof File && file.size > 0) {
            try {
              const base64 = await this.fileToBase64(file);
              submitData.attachmentFilesBase64.push(base64);
              submitData.attachmentFileNames.push(file.name);
              console.log('📎 Attachment converted to base64:', file.name, 'size:', base64.length);
            } catch (err) {
              console.error('Failed to convert attachment to base64:', err);
            }
          }
        }
      }

      // Attachment IDs to keep
      if (data.attachmentIdsToKeep && data.attachmentIdsToKeep.length > 0) {
        submitData.attachmentIdsToKeep = data.attachmentIdsToKeep;
      }

      // Replace attachments flag
      if (data.replaceAttachments !== undefined) {
        submitData.replaceAttachments = data.replaceAttachments;
      }

      console.log('📤 Submitting asset update with base64 files');

      // Kirim sebagai JSON biasa
      const result = await this.api.update(id, submitData);

      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          const errorMsg = result.data.errors?.join('\n') || result.data.message || 'Failed to update';
          ConfirmDialog.toast.error(errorMsg);
          return { success: false, errors: result.data.errors };
        }
        return { success: true, data: result.data.data };
      }
      return { success: true, data: result.data };
    } catch (error) {
      if (this.isCanceledError(error)) {
        return { success: false, error: 'canceled', isCanceled: true };
      }
      const errorMsg = error.userMessage || error.message || 'Failed to update';
      ConfirmDialog.toast.error(errorMsg);
      return { success: false, error: errorMsg };
    }
  }

  getCreateMessage() { return 'Asset created successfully'; }
  getUpdateMessage() { return 'Asset updated successfully'; }
  getDeleteMessage() { return 'Asset deleted successfully'; }
  getDeleteTitle() { return 'Delete Asset'; }
  getDeleteText() { return 'Are you sure you want to delete this asset? This action cannot be undone.'; }
}

export default AssetsData;