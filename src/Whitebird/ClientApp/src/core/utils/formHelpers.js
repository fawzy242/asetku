/**
 * Centralized form data transformation utilities
 * Untuk menghilangkan duplikasi transformFormData di berbagai pages
 */

/**
 * Clean nullable string fields (convert empty string to null)
 * @param {Object} data - Form data object
 * @param {string[]} fields - Array of field names to clean
 * @returns {Object} Cleaned data
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
 * Clean nullable ID fields (convert empty string to null, parse string to int)
 * @param {Object} data - Form data object
 * @param {string[]} fields - Array of field names to clean
 * @returns {Object} Cleaned data
 */
export const cleanIdFields = (data, fields = []) => {
  const result = { ...data };
  fields.forEach(field => {
    if (result[field] === '' || result[field] === null || result[field] === undefined) {
      result[field] = null;
    } else if (typeof result[field] === 'string') {
      const parsed = parseInt(result[field], 10);
      result[field] = isNaN(parsed) ? null : parsed;
    }
  });
  return result;
};

/**
 * Clean nullable number fields (convert empty string to null, parse string to float)
 * @param {Object} data - Form data object
 * @param {string[]} fields - Array of field names to clean
 * @returns {Object} Cleaned data
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
 * Clean nullable date fields (convert empty string to null)
 * @param {Object} data - Form data object
 * @param {string[]} fields - Array of field names to clean
 * @returns {Object} Cleaned data
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

/**
 * Clean boolean field (convert string 'true'/'false' to boolean)
 * @param {Object} data - Form data object
 * @param {string} field - Field name
 * @returns {Object} Cleaned data
 */
export const cleanBooleanField = (data, field) => {
  const result = { ...data };
  if (result[field] === 'true' || result[field] === true) {
    result[field] = true;
  } else if (result[field] === 'false' || result[field] === false) {
    result[field] = false;
  } else {
    result[field] = null;
  }
  return result;
};

/**
 * Comprehensive form cleaner for Assets
 * @param {Object} data - Asset form data
 * @returns {Object} Cleaned asset data
 */
export const cleanAssetFormData = (data) => {
  let result = cleanNullableStrings(data, [
    'brand', 'model', 'serialNumber', 'imei', 'macAddress',
    'hostname', 'ipAddress', 'invoiceNumber', 'notes'
  ]);
  
  result = cleanIdFields(result, [
    'categoryId', 'supplierId', 'officeId', 'warrantyPeriod', 
    'usefulLife', 'assetCondition', 'assetConditionPurchase'
  ]);
  
  result = cleanNumberFields(result, ['purchasePrice', 'residualValue']);
  result = cleanBooleanField(result, 'operasionalOffice');
  
  return result;
};

/**
 * Comprehensive form cleaner for Employees
 * @param {Object} data - Employee form data
 * @returns {Object} Cleaned employee data
 */
export const cleanEmployeeFormData = (data) => {
  let result = cleanNullableStrings(data, ['address', 'phoneNumber', 'email']);
  result = cleanIdFields(result, ['departmentId', 'position', 'employmentStatus', 'officeId']);
  result = cleanDateFields(result, ['joinDate', 'resignDate']);
  return result;
};

/**
 * Comprehensive form cleaner for Transactions
 * @param {Object} data - Transaction form data
 * @returns {Object} Cleaned transaction data
 */
export const cleanTransactionFormData = (data) => {
  let result = cleanNullableStrings(data, ['expectedReturnDate', 'notes', 'conditionBefore', 'maintenanceType']);
  result = cleanIdFields(result, ['fromEmployeeId', 'toEmployeeId', 'toLocationId', 'fromAssetTransactionId']);
  result = cleanNumberFields(result, ['maintenanceCost']);
  
  // Ensure assetId is number
  if (result.assetId === '' || result.assetId === null || result.assetId === undefined) {
    result.assetId = 0;
  } else if (typeof result.assetId === 'string') {
    result.assetId = parseInt(result.assetId, 10);
  }
  
  // Set default transaction date
  if (!result.transactionDate) {
    result.transactionDate = new Date().toISOString();
  }
  
  return result;
};

/**
 * Comprehensive form cleaner for Offices
 * @param {Object} data - Office form data
 * @returns {Object} Cleaned office data
 */
export const cleanOfficeFormData = (data) => {
  let result = cleanNullableStrings(data, ['officeCode', 'city', 'address', 'phone']);
  result = cleanIdFields(result, ['officeType', 'parentOfficeId']);
  return result;
};

/**
 * Comprehensive form cleaner for Departments
 * @param {Object} data - Department form data
 * @returns {Object} Cleaned department data
 */
export const cleanDepartmentFormData = (data) => {
  return cleanNullableStrings(data, ['departmentCode', 'description']);
};

/**
 * Comprehensive form cleaner for Categories
 * @param {Object} data - Category form data
 * @returns {Object} Cleaned category data
 */
export const cleanCategoryFormData = (data) => {
  let result = cleanNullableStrings(data, ['categoryCode', 'description']);
  result = cleanIdFields(result, ['parentCategoryId']);
  return result;
};

/**
 * Comprehensive form cleaner for Suppliers
 * @param {Object} data - Supplier form data
 * @returns {Object} Cleaned supplier data
 */
export const cleanSupplierFormData = (data) => {
  return cleanNullableStrings(data, ['contactPerson', 'phoneNumber', 'email', 'address']);
};

export default {
  cleanNullableStrings,
  cleanIdFields,
  cleanNumberFields,
  cleanDateFields,
  cleanBooleanField,
  cleanAssetFormData,
  cleanEmployeeFormData,
  cleanTransactionFormData,
  cleanOfficeFormData,
  cleanDepartmentFormData,
  cleanCategoryFormData,
  cleanSupplierFormData,
};