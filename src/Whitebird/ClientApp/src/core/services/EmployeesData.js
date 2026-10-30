import { employeeApi } from '../api/api';
import BaseData from './BaseData';
import { fileToBase64 } from '../utils/file.utils';

class EmployeesData extends BaseData {
  constructor() {
    super(employeeApi);
    this.setEntityName('employee');
  }

  getTemplateFileName() {
    return 'Employee_Import_Template.xlsx';
  }

  async create(data) {
    const submitData = await this._buildSubmitDataWithFiles(data, {
      isUpdate: false,
    });
    return super.create(submitData);
  }

  async update(id, data) {
    const submitData = await this._buildSubmitDataWithFiles(data, {
      isUpdate: true,
    });
    return super.update(id, submitData);
  }

  async importEmployees(file) {
    try {
      const result = await this.api.import(file);
      if (result.data?.isSuccess !== undefined) {
        if (!result.data.isSuccess) {
          return {
            success: false,
            error:
              result.data.errors?.join('\n') ||
              result.data.message ||
              'Import failed',
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
        error: error.userMessage || error.message || 'Failed to import',
      };
    }
  }

  // ============================================================
  // PRIVATE HELPERS
  // ============================================================

  async _buildSubmitDataWithFiles(data, { isUpdate = false } = {}) {
    const submitData = this.buildSubmitData(data);

    if (isUpdate) {
      submitData.isActive =
        data.isActive === true || data.isActive === 'true' || data.isActive === 1;
    }

    if (data.photoFile instanceof File && data.photoFile.size > 0) {
      try {
        submitData.photoFileBase64 = await fileToBase64(data.photoFile);
        submitData.photoFileName = data.photoFile.name;
      } catch (err) {
        console.error('Failed to convert photo to base64:', err);
      }
    }

    const attachFiles = Array.isArray(data.attachmentFiles)
      ? data.attachmentFiles
      : [];

    if (attachFiles.length > 0) {
      submitData.attachmentFilesBase64 = [];
      submitData.attachmentFileNames = [];
      if (isUpdate) {
        submitData.replaceAttachments =
          data.replaceAttachments !== undefined ? data.replaceAttachments : true;
      }
      for (const item of attachFiles) {
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
    } else if (isUpdate) {
      submitData.attachmentFilesBase64 = [];
      submitData.attachmentFileNames = [];
      submitData.replaceAttachments =
        data.replaceAttachments !== undefined ? data.replaceAttachments : false;
    }

    if (
      isUpdate &&
      Array.isArray(data.attachmentIdsToKeep) &&
      data.attachmentIdsToKeep.length > 0
    ) {
      submitData.attachmentIdsToKeep = data.attachmentIdsToKeep;
    }

    return submitData;
  }

  buildSubmitData(data) {
    return {
      employeeCode: data.employeeCode || '',
      fullName: data.fullName || '',
      address: data.address || null,
      departmentId: data.departmentId ? parseInt(data.departmentId, 10) : null,
      position: data.position ? parseInt(data.position, 10) : null,
      employmentStatus: data.employmentStatus
        ? parseInt(data.employmentStatus, 10)
        : null,
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
}

export default EmployeesData;