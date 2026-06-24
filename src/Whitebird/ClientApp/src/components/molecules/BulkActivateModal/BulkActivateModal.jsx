import React, { useState } from "react";
import { FiCheckSquare, FiAlertTriangle } from "react-icons/fi";
import { Box, Typography, Chip } from "@mui/material";
import Modal from "../Modal/Modal";
import Button from "../../atoms/Button/Button";
import "./BulkActivateModal.scss";

const BulkActivateModal = ({
  isOpen,
  onClose,
  onConfirm,
  selectedIds = [],
  itemName = "items",
  title = "Bulk Activation",
  description = "This action will change the status of the selected items."
}) => {
  const [isActivating, setIsActivating] = useState(false);
  const [activate, setActivate] = useState(true);

  const handleConfirm = async () => {
    if (selectedIds.length === 0) return;
    setIsActivating(true);
    await onConfirm(selectedIds, activate);
    setIsActivating(false);
    onClose();
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title={title} size="md">
      <div className="bulk-activate-modal">
        <div className="bulk-activate-modal__icon">
          <FiAlertTriangle size={48} />
        </div>

        <Typography variant="h6" className="bulk-activate-modal__title">
          {activate ? "Activate" : "Deactivate"} {selectedIds.length} {itemName}?
        </Typography>

        <Typography variant="body2" className="bulk-activate-modal__description">
          {description}
        </Typography>

        <div className="bulk-activate-modal__selected">
          <div className="bulk-activate-modal__selected-header">
            <FiCheckSquare size={16} />
            <span>Selected ({selectedIds.length})</span>
          </div>
          <div className="bulk-activate-modal__selected-list">
            {selectedIds.slice(0, 10).map((id, idx) => (
              <Chip
                key={idx}
                label={`ID: ${id}`}
                size="small"
                variant="outlined"
                className="bulk-activate-modal__selected-chip"
              />
            ))}
            {selectedIds.length > 10 && (
              <Chip
                label={`+${selectedIds.length - 10} more`}
                size="small"
                variant="outlined"
                className="bulk-activate-modal__selected-chip"
              />
            )}
          </div>
        </div>

        <div className="bulk-activate-modal__actions">
          <Button variant="outline" onClick={onClose} disabled={isActivating}>
            Cancel
          </Button>
          <Button
            variant={activate ? "primary" : "danger"}
            onClick={handleConfirm}
            loading={isActivating}
          >
            {activate ? "Yes, Activate" : "Yes, Deactivate"}
          </Button>
        </div>
      </div>
    </Modal>
  );
};

export default BulkActivateModal;