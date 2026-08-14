// src/components/molecules/FileUploader/FileUploader.jsx
import React, { useState, useCallback, useEffect, useRef } from "react";
import {
  FiUpload,
  FiX,
  FiImage,
  FiFile,
  FiTrash2,
  FiEye,
  FiPaperclip,
  FiDownload,
} from "react-icons/fi";
import {
  Box,
  Chip,
  IconButton,
  Typography,
  Dialog,
  DialogContent,
  IconButton as MuiIconButton,
} from "@mui/material";
import ConfirmDialog from "../ConfirmDialog/ConfirmDialog";
import Button from "../../atoms/Button/Button";
import "./FileUploader.scss";

const PREVIEW_EXTENSIONS = ['.jpg', '.jpeg', '.png', '.gif', '.webp', '.pdf', '.txt', '.doc', '.docx', '.xls', '.xlsx'];

const isPreviewable = (fileName) => {
  if (!fileName) return false;
  const ext = `.${fileName.split('.').pop()?.toLowerCase()}`;
  return PREVIEW_EXTENSIONS.includes(ext);
};

const getFileIcon = (fileName) => {
  if (!fileName) return FiFile;
  const ext = `.${fileName.split('.').pop()?.toLowerCase()}`;
  if (['.jpg', '.jpeg', '.png', '.gif', '.webp'].includes(ext)) return FiImage;
  return FiFile;
};

