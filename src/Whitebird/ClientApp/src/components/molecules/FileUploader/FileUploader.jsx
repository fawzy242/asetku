// src/components/molecules/FileUploader/FileUploader.jsx
import React, { useState, useCallback } from 'react';
import { FiUpload, FiX, FiImage, FiFile, FiTrash2, FiStar, FiEye } from 'react-icons/fi';
import { Box, Chip, LinearProgress, IconButton, Typography, Dialog, DialogContent, IconButton as MuiIconButton } from '@mui/material';
import apiService from '../../../core/services/api.service';
import ConfirmDialog from '../ConfirmDialog/ConfirmDialog';
import { handleSuccess } from '../../../core/services/successHandler';
import './FileUploader.scss';

/**
 * Generic File Uploader Component
 * 
 * @param {Object} props
 * @param {string} props.uploadEndpoint - API endpoint for upload (e.g., "/Asset/1/photo")
 * @param {number} props.referenceId - Reference ID for the entity (optional, for delete)
 * @param {Function} props.onUploadComplete - Callback after upload
 * @param {Function} props.onFileDeleted - Callback after file deleted
 * @param {number} props.maxFiles - Maximum number of files
 * @param {Array} props.allowedTypes - Allowed MIME types
 * @param {number} props.maxSizeMB - Maximum file size in MB
 * @param {string} props.entityType - Entity type for messages (e.g., "asset", "employee")
 * @param {boolean} props.disableDelete - Disable delete functionality
 */
