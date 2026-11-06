// src/components/molecules/FileUploader/FileList.jsx
import React from 'react';
import { FiEye, FiX, FiImage, FiFileText, FiFile } from 'react-icons/fi';
import { Chip } from '@mui/material';
import { getFileIcon, formatFileSize } from '../../../core/utils/file.utils';
import './FileUploader.scss';

const ICON_MAP = {
  image: FiImage,
  pdf: FiFileText,
  document: FiFileText,
  excel: FiFileText,
  text: FiFileText,
  file: FiFile,
};

/**
 * Prefer backend contentType over extension inference (Phase 3 contract).
 * Falls back to filename-based detection for staged files that don't
 * yet have a contentType (not uploaded → no backend metadata).
 */
const deriveIconKey = (file) => {
  const ct = (file?.contentType || '').toLowerCase();
  if (ct.startsWith('image/')) return 'image';
  if (ct === 'application/pdf') return 'pdf';
  if (ct.includes('word') || ct.includes('document')) return 'document';
  if (ct.includes('excel') || ct.includes('spreadsheet')) return 'excel';
  if (ct.startsWith('text/')) return 'text';
  return getFileIcon(file?.originalFileName || file?.fileName);
};

const isPreviewable = (file) => {
  const ct = (file?.contentType || '').toLowerCase();
  if (ct.startsWith('image/') || ct === 'application/pdf') return true;
  const name = (file?.originalFileName || file?.fileName || '').toLowerCase();
  return /\.(jpe?g|png|gif|webp|bmp|svg)$/.test(name) || name.endsWith('.pdf');
};

const FileList = ({
  files,
  onPreview,
  onDelete,
  disablePreview,
  disableDelete,
}) => {
  if (!files || files.length === 0) return null;

  const renderFileCard = (file, index) => {
    const FileIcon = ICON_MAP[deriveIconKey(file)] || FiFile;
    const previewable = isPreviewable(file);
    const displayName = file.originalFileName || file.fileName || 'Untitled';

    return (
      <div
        key={file.fileAttachmentId || `file-${index}`}
        className="file-uploader__file-card"
      >
        <div className="file-uploader__file-card-content">
          <div className="file-uploader__file-icon">
            <FileIcon size={24} />
          </div>
          <div className="file-uploader__file-info">
            <span className="file-uploader__file-name" title={displayName}>
              {displayName}
            </span>
            {file.fileSize ? (
              <span className="file-uploader__file-size">
                {formatFileSize(file.fileSize)}
              </span>
            ) : null}
            {file.isNew && (
              <Chip
                label="New"
                size="small"
                color="info"
                className="file-uploader__file-badge"
              />
            )}
          </div>
        </div>
        <div className="file-uploader__file-actions">
          {previewable && !disablePreview && (
            <button
              type="button"
              className="file-uploader__file-action file-uploader__file-action--preview"
              onClick={() => onPreview(file)}
              title="Preview"
              aria-label={`Preview ${displayName}`}
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
              aria-label={`Delete ${displayName}`}
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