const FileUploader = ({
  referenceType,
  referenceId,
  onUploadComplete,
  onFileDeleted,
  allowedTypes = [".jpg", ".jpeg", ".png", ".gif", ".webp", ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt"],
  maxSizeMB = 10,
  entityType = "record",
  disableDelete = false,
  disablePreview = false,
  title = null,
  isAttachment = false,
  existingFiles = [],
  onFilesChange,
}) => {
  const [fileList, setFileList] = useState([]);
  const [isDragging, setIsDragging] = useState(false);
  const [previewOpen, setPreviewOpen] = useState(false);
  const [previewFile, setPreviewFile] = useState(null);
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [fileToDelete, setFileToDelete] = useState(null);
  const isMountedRef = useRef(true);
  const fileInputRef = useRef(null);
  const dropRef = useRef(null);

  // 🔥 FIX: Hanya set initial files sekali saat komponen mount
  // Jangan re-run ketika existingFiles berubah dari parent
  const hasInitializedRef = useRef(false);
  useEffect(() => {
    if (!hasInitializedRef.current && existingFiles && Array.isArray(existingFiles) && existingFiles.length > 0) {
      setFileList(existingFiles);
      hasInitializedRef.current = true;
    }
  }, [existingFiles]);

  // 🔥 FIX: Hanya notify parent ketika fileList berubah DARI internal (bukan dari parent)
  // Gunakan ref untuk track apakah perubahan berasal dari internal
  const internalUpdateRef = useRef(false);
  
  useEffect(() => {
    // Jika perubahan berasal dari internal update, notify parent
    if (internalUpdateRef.current) {
      if (onFilesChange) {
        onFilesChange(fileList);
      }
      internalUpdateRef.current = false;
    }
  }, [fileList, onFilesChange]);

  // ============================================================
  // HANDLE FILE SELECT
  // ============================================================
  const handleFileSelect = useCallback(
    async (files) => {
      if (!files || files.length === 0) return;

      const validFiles = [];
      const invalidFiles = [];
      for (const file of files) {
        const ext = `.${file.name.split('.').pop()?.toLowerCase()}`;
        const isValidExt = allowedTypes.some(t => 
          t.startsWith('.') ? t === ext : file.type === t
        );
        
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

      // 🔥 FIX: Update state dengan flag internal update
      internalUpdateRef.current = true;
      setFileList(prev => [...prev, ...newFiles]);

      if (onUploadComplete) {
        onUploadComplete(newFiles);
      }

      ConfirmDialog.toast.success(`${validFiles.length} file(s) added. Click Save/Update to upload.`);
    },
    [allowedTypes, maxSizeMB, onUploadComplete]
  );

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

  const handleBrowseClick = useCallback(() => {
    if (fileInputRef.current) {
      fileInputRef.current.click();
    }
  }, []);

  const formatFileSize = (bytes) => {
    if (!bytes) return '';
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  const renderFileCard = (file, index) => {
    const FileIcon = getFileIcon(file.originalFileName || file.fileName);
    const isPreviewableFile = isPreviewable(file.originalFileName || file.fileName);

    return (
      <div key={file.fileAttachmentId || index} className="file-uploader__file-card">
        <div className="file-uploader__file-card-content">
          <div className="file-uploader__file-icon">
            <FileIcon size={24} />
          </div>
          <div className="file-uploader__file-info">
            <span className="file-uploader__file-name">
              {file.originalFileName || file.fileName || 'Untitled'}
            </span>
            <span className="file-uploader__file-size">
              {formatFileSize(file.fileSize)}
            </span>
            {file.isNew && (
              <Chip label="New" size="small" color="info" className="file-uploader__file-badge" />
            )}
          </div>
        </div>
        <div className="file-uploader__file-actions">
          {isPreviewableFile && !disablePreview && (
            <button
              type="button"
              className="file-uploader__file-action file-uploader__file-action--preview"
              onClick={() => handlePreview(file)}
              title="Preview"
            >
              <FiEye size={16} />
            </button>
          )}
          {!disableDelete && (
            <button
              type="button"
              className="file-uploader__file-action file-uploader__file-action--delete"
              onClick={() => handleDeleteFile(file)}
              title="Delete"
            >
              <FiX size={16} />
            </button>
          )}
        </div>
      </div>
    );
  };

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

      <div
        ref={dropRef}
        className={`file-uploader__dropzone ${isDragging ? 'file-uploader__dropzone--dragging' : ''} ${fileCount > 0 ? 'file-uploader__dropzone--has-files' : ''}`}
        onDragEnter={handleDragEnter}
        onDragLeave={handleDragLeave}
        onDragOver={handleDragOver}
        onDrop={handleDrop}
        onClick={handleBrowseClick}
      >
        <input
          type="file"
          ref={fileInputRef}
          onChange={(e) => {
            if (e.target.files && e.target.files.length > 0) {
              handleFileSelect(e.target.files);
            }
            e.target.value = '';
          }}
          accept={allowedTypes.filter(t => t.startsWith('.')).join(',')}
          multiple={isAttachment}
          className="file-uploader__file-input"
        />
        
        <div className="file-uploader__dropzone-content">
          <FiUpload className="file-uploader__dropzone-icon" size={32} />
          <Typography variant="body2" className="file-uploader__dropzone-text">
            {isDragging ? 'Drop files here...' : 'Drag & drop files here or click to browse'}
          </Typography>
          <Typography variant="caption" className="file-uploader__dropzone-hint">
            {isAttachment 
              ? `Supported: ${allowedTypes.join(', ')} (max ${maxSizeMB}MB each)`
              : `Max ${maxSizeMB}MB, supported: ${allowedTypes.join(', ')}`
            }
          </Typography>
          <button
            type="button"
            className="file-uploader__dropzone-btn"
            onClick={(e) => {
              e.stopPropagation();
              handleBrowseClick();
            }}
          >
            {fileCount > 0 ? 'Add More Files' : 'Add Files'}
          </button>
        </div>
      </div>

      {fileCount > 0 && (
        <div className="file-uploader__file-list">
          {fileList.map((file, index) => renderFileCard(file, index))}
        </div>
      )}

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

      <Dialog
        open={previewOpen}
        onClose={handleClosePreview}
        maxWidth="lg"
        fullWidth
        PaperProps={{
          sx: {
            backgroundColor: 'var(--card-bg)',
            borderRadius: '12px',
          },
        }}
      >
        <DialogContent sx={{ p: 0, position: 'relative', minHeight: 400 }}>
          <MuiIconButton
            onClick={handleClosePreview}
            sx={{
              position: 'absolute',
              top: 8,
              right: 8,
              zIndex: 1,
              backgroundColor: 'rgba(0,0,0,0.5)',
              color: 'white',
              '&:hover': { backgroundColor: 'rgba(0,0,0,0.7)' },
            }}
          >
            <FiX size={20} />
          </MuiIconButton>
          <Box
            sx={{
              width: '100%',
              height: '70vh',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              p: 2,
            }}
          >
            {previewFile && (() => {
              const fileName = previewFile.originalFileName || previewFile.fileName || '';
              const ext = `.${fileName.split('.').pop()?.toLowerCase()}`;
              
              if (['.jpg', '.jpeg', '.png', '.gif', '.webp'].includes(ext)) {
                const url = previewFile.previewUrl || previewFile.fileUrl;
                if (url) {
                  return (
                    <img
                      src={url}
                      alt={fileName}
                      style={{
                        maxWidth: '100%',
                        maxHeight: '100%',
                        objectFit: 'contain',
                      }}
                    />
                  );
                }
                return <Typography>Image preview not available</Typography>;
              }
              
              if (ext === '.pdf') {
                return (
                  <Box sx={{ textAlign: 'center' }}>
                    <FiFile size={48} style={{ color: 'var(--text-secondary)', marginBottom: 16 }} />
                    <Typography variant="h6">{fileName}</Typography>
                    <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                      PDF preview is not available in this view
                    </Typography>
                    <Button
                      variant="primary"
                      startIcon={<FiDownload />}
                      onClick={() => {
                        const url = previewFile.fileUrl || previewFile.previewUrl;
                        if (url) {
                          window.open(url, '_blank');
                        }
                      }}
                    >
                      Download / View PDF
                    </Button>
                  </Box>
                );
              }
              
              return (
                <Box sx={{ textAlign: 'center' }}>
                  <FiFile size={48} style={{ color: 'var(--text-secondary)', marginBottom: 16 }} />
                  <Typography variant="h6">{fileName}</Typography>
                  <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                    Preview not available for this file type
                  </Typography>
                  <Button
                    variant="primary"
                    startIcon={<FiDownload />}
                    onClick={() => {
                      const url = previewFile.fileUrl || previewFile.previewUrl;
                      if (url) {
                        window.open(url, '_blank');
                      }
                    }}
                  >
                    Download
                  </Button>
                </Box>
              );
            })()}
          </Box>
        </DialogContent>
      </Dialog>
    </div>
  );
};

export default FileUploader;