const FileUploader = ({ 
  uploadEndpoint,
  referenceId, 
  onUploadComplete, 
  onFileDeleted,
  maxFiles = 10,
  allowedTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/webp', 'application/pdf'],
  maxSizeMB = 10,
  entityType = 'record',
  disableDelete = false
}) => {
  const [files, setFiles] = useState([]);
  const [uploading, setUploading] = useState(false);
  const [uploadProgress, setUploadProgress] = useState(0);
  const [loading, setLoading] = useState(false);
  const [previewOpen, setPreviewOpen] = useState(false);
  const [previewFile, setPreviewFile] = useState(null);
  const [previewUrl, setPreviewUrl] = useState(null);

  const fetchFiles = useCallback(async () => {
    if (!referenceId || !uploadEndpoint) {
      setFiles([]);
      return;
    }
    setLoading(true);
    try {
      const basePath = uploadEndpoint.split('/').slice(0, -1).join('/');
      const response = await apiService.get(`${basePath}/attachments`);
      if (response.data?.isSuccess) {
        setFiles(response.data.data || []);
      }
    } catch (error) {
      console.warn('Failed to fetch files:', error);
      setFiles([]);
    } finally {
      setLoading(false);
    }
  }, [referenceId, uploadEndpoint]);

  React.useEffect(() => {
    if (referenceId && uploadEndpoint) {
      fetchFiles();
    }
  }, [referenceId, uploadEndpoint, fetchFiles]);

  const handleFileSelect = useCallback(async (event) => {
    const selectedFiles = Array.from(event.target.files);
    if (files.length + selectedFiles.length > maxFiles) {
      ConfirmDialog.toast.error(`Maximum ${maxFiles} files allowed`);
      return;
    }

    for (const file of selectedFiles) {
      if (!allowedTypes.includes(file.type)) {
        ConfirmDialog.toast.error(`File type not allowed: ${file.name}`);
        return;
      }
      if (file.size > maxSizeMB * 1024 * 1024) {
        ConfirmDialog.toast.error(`File too large: ${file.name} (max ${maxSizeMB}MB)`);
        return;
      }
    }

    if (!uploadEndpoint) {
      ConfirmDialog.toast.error('Upload endpoint not available. Please save the record first.');
      return;
    }

    setUploading(true);
    setUploadProgress(0);

    for (let i = 0; i < selectedFiles.length; i++) {
      const file = selectedFiles[i];
      const formData = new FormData();
      formData.append('file', file);

      try {
        const response = await apiService.post(uploadEndpoint, formData, {
          headers: { 'Content-Type': 'multipart/form-data' },
          onUploadProgress: (progressEvent) => {
            const percent = Math.round((progressEvent.loaded * 100) / progressEvent.total);
            setUploadProgress(percent);
          },
        });
        if (response.data?.isSuccess) {
          handleSuccess({
            operation: 'upload',
            entityName: 'file',
            message: `Uploaded: ${file.name}`,
          });
        }
      } catch (error) {
        ConfirmDialog.toast.error(`Failed to upload: ${file.name}`);
      }
      setUploadProgress(Math.round(((i + 1) / selectedFiles.length) * 100));
    }

    setUploading(false);
    setUploadProgress(0);
    fetchFiles();
    if (onUploadComplete) onUploadComplete();
  }, [files.length, maxFiles, allowedTypes, maxSizeMB, uploadEndpoint, fetchFiles, onUploadComplete]);

  const handleDelete = useCallback(async (fileId, fileName) => {
    if (disableDelete) {
      ConfirmDialog.toast.warning('Delete is not available for this entity');
      return;
    }

    const confirmed = await ConfirmDialog.show({
      title: 'Delete File',
      text: `Are you sure you want to delete "${fileName}"?`,
      icon: 'warning',
      confirmButtonText: 'Delete',
    });
    if (!confirmed) return;

    try {
      const response = await apiService.delete(`/FileAttachment/${fileId}`);
      if (response.data?.isSuccess) {
        ConfirmDialog.toast.success('File deleted');
        fetchFiles();
        if (onFileDeleted) onFileDeleted();
      } else {
        ConfirmDialog.toast.error('Failed to delete');
      }
    } catch {
      ConfirmDialog.toast.error('Failed to delete');
    }
  }, [fetchFiles, onFileDeleted, disableDelete]);

  const handleSetPrimary = useCallback(async (fileId) => {
    try {
      const response = await apiService.put(`/FileAttachment/${fileId}`, { isPrimary: true });
      if (response.data?.isSuccess) {
        ConfirmDialog.toast.success('Primary image set');
        fetchFiles();
      }
    } catch {
      ConfirmDialog.toast.error('Failed to set primary');
    }
  }, [fetchFiles]);

  const handlePreview = useCallback(async (file) => {
    setPreviewFile(file);
    setPreviewOpen(true);
    
    try {
      const response = await apiService.get(`/FileAttachment/download/${file.fileAttachmentId}`, {
        responseType: 'blob'
      });
      const blob = response.data;
      const url = URL.createObjectURL(blob);
      setPreviewUrl(url);
    } catch (error) {
      console.error('Failed to load file preview:', error);
      ConfirmDialog.toast.error('Failed to load file preview');
    }
  }, []);

  const handleClosePreview = useCallback(() => {
    if (previewUrl) {
      URL.revokeObjectURL(previewUrl);
    }
    setPreviewOpen(false);
    setPreviewFile(null);
    setPreviewUrl(null);
  }, [previewUrl]);

  const isImage = (mimeType) => mimeType?.startsWith('image/');
  const isPdf = (mimeType) => mimeType === 'application/pdf';
  
  const getFileIcon = (mimeType, fileName) => {
    if (isImage(mimeType)) return <FiImage size={24} />;
    if (isPdf(mimeType)) return <FiFile size={24} />;
    return <FiFile size={24} />;
  };

  const getFileTypeLabel = (mimeType, fileName) => {
    if (isImage(mimeType)) return 'Image';
    if (isPdf(mimeType)) return 'PDF';
    return 'Document';
  };

  if (!referenceId) {
    return (
      <Box className="file-uploader-placeholder">
        <Typography variant="body2" color="text.secondary">
          Save the {entityType} first to enable file attachments
        </Typography>
      </Box>
    );
  }

  return (
    <div className="file-uploader">
      <div className="file-uploader__header">
        <Typography variant="subtitle2">Attachments ({files.length}/{maxFiles})</Typography>
        <label className="file-uploader__button">
          <FiUpload size={16} />
          Upload
          <input
            type="file"
            multiple
            onChange={handleFileSelect}
            disabled={uploading || !uploadEndpoint}
            accept={allowedTypes.join(',')}
            style={{ display: 'none' }}
          />
        </label>
      </div>

      {uploading && (
        <Box sx={{ mb: 2 }}>
          <LinearProgress variant="determinate" value={uploadProgress} />
          <Typography variant="caption" color="text.secondary">
            Uploading... {uploadProgress}%
          </Typography>
        </Box>
      )}

      {loading ? (
        <Typography variant="body2" color="text.secondary">Loading...</Typography>
      ) : files.length === 0 ? (
        <Typography variant="body2" color="text.secondary" className="file-uploader__empty">
          No attachments. Click Upload to add files.
        </Typography>
      ) : (
        <div className="file-uploader__list">
          {files.map((file) => (
            <div key={file.fileAttachmentId} className={`file-uploader__item ${file.isPrimary ? 'file-uploader__item--primary' : ''}`}>
              <div className="file-uploader__icon">{getFileIcon(file.fileMimeType, file.originalFileName)}</div>
              <div className="file-uploader__info">
                <div className="file-uploader__name">{file.originalFileName}</div>
                <div className="file-uploader__meta">
                  {file.fileSize && <span>{Math.round(file.fileSize / 1024)} KB</span>}
                  <span className="file-uploader__type">{getFileTypeLabel(file.fileMimeType, file.originalFileName)}</span>
                  {file.isPrimary && <Chip label="Primary" size="small" color="primary" />}
                </div>
              </div>
              <div className="file-uploader__actions">
                <IconButton size="small" onClick={() => handlePreview(file)} title="Preview">
                  <FiEye size={16} />
                </IconButton>
                {!file.isPrimary && isImage(file.fileMimeType) && (
                  <IconButton size="small" onClick={() => handleSetPrimary(file.fileAttachmentId)} title="Set as primary">
                    <FiStar size={16} />
                  </IconButton>
                )}
                {!disableDelete && (
                  <IconButton size="small" onClick={() => handleDelete(file.fileAttachmentId, file.originalFileName)} title="Delete">
                    <FiTrash2 size={16} />
                  </IconButton>
                )}
              </div>
            </div>
          ))}
        </div>
      )}

      <Dialog
        open={previewOpen}
        onClose={handleClosePreview}
        maxWidth="lg"
        fullWidth
        PaperProps={{
          sx: {
            backgroundColor: 'var(--card-bg)',
            borderRadius: '12px',
          }
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
              '&:hover': { backgroundColor: 'rgba(0,0,0,0.7)' }
            }}
          >
            <FiX size={20} />
          </MuiIconButton>
          
          {previewFile && previewUrl && (
            <Box sx={{ width: '100%', height: '70vh', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
              {isImage(previewFile.fileMimeType) ? (
                <img 
                  src={previewUrl} 
                  alt={previewFile.originalFileName}
                  style={{ maxWidth: '100%', maxHeight: '100%', objectFit: 'contain' }}
                />
              ) : isPdf(previewFile.fileMimeType) ? (
                <iframe
                  src={previewUrl}
                  title={previewFile.originalFileName}
                  style={{ width: '100%', height: '100%', border: 'none' }}
                />
              ) : (
                <Box sx={{ textAlign: 'center' }}>
                  <FiFile size={64} style={{ color: 'var(--text-secondary)', marginBottom: 16 }} />
                  <Typography variant="h6">{previewFile.originalFileName}</Typography>
                  <Typography variant="body2" color="text.secondary">
                    Preview not available for this file type
                  </Typography>
                </Box>
              )}
            </Box>
          )}
        </DialogContent>
      </Dialog>
    </div>
  );
};

export default FileUploader;