// src/components/molecules/FileUploader/FilePreview.jsx
import React, { useState, useEffect } from 'react';
import { FiX, FiFile, FiDownload, FiAlertCircle } from 'react-icons/fi';
import {
  Dialog,
  DialogContent,
  Typography,
  IconButton as MuiIconButton,
  Box,
  CircularProgress,
} from '@mui/material';
import Button from '../../atoms/Button/Button';
import { useFileBlob } from '../../../hooks/useFileBlob';
import { downloadBlob, formatFileSize } from '../../../core/utils/file.utils';

/**
 * File preview dialog.
 *
 * Accepts EITHER:
 *   - a backend FileResourceDto (existing file):
 *       { fileAttachmentId, originalFileName, fileName, contentType,
 *         fileSize, downloadUrl, fileUrl?, ... }
 *   - a staged-file wrapper (from FileUploader, not yet uploaded):
 *       { file: File, isNew: true, fileName, originalFileName, fileSize,
 *         fileAttachmentId: 'new-…' }
 *
 * Rendering:
 *   - image files: inline <img> from a blob URL
 *   - PDFs: inline <iframe> from a blob URL + download fallback
 *   - other: metadata + download button
 *
 * Fetching:
 *   - existing files: useFileBlob (authenticated axios → blob → objectURL)
 *   - staged files: local URL.createObjectURL(file.file), revoked on cleanup
 *   - download reuses the already-fetched blob — no extra network call
 */
