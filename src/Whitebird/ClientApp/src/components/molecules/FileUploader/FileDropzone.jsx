import React, { useCallback, useRef } from 'react';
import { FiUpload } from 'react-icons/fi';
import { Typography } from '@mui/material';
import './FileUploader.scss';

const FileDropzone = ({
  isDragging,
  onDragEnter,
  onDragLeave,
  onDragOver,
  onDrop,
  onFileSelect,
  allowedTypes,
  maxSizeMB,
  isAttachment,
  hasFiles,
}) => {
  const fileInputRef = useRef(null);

  const handleBrowseClick = useCallback(() => {
    if (fileInputRef.current) {
      fileInputRef.current.click();
    }
  }, []);

  const handleFileInputChange = useCallback((e) => {
    if (e.target.files && e.target.files.length > 0) {
      onFileSelect(e.target.files);
    }
    e.target.value = '';
  }, [onFileSelect]);

  return (
    <div
      className={`file-uploader__dropzone ${isDragging ? 'file-uploader__dropzone--dragging' : ''} ${hasFiles ? 'file-uploader__dropzone--has-files' : ''}`}
      onDragEnter={onDragEnter}
      onDragLeave={onDragLeave}
      onDragOver={onDragOver}
      onDrop={onDrop}
      onClick={handleBrowseClick}
    >
      <input
        type="file"
        ref={fileInputRef}
        onChange={handleFileInputChange}
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
          {hasFiles ? 'Add More Files' : 'Add Files'}
        </button>
      </div>
    </div>
  );
};

export default FileDropzone;