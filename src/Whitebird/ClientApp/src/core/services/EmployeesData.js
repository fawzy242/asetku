import { employeeApi } from '../api/employee.api';
import BaseData from './BaseData';
import { fileToBase64 } from '../utils/file.utils';

class EmployeesData extends BaseData {
  constructor() {
    super(employeeApi);
    this.setEntityName('employee');
  }

  // ============================================================
  // CREATE
  // ============================================================
  async create(data) {
    try {
      const submitData = this.buildSubmitData(data);

      if (data.photoFile instanceof File && data.photoFile.size > 0) {
        try {
          submitData.photoFileBase64 = await fileToBase64(data.photoFile);
          submitData.photoFileName = data.photoFile.name;
        } catch (err) {
          console.error('Failed to convert photo to base64:', err);
        }
      }

      if (data.attachmentFiles && data.attachmentFiles.length > 0) {
        submitData.attachmentFilesBase64 = [];
        submitData.attachmentFileNames = [];
        for (const item of data.attachmentFiles) {
          const file = this.extractFile(item);
          if (file && file.size > 0) {
            try {
              const base64 = await fileToBase64(file);
              submitData.attachmentFilesBase64.push(base64);
              submitData.attachmentFileNames.push(file.name || 'file');
            } catch (err) {
              console.error('Failed to convert attachment to base64:', err);
            }
          }
        }
      }

      const result = await this.api.create(submitData);
      return this.handleApiResponse(result);
    } catch (error) {
      return this.handleError(error, 'Failed to create');
    }
  }

  // ============================================================
  // UPDATE
  // ============================================================
  async update(id, data) {
    try {
      const submitData = this.buildSubmitData(data);
      submitData.isActive = data.isActive === true || data.isActive === 'true' || data.isActive === 1;

      if (data.photoFile instanceof File && data.photoFile.size > 0) {
        try {
          submitData.photoFileBase64 = await fileToBase64(data.photoFile);
          submitData.photoFileName = data.photoFile.name;
        } catch (err) {
          console.error('Failed to convert photo to base64:', err);
        }
      }

      if (data.attachmentFiles && data.attachmentFiles.length > 0) {
        submitData.attachmentFilesBase64 = [];
        submitData.attachmentFileNames = [];
        submitData.replaceAttachments = data.replaceAttachments !== undefined ? data.replaceAttachments : true;
        for (const item of data.attachmentFiles) {
          const file = this.extractFile(item);
          if (file && file.size > 0) {
            try {
              const base64 = await fileToBase64(file);
              submitData.attachmentFilesBase64.push(base64);
              submitData.attachmentFileNames.push(file.name || 'file');
            } catch (err) {
              console.error('Failed to convert attachment to base64:', err);
            }
          }
        }
      } else {
        submitData.attachmentFilesBase64 = [];
        submitData.attachmentFileNames = [];
        submitData.replaceAttachments = data.replaceAttachments !== undefined ? data.replaceAttachments : false;
      }

      if (data.attachmentIdsToKeep && data.attachmentIdsToKeep.length > 0) {
        submitData.attachmentIdsToKeep = data.attachmentIdsToKeep;
      }

      const result = await this.api.update(id, submitData);
      return this.handleApiResponse(result);
    } catch (error) {
      return this.handleError(error, 'Failed to update');
    }
  }

  // ============================================================
  // BULK ACTIVATE
  // ============================================================
  async bulkActivate(ids, activate) {
    return super.bulkActivate(ids, activate);
  }

  // ============================================================
  // IMPORT
  // ============================================================
  async importEmployees(file) {
    try {
      const result = await this.api.import(file);
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          return { 
            success: false, 
            error: result.data.errors?.join('\n') || result.data.message || 'Import failed',
            errorType: result.data.errorType,
          };
        }
        return { success: true, data: result.data.data };
      }
      return { success: true };
    } catch (error) {
      if (this.isCanceledError(error)) {
        return { success: false, error: 'canceled', isCanceled: true };
      }
      return { 
        success: false, 
        error: error.userMessage || error.message || 'Failed to import' 
      };
    }
  }

  // ============================================================
  // DOWNLOAD TEMPLATE
  // ============================================================
  async downloadTemplate() {
    return this.downloadTemplate('Employee_Import_Template.xlsx');
  }

  // ============================================================
  // PRIVATE HELPERS
  // ============================================================

  buildSubmitData(data) {
    return {
      employeeCode: data.employeeCode || '',
      fullName: data.fullName || '',
      address: data.address || null,
      departmentId: data.departmentId ? parseInt(data.departmentId, 10) : null,
      position: data.position ? parseInt(data.position, 10) : null,
      employmentStatus: data.employmentStatus ? parseInt(data.employmentStatus, 10) : null,
      phoneNumber: data.phoneNumber || null,
      email: data.email || null,
      officeId: data.officeId ? parseInt(data.officeId, 10) : null,
      joinDate: data.joinDate || null,
      resignDate: data.resignDate || null,
    };
  }

  extractFile(item) {
    if (item instanceof File) return item;
    if (item && item.file instanceof File) return item.file;
    if (item && item.isNew && item.file) return item.file;
    return null;
  }

  handleApiResponse(result) {
    if (result.data?.isSuccess !== undefined) {
      if (!result.data.isSuccess) {
        return { 
          success: false, 
          errors: result.data.errors || [result.data.message || 'Operation failed'],
          errorType: result.data.errorType,
        };
      }
      return { success: true, data: result.data.data };
    }
    return { success: true, data: result.data };
  }

  handleError(error, defaultMessage) {
    if (this.isCanceledError(error)) {
      return { success: false, error: 'canceled', isCanceled: true };
    }
    return { 
      success: false, 
      error: error.userMessage || error.message || defaultMessage 
    };
  }
}

export default EmployeesData;