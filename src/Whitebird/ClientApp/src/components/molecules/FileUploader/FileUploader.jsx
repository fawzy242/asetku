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
  LinearProgress,
  IconButton,
  Typography,
  Dialog,
  DialogContent,
  IconButton as MuiIconButton,
} from "@mui/material";
import apiService from "../../../core/services/api.service";
import ConfirmDialog from "../ConfirmDialog/ConfirmDialog";
import { handleSuccess } from "../../../core/services/successHandler";
import Button from "../../atoms/Button/Button";
import "./FileUploader.scss";

// Allowed file extensions for preview
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
  if (['.pdf'].includes(ext)) return FiFile;
  if (['.doc', '.docx'].includes(ext)) return FiFile;
  if (['.xls', '.xlsx'].includes(ext)) return FiFile;
  if (['.txt'].includes(ext)) return FiFile;
  return FiFile;
};

/**
 * Generic File Uploader Component with Drop Zone
 * FIX: Hanya menambahkan file ke state, TIDAK langsung upload
 */
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
  const [uploading, setUploading] = useState(false);
  const [uploadProgress, setUploadProgress] = useState(0);
  const [loading, setLoading] = useState(false);
  const [previewOpen, setPreviewOpen] = useState(false);
  const [previewUrl, setPreviewUrl] = useState(null);
  const [previewFile, setPreviewFile] = useState(null);
  const [fileList, setFileList] = useState([]);
  const [isDragging, setIsDragging] = useState(false);
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [fileToDelete, setFileToDelete] = useState(null);
  const isMountedRef = useRef(true);
  const fileInputRef = useRef(null);
  const dropRef = useRef(null);

  // Load existing files from parent
  useEffect(() => {
    if (existingFiles && Array.isArray(existingFiles)) {
      setFileList(existingFiles);
    }
  }, [existingFiles]);

  // Notify parent when files change
  useEffect(() => {
    if (onFilesChange) {
      onFilesChange(fileList);
    }
  }, [fileList, onFilesChange]);

  // ============================================================
  // FIX: HANDLE FILE SELECT - TAMBAHKAN KE STATE, JANGAN UPLOAD
  // ============================================================
  const handleFileSelect = useCallback(
    async (files) => {
      if (!files || files.length === 0) return;

      // Validate each file
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

      // Show confirmation before adding files
      const fileNames = validFiles.map(f => f.name).join('\n');
      const confirmed = await ConfirmDialog.show({
        title: `Add ${validFiles.length} File(s)`,
        text: `Are you sure you want to add:\n${fileNames}`,
        icon: 'question',
        confirmButtonText: 'Yes, Add',
        confirmButtonColor: '#dc2626',
      });

      if (!confirmed) return;

      // ============================================================
      // FIX: TAMBAHKAN FILE KE STATE, JANGAN UPLOAD LANGSUNG
      // File akan dikirim saat user klik Submit/Update
      // ============================================================
      const newFiles = validFiles.map(file => ({
        file: file,
        isNew: true,
        fileName: file.name,
        originalFileName: file.name,
        fileSize: file.size,
        fileAttachmentId: Date.now() + Math.random() * 1000,
      }));

      // Update local state
      setFileList(prev => [...prev, ...newFiles]);

      // Notify parent
      if (onFilesChange) {
        onFilesChange([...fileList, ...newFiles]);
      }

      // Notify parent that files are ready to be saved
      if (onUploadComplete) {
        onUploadComplete(newFiles);
      }

      ConfirmDialog.toast.success(`${validFiles.length} file(s) added. Click Save/Update to upload.`);
    },
    [allowedTypes, maxSizeMB, fileList, onFilesChange, onUploadComplete]
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
      // If it's a new file (not yet saved to server), just remove from list
      if (file.isNew) {
        setFileList(prev => prev.filter(f => f.fileAttachmentId !== file.fileAttachmentId));
        if (onFileDeleted) onFileDeleted();
        if (onFilesChange) onFilesChange(fileList.filter(f => f.fileAttachmentId !== file.fileAttachmentId));
        ConfirmDialog.toast.success("File removed");
        setFileToDelete(null);
        return;
      }

      // For existing files, mark for removal
      setFileList(prev => prev.filter(f => f.fileAttachmentId !== file.fileAttachmentId));
      if (onFileDeleted) onFileDeleted();
      if (onFilesChange) onFilesChange(fileList.filter(f => f.fileAttachmentId !== file.fileAttachmentId));
      ConfirmDialog.toast.success("File removed. Click Save/Update to apply changes.");
    } catch (error) {
      const errorMsg = error.userMessage || error.message || "Failed to delete";
      ConfirmDialog.toast.error(errorMsg);
    }
    setFileToDelete(null);
  }, [fileToDelete, fileList, onFileDeleted, onFilesChange]);

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
    if (previewUrl) {
      URL.revokeObjectURL(previewUrl);
      setPreviewUrl(null);
    }
  }, [previewUrl]);

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
              className="file-uploader__file-action file-uploader__file-action--preview"
              onClick={() => handlePreview(file)}
              title="Preview"
            >
              <FiEye size={16} />
            </button>
          )}
          {!disableDelete && (
            <button
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

      {/* DROP ZONE */}
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
          disabled={uploading}
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

      {/* FILE LIST */}
      {fileCount > 0 && (
        <div className="file-uploader__file-list">
          {fileList.map((file, index) => renderFileCard(file, index))}
        </div>
      )}

      {/* DELETE CONFIRM DIALOG */}
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

      {/* PREVIEW DIALOG */}
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
              
              if (ext === '.txt') {
                return (
                  <Box sx={{ width: '100%', maxHeight: '100%', overflow: 'auto', p: 3, backgroundColor: 'var(--surface)', borderRadius: 1 }}>
                    <Typography variant="body2" component="pre" sx={{ whiteSpace: 'pre-wrap', wordBreak: 'break-word' }}>
                      {previewFile.textContent || 'No content available'}
                    </Typography>
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