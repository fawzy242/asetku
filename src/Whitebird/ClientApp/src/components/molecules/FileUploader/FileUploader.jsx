import React, { useState, useCallback, useEffect, useRef } from 'react';
import {
  FiImage,
  FiPaperclip,
  FiTrash2,
} from 'react-icons/fi';
import {
  Box,
  Typography,
  Dialog,
  DialogContent,
} from '@mui/material';
import ConfirmDialog from '../ConfirmDialog/ConfirmDialog';
import Button from '../../atoms/Button/Button';
import FileDropzone from './FileDropzone';
import FileList from './FileList';
import FilePreview from './FilePreview';
import './FileUploader.scss';

const FileUploader = ({
  referenceType,
  referenceId,
  onUploadComplete,
  onFileDeleted,
  allowedTypes = ['.jpg', '.jpeg', '.png', '.gif', '.webp', '.pdf', '.doc', '.docx', '.xls', '.xlsx', '.txt'],
  maxSizeMB = 10,
  entityType = 'record',
  disableDelete = false,
  disablePreview = false,
  title = null,
  isAttachment = false,
  existingFiles = [],
  onFilesChange,
}) => {
  const [fileList, setFileList] = useState([]);
  const [isDragging, setIsDragging] = useState(false);
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [fileToDelete, setFileToDelete] = useState(null);
  const [previewFile, setPreviewFile] = useState(null);
  const [previewOpen, setPreviewOpen] = useState(false);
  const isMountedRef = useRef(true);
  const hasInitializedRef = useRef(false);
  const internalUpdateRef = useRef(false);

  // Initialize files from existingFiles prop
  useEffect(() => {
    if (!hasInitializedRef.current && existingFiles && Array.isArray(existingFiles) && existingFiles.length > 0) {
      setFileList(existingFiles);
      hasInitializedRef.current = true;
    }
  }, [existingFiles]);

  // Notify parent when fileList changes from internal updates
  useEffect(() => {
    if (internalUpdateRef.current) {
      if (onFilesChange) {
        onFilesChange(fileList);
      }
      internalUpdateRef.current = false;
    }
  }, [fileList, onFilesChange]);

  // ============================================================
  // FILE SELECT
  // ============================================================
  const handleFileSelect = useCallback(
    async (files) => {
      if (!files || files.length === 0) return;

      const validFiles = [];
      const invalidFiles = [];
      
      for (const file of files) {
        const ext = `.${file.name.split('.').pop()?.toLowerCase()}`;
        const isValidExt = allowedTypes.some(t => t.startsWith('.') ? t === ext : file.type === t);
        
        if (!isValidExt) {
          invalidFiles.push(`${file.name} (type not allowed)`);
          continue;
        }
        if (file.size > maxSizeMB * 1024 * 1024) {
          invalidFiles.push(`${file.name} (max ${maxSizeMB}MB)`);
          continue;
        }
        validFiles.push(file);
      }

      if (invalidFiles.length > 0) {
        ConfirmDialog.toast.error(`Invalid files: ${invalidFiles.join(', ')}`);
      }
      if (validFiles.length === 0) return;

      const fileNames = validFiles.map(f => f.name).join('\n');
      const confirmed = await ConfirmDialog.show({
        title: `Add ${validFiles.length} File(s)`,
        text: `Are you sure you want to add:\n${fileNames}`,
        icon: 'question',
        confirmButtonText: 'Yes, Add',
        confirmButtonColor: '#dc2626',
      });

      if (!confirmed) return;

      const newFiles = validFiles.map(file => ({
        file: file,
        isNew: true,
        fileName: file.name,
        originalFileName: file.name,
        fileSize: file.size,
        fileAttachmentId: `new-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`,
      }));

      internalUpdateRef.current = true;
      setFileList(prev => [...prev, ...newFiles]);

      if (onUploadComplete) {
        onUploadComplete(newFiles);
      }

      ConfirmDialog.toast.success(`${validFiles.length} file(s) added. Click Save/Update to upload.`);
    },
    [allowedTypes, maxSizeMB, onUploadComplete]
  );

  // ============================================================
  // DELETE
  // ============================================================
  const handleDeleteFile = useCallback(async (file) => {
    if (disableDelete) {
      ConfirmDialog.toast.warning("Delete is not available for this entity");
      return;
    }
    setFileToDelete(file);
    setShowDeleteConfirm(true);
  }, [disableDelete]);

  const confirmDeleteFile = useCallback(async () => {
    if (!fileToDelete) return;
    setShowDeleteConfirm(false);

    const file = fileToDelete;
    
    try {
      if (file.isNew) {
        internalUpdateRef.current = true;
        setFileList(prev => prev.filter(f => f.fileAttachmentId !== file.fileAttachmentId));
        if (onFileDeleted) onFileDeleted();
        ConfirmDialog.toast.success("File removed");
        setFileToDelete(null);
        return;
      }

      internalUpdateRef.current = true;
      setFileList(prev => prev.filter(f => f.fileAttachmentId !== file.fileAttachmentId));
      if (onFileDeleted) onFileDeleted();
      ConfirmDialog.toast.success("File removed. Click Save/Update to apply changes.");
    } catch (error) {
      const errorMsg = error.userMessage || error.message || "Failed to delete";
      ConfirmDialog.toast.error(errorMsg);
    }
    setFileToDelete(null);
  }, [fileToDelete, onFileDeleted]);

  // ============================================================
  // PREVIEW
  // ============================================================
  const handlePreview = useCallback((file) => {
    if (disablePreview) {
      ConfirmDialog.toast.warning("Preview is not available");
      return;
    }
    setPreviewFile(file);
    setPreviewOpen(true);
  }, [disablePreview]);

  const handleClosePreview = useCallback(() => {
    setPreviewOpen(false);
    setPreviewFile(null);
  }, []);

  // ============================================================
  // DRAG & DROP
  // ============================================================
  const handleDragEnter = useCallback((e) => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragging(true);
  }, []);

  const handleDragLeave = useCallback((e) => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragging(false);
  }, []);

  const handleDragOver = useCallback((e) => {
    e.preventDefault();
    e.stopPropagation();
  }, []);

  const handleDrop = useCallback((e) => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragging(false);
    
    const files = e.dataTransfer.files;
    if (files && files.length > 0) {
      handleFileSelect(files);
    }
  }, [handleFileSelect]);

  // ============================================================
  // RENDER
  // ============================================================
  if (!referenceId) {
    return (
      <Box className="file-uploader-placeholder">
        <Typography variant="body2" color="text.secondary">
          Save the {entityType} first to enable file upload
        </Typography>
      </Box>
    );
  }

  const headerTitle = title || `${entityType.charAt(0).toUpperCase() + entityType.slice(1)} ${isAttachment ? 'Attachments' : 'Photo'}`;
  const fileCount = fileList.length;

  return (
    <div className={`file-uploader ${isAttachment ? 'file-uploader--attachment' : 'file-uploader--photo'}`}>
      <div className="file-uploader__header">
        <Typography variant="subtitle2" className="file-uploader__header-title">
          {isAttachment ? <FiPaperclip style={{ marginRight: 8 }} /> : <FiImage style={{ marginRight: 8 }} />}
          {headerTitle}
          {fileCount > 0 && (
            <span className="file-uploader__file-count">
              ({fileCount} file{fileCount > 1 ? 's' : ''})
            </span>
          )}
        </Typography>
      </div>

      <FileDropzone
        isDragging={isDragging}
        onDragEnter={handleDragEnter}
        onDragLeave={handleDragLeave}
        onDragOver={handleDragOver}
        onDrop={handleDrop}
        onFileSelect={handleFileSelect}
        allowedTypes={allowedTypes}
        maxSizeMB={maxSizeMB}
        isAttachment={isAttachment}
        hasFiles={fileCount > 0}
      />

      {fileCount > 0 && (
        <FileList
          files={fileList}
          onPreview={handlePreview}
          onDelete={handleDeleteFile}
          disablePreview={disablePreview}
          disableDelete={disableDelete}
        />
      )}

      {/* Delete Confirmation Dialog */}
      <Dialog
        open={showDeleteConfirm}
        onClose={() => setShowDeleteConfirm(false)}
        maxWidth="xs"
        fullWidth
        PaperProps={{
          sx: {
            backgroundColor: 'var(--card-bg)',
            borderRadius: '12px',
          },
        }}
      >
        <DialogContent sx={{ p: 3, textAlign: 'center' }}>
          <Box sx={{ display: 'flex', justifyContent: 'center', mb: 2 }}>
            <Box sx={{ 
              width: 60, 
              height: 60, 
              borderRadius: '50%', 
              backgroundColor: 'rgba(239, 68, 68, 0.1)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
            }}>
              <FiTrash2 size={28} color="#ef4444" />
            </Box>
          </Box>
          <Typography variant="h6" fontWeight={600} sx={{ color: 'var(--text-primary)', mb: 1 }}>
            Delete File
          </Typography>
          <Typography variant="body2" sx={{ color: 'var(--text-secondary)', mb: 3 }}>
            Are you sure you want to delete "{fileToDelete?.originalFileName || fileToDelete?.fileName}"?
          </Typography>
          <Box sx={{ display: 'flex', gap: 2, justifyContent: 'center' }}>
            <Button
              variant="outline"
              onClick={() => setShowDeleteConfirm(false)}
            >
              Cancel
            </Button>
            <Button
              variant="danger"
              onClick={confirmDeleteFile}
            >
              Delete
            </Button>
          </Box>
        </DialogContent>
      </Dialog>

      {/* Preview Dialog */}
      <FilePreview
        isOpen={previewOpen}
        file={previewFile}
        onClose={handleClosePreview}
      />
    </div>
  );
};

export default FileUploader;