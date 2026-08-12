// src/core/utils/formHelpers.js

/**
 * Clean nullable string fields (convert empty string to null)
 */
export const cleanNullableStrings = (data, fields = []) => {
  const result = { ...data };
  fields.forEach(field => {
    if (result[field] === '' || result[field] === undefined) {
      result[field] = null;
    }
  });
  return result;
};

/**
 * Clean ID fields (convert to number or null)
 */
export const cleanIdFields = (data, fields = []) => {
  const result = { ...data };
  fields.forEach(field => {
    if (result[field] === '' || result[field] === undefined || result[field] === null) {
      result[field] = null;
    } else if (typeof result[field] === 'string') {
      const parsed = parseInt(result[field], 10);
      result[field] = isNaN(parsed) ? null : parsed;
    } else if (typeof result[field] !== 'number') {
      result[field] = null;
    }
  });
  return result;
};

/**
 * Clean number fields (convert to number or null)
 */
export const cleanNumberFields = (data, fields = []) => {
  const result = { ...data };
  fields.forEach(field => {
    if (result[field] === '' || result[field] === null || result[field] === undefined) {
      result[field] = null;
    } else if (typeof result[field] === 'string') {
      const parsed = parseFloat(result[field]);
      result[field] = isNaN(parsed) ? null : parsed;
    }
  });
  return result;
};

/**
 * Clean date fields (convert empty string to null)
 */
export const cleanDateFields = (data, fields = []) => {
  const result = { ...data };
  fields.forEach(field => {
    if (result[field] === '' || result[field] === undefined) {
      result[field] = null;
    }
  });
  return result;
};

// ============================================================
// ASSET
// ============================================================
export const cleanAssetFormData = (data) => {
  let result = cleanNullableStrings(data, [
    'brand', 'model', 'serialNumber', 'imei', 'macAddress',
    'hostname', 'ipAddress', 'invoiceNumber', 'notes'
  ]);
  result = cleanIdFields(result, ['categoryId', 'supplierId', 'warrantyPeriod']);
  result = cleanNumberFields(result, ['purchasePrice']);
  // REMOVED: warrantyExpiryDate (response-only, not for create/update)
  // REMOVED: operasionalOffice, officeId, residualValue, usefulLife, assetCondition
  return result;
};

// ============================================================
// EMPLOYEE
// ============================================================
export const cleanEmployeeFormData = (data) => {
  let result = cleanNullableStrings(data, ['address', 'phoneNumber', 'email']);
  result = cleanIdFields(result, ['departmentId', 'position', 'employmentStatus', 'officeId']);
  result = cleanDateFields(result, ['joinDate', 'resignDate']);
  return result;
};

// ============================================================
// TRANSACTION
// ============================================================
export const cleanTransactionFormData = (data) => {
  let result = cleanNullableStrings(data, ['expectedLoanReturnDate', 'notes']);
  result = cleanIdFields(result, ['employeeId', 'officeId', 'fromAssetTransactionId']);
  result = cleanNumberFields(result, ['maintenanceCost']);

  // Ensure assetId is number
  if (result.assetId === '' || result.assetId === null || result.assetId === undefined) {
    result.assetId = 0;
  } else if (typeof result.assetId === 'string') {
    result.assetId = parseInt(result.assetId, 10);
  }

  // Convert transactionDate to ISO string
  if (result.transactionDate) {
    if (typeof result.transactionDate === 'string' && result.transactionDate.length === 10) {
      result.transactionDate = `${result.transactionDate}T00:00:00`;
    }
  } else {
    result.transactionDate = new Date().toISOString();
  }

  return result;
};

// ============================================================
// TRANSACTION RETURN (Shortcut)
// ============================================================
export const cleanReturnFormData = (data) => {
  let result = cleanNullableStrings(data, ['notes']);
  // BACKEND: condition (not conditionAfter)
  result = cleanIdFields(result, ['condition']);
  return result;
};

// ============================================================
// POST MAINTENANCE (Shortcut)
// ============================================================
export const cleanPostMaintenanceFormData = (data) => {
  let result = cleanNullableStrings(data, ['notes']);
  // BACKEND: condition (not conditionAfter)
  result = cleanIdFields(result, ['condition']);
  result = cleanNumberFields(result, ['maintenanceCost']);
  return result;
};

// ============================================================
// OFFICE
// ============================================================
export const cleanOfficeFormData = (data) => {
  let result = cleanNullableStrings(data, ['officeCode', 'city', 'address', 'phone']);
  result = cleanIdFields(result, ['officeType', 'parentOfficeId']);
  return result;
};

// ============================================================
// DEPARTMENT
// ============================================================
export const cleanDepartmentFormData = (data) => {
  return cleanNullableStrings(data, ['departmentCode', 'description']);
};

// ============================================================
// CATEGORY
// ============================================================
export const cleanCategoryFormData = (data) => {
  let result = cleanNullableStrings(data, ['categoryCode', 'description']);
  result = cleanIdFields(result, ['parentCategoryId']);
  return result;
};

// ============================================================
// SUPPLIER
// ============================================================
export const cleanSupplierFormData = (data) => {
  return cleanNullableStrings(data, ['contactPerson', 'phoneNumber', 'email', 'address']);
};

// ============================================================
// EXPORT ALL
// ============================================================
export default {
  cleanNullableStrings,
  cleanIdFields,
  cleanNumberFields,
  cleanDateFields,
  cleanAssetFormData,
  cleanEmployeeFormData,
  cleanTransactionFormData,
  cleanReturnFormData,
  cleanPostMaintenanceFormData,
  cleanOfficeFormData,
  cleanDepartmentFormData,
  cleanCategoryFormData,
  cleanSupplierFormData,
};