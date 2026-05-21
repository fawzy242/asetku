import React, { useState, useCallback } from "react";
import { FiUpload, FiDownload, FiX, FiCheckCircle, FiAlertCircle, FiFileText } from "react-icons/fi";
import { Box, LinearProgress, Typography, Chip } from "@mui/material";
import Modal from "../Modal/Modal";
import Button from "../../atoms/Button/Button";
import "./ImportModal.scss";

const ImportModal = ({ 
  isOpen, 
  onClose, 
  onImport, 
  onDownloadTemplate, 
  isImporting = false, 
  importResult = null,
  title = "Import Data",
  description = "Upload Excel or TXT file with data. Data will be imported as INACTIVE and need activation."
}) => {
  const [selectedFile, setSelectedFile] = useState(null);
  const [dragActive, setDragActive] = useState(false);

  const handleDrag = useCallback((e) => {
    e.preventDefault();
    e.stopPropagation();
    if (e.type === "dragenter" || e.type === "dragover") {
      setDragActive(true);
    } else if (e.type === "dragleave") {
      setDragActive(false);
    }
  }, []);

  const handleDrop = useCallback((e) => {
    e.preventDefault();
    e.stopPropagation();
    setDragActive(false);
    
    if (e.dataTransfer.files && e.dataTransfer.files[0]) {
      setSelectedFile(e.dataTransfer.files[0]);
    }
  }, []);

  const handleFileChange = useCallback((e) => {
    if (e.target.files && e.target.files[0]) {
      setSelectedFile(e.target.files[0]);
    }
  }, []);

  const handleImport = useCallback(async () => {
    if (!selectedFile) return;
    await onImport(selectedFile);
    setSelectedFile(null);
  }, [selectedFile, onImport]);

  const handleClose = useCallback(() => {
    setSelectedFile(null);
    setDragActive(false);
    onClose();
  }, [onClose]);

  const handleDownload = useCallback(async () => {
    await onDownloadTemplate();
  }, [onDownloadTemplate]);

  const formatFileSize = (bytes) => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  const isSuccess = importResult?.errorCount === 0 && importResult?.successCount > 0;
  const hasErrors = importResult?.errorCount > 0;
  const hasWarnings = importResult?.warnings?.length > 0;

  return (
    <Modal isOpen={isOpen} onClose={handleClose} title={title} size="lg">
      <div className="import-modal">
        <p className="import-modal__description">{description}</p>

        {!importResult ? (
          <>
            <div 
              className={`import-modal__dropzone ${dragActive ? 'import-modal__dropzone--active' : ''}`}
              onDragEnter={handleDrag}
              onDragLeave={handleDrag}
              onDragOver={handleDrag}
              onDrop={handleDrop}
            >
              <FiUpload size={40} className="import-modal__dropzone-icon" />
              <p className="import-modal__dropzone-text">
                Drag & drop your file here or <span className="import-modal__dropzone-link">browse</span>
              </p>
              <p className="import-modal__dropzone-hint">
                Supports .xlsx, .xls, .csv, .txt (max 10MB)
              </p>
              <input
                type="file"
                id="import-file-input"
                className="import-modal__file-input"
                onChange={handleFileChange}
                accept=".xlsx,.xls,.csv,.txt"
              />
              <label htmlFor="import-file-input" className="import-modal__file-label">
                Select File
              </label>
            </div>

            {selectedFile && (
              <div className="import-modal__selected-file">
                <FiFileText size={20} />
                <span className="import-modal__selected-file-name">{selectedFile.name}</span>
                <span className="import-modal__selected-file-size">
                  {formatFileSize(selectedFile.size)}
                </span>
                <button 
                  className="import-modal__selected-file-remove"
                  onClick={() => setSelectedFile(null)}
                >
                  <FiX size={16} />
                </button>
              </div>
            )}

            <div className="import-modal__actions">
              <Button 
                variant="outline" 
                onClick={handleDownload} 
                startIcon={<FiDownload />}
              >
                Download Template
              </Button>
              <Button 
                variant="primary" 
                onClick={handleImport} 
                loading={isImporting}
                disabled={!selectedFile}
                startIcon={<FiUpload />}
              >
                Import
              </Button>
            </div>
          </>
        ) : (
          <div className="import-modal__result">
            <div className={`import-modal__result-header import-modal__result-header--${isSuccess ? 'success' : hasErrors ? 'error' : 'warning'}`}>
              {isSuccess && <FiCheckCircle size={32} />}
              {hasErrors && <FiAlertCircle size={32} />}
              {!isSuccess && !hasErrors && <FiAlertCircle size={32} />}
              <div>
                <h3 className="import-modal__result-title">
                  {isSuccess ? 'Import Successful' : hasErrors ? 'Import Completed with Errors' : 'Import Completed'}
                </h3>
                <p className="import-modal__result-summary">
                  {importResult.successCount} successful, {importResult.errorCount} errors, {importResult.totalRows} total rows
                </p>
              </div>
            </div>

            {hasWarnings && importResult.warnings?.length > 0 && (
              <div className="import-modal__warnings">
                <h4 className="import-modal__warnings-title">Warnings ({importResult.warnings.length})</h4>
                <div className="import-modal__warnings-list">
                  {importResult.warnings.slice(0, 10).map((warning, idx) => (
                    <div key={idx} className="import-modal__warning">
                      <span className="import-modal__warning-row">Row {warning.rowNumber}</span>
                      <span className="import-modal__warning-message">{warning.message}</span>
                    </div>
                  ))}
                  {importResult.warnings.length > 10 && (
                    <div className="import-modal__warning-more">
                      +{importResult.warnings.length - 10} more warnings
                    </div>
                  )}
                </div>
              </div>
            )}

            {importResult.errors?.length > 0 && (
              <div className="import-modal__errors">
                <h4 className="import-modal__errors-title">Errors ({importResult.errors.length})</h4>
                <div className="import-modal__errors-list">
                  {importResult.errors.slice(0, 10).map((error, idx) => (
                    <div key={idx} className="import-modal__error">
                      <span className="import-modal__error-row">Row {error.rowNumber}</span>
                      <span className="import-modal__error-column">{error.column}</span>
                      <span className="import-modal__error-message">{error.message}</span>
                    </div>
                  ))}
                  {importResult.errors.length > 10 && (
                    <div className="import-modal__error-more">
                      +{importResult.errors.length - 10} more errors
                    </div>
                  )}
                </div>
              </div>
            )}

            <div className="import-modal__result-actions">
              <Button variant="primary" onClick={handleClose}>
                Close
              </Button>
            </div>
          </div>
        )}
      </div>
    </Modal>
  );
};

export default ImportModal;