const FilePreview = ({ isOpen, file, onClose }) => {
  const isNewFile = Boolean(file?.isNew && file?.file instanceof File);

  // Staged files — local blob URL (client only, no auth)
  const [stagedBlobUrl, setStagedBlobUrl] = useState(null);
  useEffect(() => {
    if (!isNewFile || !(file?.file instanceof File)) {
      setStagedBlobUrl(null);
      return undefined;
    }
    const url = URL.createObjectURL(file.file);
    setStagedBlobUrl(url);
    return () => URL.revokeObjectURL(url);
  }, [isNewFile, file?.file]);

  // Existing files — authenticated fetch via useFileBlob
  const backendPath = isNewFile
    ? null
    : file?.downloadUrl || file?.fileUrl || null;
  const {
    blob: backendBlob,
    blobUrl: backendBlobUrl,
    loading: fetching,
  } = useFileBlob(backendPath);

  const effectiveUrl = isNewFile ? stagedBlobUrl : backendBlobUrl;
  const effectiveBlob = isNewFile ? file?.file : backendBlob;

  // Type detection: prefer contentType (backend contract) over extension
  const fileName = file?.originalFileName || file?.fileName || '';
  const contentType = (file?.contentType || '').toLowerCase();
  const lowerName = fileName.toLowerCase();
  const isImage =
    contentType.startsWith('image/') ||
    /\.(jpe?g|png|gif|webp|bmp|svg)$/i.test(lowerName);
  const isPdf =
    contentType === 'application/pdf' || lowerName.endsWith('.pdf');

  // Image render state
  const [imageLoading, setImageLoading] = useState(false);
  const [imageError, setImageError] = useState(false);
  useEffect(() => {
    if (!effectiveUrl) {
      setImageLoading(false);
      setImageError(false);
      return;
    }
    setImageLoading(true);
    setImageError(false);
  }, [effectiveUrl]);

  const handleDownload = () => {
    if (!effectiveBlob) return;
    downloadBlob(effectiveBlob, fileName || 'file');
  };

  if (!file) return null;

  const isLoading = !isNewFile && fetching;
  const hasLoadableFile = Boolean(effectiveUrl) && !isLoading;

  // ============================================================
  // RENDERERS
  // ============================================================
  const renderImagePreview = () => {
    if (isLoading) {
      return (
        <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: 200 }}>
          <CircularProgress />
        </Box>
      );
    }
    if (imageError) {
      return (
        <Box sx={{ textAlign: 'center' }}>
          <FiAlertCircle
            size={48}
            style={{ color: 'var(--error)', marginBottom: 16 }}
          />
          <Typography variant="h6">Unable to load preview</Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
            The file may be unavailable, or your session may have expired.
          </Typography>
          {effectiveBlob && (
            <Button
              variant="primary"
              startIcon={<FiDownload />}
              onClick={handleDownload}
            >
              Download Instead
            </Button>
          )}
        </Box>
      );
    }
    if (!effectiveUrl) {
      return (
        <Box sx={{ textAlign: 'center' }}>
          <FiFile
            size={48}
            style={{ color: 'var(--text-secondary)', marginBottom: 16 }}
          />
          <Typography>Image preview not available</Typography>
        </Box>
      );
    }
    return (
      <>
        {imageLoading && (
          <Box sx={{ position: 'absolute' }}>
            <CircularProgress />
          </Box>
        )}
        <img
          src={effectiveUrl}
          alt={fileName}
          onLoad={() => setImageLoading(false)}
          onError={() => {
            setImageLoading(false);
            setImageError(true);
          }}
          style={{
            maxWidth: '100%',
            maxHeight: '100%',
            objectFit: 'contain',
            display: imageLoading ? 'none' : 'block',
          }}
        />
      </>
    );
  };

  const renderPdfPreview = () => {
    if (isLoading) {
      return (
        <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: 200 }}>
          <CircularProgress />
        </Box>
      );
    }
    if (!effectiveUrl) {
      return (
        <Box sx={{ textAlign: 'center' }}>
          <FiFile
            size={48}
            style={{ color: 'var(--text-secondary)', marginBottom: 16 }}
          />
          <Typography variant="h6">{fileName || 'PDF'}</Typography>
          <Typography variant="body2" color="text.secondary">
            PDF preview not available
          </Typography>
        </Box>
      );
    }
    return (
      <Box
        sx={{
          width: '100%',
          height: '100%',
          display: 'flex',
          flexDirection: 'column',
        }}
      >
        <Box sx={{ flex: 1, minHeight: 0 }}>
          <iframe
            src={effectiveUrl}
            title={fileName || 'PDF preview'}
            style={{ width: '100%', height: '100%', border: 'none' }}
          />
        </Box>
        <Box sx={{ display: 'flex', justifyContent: 'center', mt: 1, flexShrink: 0 }}>
          <Button
            variant="outline"
            size="sm"
            startIcon={<FiDownload />}
            onClick={handleDownload}
          >
            Download
          </Button>
        </Box>
      </Box>
    );
  };

  const renderFallback = () => (
    <Box sx={{ textAlign: 'center' }}>
      <FiFile
        size={48}
        style={{ color: 'var(--text-secondary)', marginBottom: 16 }}
      />
      <Typography variant="h6">{fileName || 'File'}</Typography>
      {contentType && (
        <Typography
          variant="caption"
          color="text.secondary"
          sx={{ display: 'block', mb: 1 }}
        >
          {contentType}
        </Typography>
      )}
      {file?.fileSize ? (
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          {formatFileSize(file.fileSize)}
        </Typography>
      ) : (
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          Preview not available for this file type
        </Typography>
      )}
      {effectiveBlob && (
        <Button
          variant="primary"
          startIcon={<FiDownload />}
          onClick={handleDownload}
        >
          Download
        </Button>
      )}
    </Box>
  );

  const renderPreviewContent = () => {
    if (isImage) return renderImagePreview();
    if (isPdf) return renderPdfPreview();
    return renderFallback();
  };

  // ============================================================
  // DIALOG
  // ============================================================
  return (
    <Dialog
      open={isOpen}
      onClose={onClose}
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
          onClick={onClose}
          aria-label="Close preview"
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
            position: 'relative',
          }}
        >
          {hasLoadableFile || isLoading || imageError || !effectiveUrl
            ? renderPreviewContent()
            : renderPreviewContent()}
        </Box>
      </DialogContent>
    </Dialog>
  );
};

export default FilePreview;