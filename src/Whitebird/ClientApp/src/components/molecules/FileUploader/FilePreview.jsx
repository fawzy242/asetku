import React from 'react';
import { FiX, FiFile, FiDownload } from 'react-icons/fi';
import { Dialog, DialogContent, Typography, IconButton as MuiIconButton, Box } from '@mui/material';
import Button from '../../atoms/Button/Button';
import { isImageFile } from '../../../core/utils/file.utils';

const FilePreview = ({ isOpen, file, onClose }) => {
  if (!file) return null;

  const fileName = file.originalFileName || file.fileName || '';
  const isImage = isImageFile(fileName);
  const isPdf = fileName.toLowerCase().endsWith('.pdf');

  const handleDownload = () => {
    const url = file.fileUrl || file.previewUrl;
    if (url) {
      window.open(url, '_blank');
    }
  };

  const renderPreviewContent = () => {
    if (isImage) {
      const url = file.previewUrl || file.fileUrl;
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

    if (isPdf) {
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
            onClick={handleDownload}
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
          onClick={handleDownload}
        >
          Download
        </Button>
      </Box>
    );
  };

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
          {renderPreviewContent()}
        </Box>
      </DialogContent>
    </Dialog>
  );
};

export default FilePreview;