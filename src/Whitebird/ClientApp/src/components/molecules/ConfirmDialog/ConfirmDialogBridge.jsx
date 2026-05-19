import { useEffect } from 'react';
import { useConfirm } from './ConfirmDialog';
import ConfirmDialog from './ConfirmDialog';

/**
 * Bridge untuk menghubungkan ConfirmDialogProvider dengan static API ConfirmDialog.
 * Render sekali di root app.
 */
const ConfirmDialogBridge = () => {
  const { show } = useConfirm();

  useEffect(() => {
    // Override static method dengan context-aware implementation
    ConfirmDialog.show = (options = {}) => show(options);
    ConfirmDialog.close = () => {};
  }, [show]);

  return null;
};

export default ConfirmDialogBridge;