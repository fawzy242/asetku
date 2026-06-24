import { useCallback } from 'react';
import { useSweetAlert } from './useSweetAlert';

/**
 * Standardized hook for bulk activate/deactivate operations
 * @param {Object} dataService - Data service instance with update method
 * @param {Object} options
 * @param {string} options.entityName - Entity name for display (e.g., 'asset', 'employee')
 * @param {Function} options.onSuccess - Callback after successful operation
 * @param {Function} options.onError - Callback after failed operation
 * @param {Function} options.transformData - Transform data before update (optional)
 * @param {string} options.idField - ID field name (default: auto-detect from service)
 */
export const useBulkActivate = (dataService, options = {}) => {
  const { 
    entityName = 'item', 
    onSuccess, 
    onError,
    transformData,
    idField
  } = options;
  
  const { toast, confirm } = useSweetAlert();

  const handleBulkActivate = useCallback(async (ids, activate) => {
    if (!ids || ids.length === 0) {
      toast.warning('No items selected');
      return { success: false };
    }

    const actionText = activate ? 'activate' : 'deactivate';
    const ActionText = activate ? 'Activate' : 'Deactivate';
    
    const confirmed = await confirm({
      title: `${ActionText} ${entityName.charAt(0).toUpperCase() + entityName.slice(1)}s`,
      text: `Are you sure you want to ${actionText} ${ids.length} ${entityName}(s)?`,
      confirmButtonText: activate ? 'Yes, Activate' : 'Yes, Deactivate',
    });
    
    if (!confirmed) return { success: false, cancelled: true };
    
    let successCount = 0;
    let failedIds = [];

    for (const id of ids) {
      try {
        const result = await dataService.fetchById(id);
        if (result.success && result.data) {
          const updateData = transformData 
            ? transformData({ ...result.data, isActive: activate })
            : { ...result.data, isActive: activate };
          
          const updateResult = await dataService.update(id, updateData);
          if (updateResult.success) {
            successCount++;
          } else {
            failedIds.push(id);
          }
        } else {
          failedIds.push(id);
        }
      } catch (error) {
        failedIds.push(id);
      }
    }
    
    if (successCount > 0) {
      toast.success(`${successCount} ${entityName}(s) ${actionText}d successfully`);
      if (onSuccess) onSuccess(successCount);
    }
    
    if (failedIds.length > 0 && failedIds.length === ids.length) {
      toast.error(`Failed to ${actionText} ${entityName}(s)`);
      if (onError) onError(failedIds);
      return { success: false, failedIds };
    }
    
    if (failedIds.length > 0) {
      toast.warning(`Partial success: ${successCount} ${actionText}d, ${failedIds.length} failed`);
      if (onError) onError(failedIds);
    }
    
    return { success: true, successCount, failedIds };
  }, [dataService, entityName, onSuccess, onError, transformData, toast, confirm]);

  return { handleBulkActivate };
};

export default useBulkActivate;