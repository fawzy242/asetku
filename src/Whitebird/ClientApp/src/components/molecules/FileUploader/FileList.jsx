import React from 'react';
import { FiEye, FiX } from 'react-icons/fi';
import { Chip, Typography } from '@mui/material';
import { getFileIcon, formatFileSize, isImageFile } from '../../../core/utils/file.utils';
import './FileUploader.scss';

const FileList = ({
  files,
  onPreview,
  onDelete,
  disablePreview,
  disableDelete,
}) => {
  if (!files || files.length === 0) {
    return null;
  }

  const renderFileCard = (file, index) => {
    const FileIcon = getFileIcon(file.originalFileName || file.fileName);
    const isPreviewable = isImageFile(file.originalFileName || file.fileName);
    const isPdf = file.originalFileName?.toLowerCase().endsWith('.pdf') || false;

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
          {(isPreviewable || isPdf) && !disablePreview && (
            <button
              type="button"
              className="file-uploader__file-action file-uploader__file-action--preview"
              onClick={() => onPreview(file)}
              title="Preview"
            >
              <FiEye size={16} />
            </button>
          )}
          {!disableDelete && (
            <button
              type="button"
              className="file-uploader__file-action file-uploader__file-action--delete"
              onClick={() => onDelete(file)}
              title="Delete"
            >
              <FiX size={16} />
            </button>
          )}
        </div>
      </div>
    );
  };

  return (
    <div className="file-uploader__file-list">
      {files.map((file, index) => renderFileCard(file, index))}
    </div>
  );
};

export default FileList;