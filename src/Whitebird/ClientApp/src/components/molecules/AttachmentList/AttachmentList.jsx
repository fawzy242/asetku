// src/components/molecules/AttachmentList/AttachmentList.jsx
import React, { useState, useCallback } from 'react';
import { FiEye, FiDownload, FiFile, FiImage, FiFileText } from 'react-icons/fi';
import { Box, Typography, CircularProgress } from '@mui/material';
import FilePreview from '../FileUploader/FilePreview';
import { useSweetAlert } from '../../../hooks/useSweetAlert';
import apiService from '../../../core/services/api.service';
import { downloadBlob, formatFileSize } from '../../../core/utils/file.utils';
import './AttachmentList.scss';

const ICON_MAP = {
  image: FiImage,
  pdf: FiFileText,
  document: FiFileText,
  excel: FiFileText,
  text: FiFileText,
  file: FiFile,
};

const deriveIconKey = (att) => {
  const ct = (att?.contentType || '').toLowerCase();
  if (ct.startsWith('image/')) return 'image';
  if (ct === 'application/pdf') return 'pdf';
  if (ct.includes('word') || ct.includes('document')) return 'document';
  if (ct.includes('excel') || ct.includes('spreadsheet')) return 'excel';
  if (ct.startsWith('text/')) return 'text';
  const name = (att?.originalFileName || att?.fileName || '').toLowerCase();
  if (/\.(jpe?g|png|gif|webp|bmp|svg)$/.test(name)) return 'image';
  if (name.endsWith('.pdf')) return 'pdf';
  if (/\.(docx?|odt)$/.test(name)) return 'document';
  if (/\.(xlsx?|ods|csv)$/.test(name)) return 'excel';
  if (/\.txt$/.test(name)) return 'text';
  return 'file';
};

const canPreview = (att) => {
  const ct = (att?.contentType || '').toLowerCase();
  if (ct.startsWith('image/') || ct === 'application/pdf') return true;
  const name = (att?.originalFileName || att?.fileName || '').toLowerCase();
  return (
    /\.(jpe?g|png|gif|webp|bmp|svg)$/.test(name) || name.endsWith('.pdf')
  );
};

/**
 * Read-only list of file attachments.
 *
 * Contract: each entry is a FileResourceDto (backend change log §2.2):
 *   { fileAttachmentId, originalFileName, fileName, contentType, fileSize,
 *     downloadUrl, fileUrl?, createdDate, ... }
 *
 * Backend requires X-Session-Token on download endpoints. `<a href>` cannot
 * send that header → we route downloads through authenticated axios
 * (apiService.getFileBlob) and trigger the browser download via the
 * existing downloadBlob() helper. Preview opens FilePreview, which performs
 * its own authenticated fetch.
 */
const AttachmentList = ({ attachments = [], title = 'Attachments' }) => {
  const [previewAtt, setPreviewAtt] = useState(null);
  const [downloadingIds, setDownloadingIds] = useState(() => new Set());
  const { toast } = useSweetAlert();

  const handlePreview = useCallback((att) => setPreviewAtt(att), []);
  const handleClosePreview = useCallback(() => setPreviewAtt(null), []);

  const handleDownload = useCallback(
    async (att) => {
      const backendPath = att.downloadUrl || att.fileUrl;
      if (!backendPath) return;

      const id = att.fileAttachmentId;
      setDownloadingIds((prev) => {
        const next = new Set(prev);
        next.add(id);
        return next;
      });

      try {
        const blob = await apiService.getFileBlob(backendPath);
        if (blob) {
          downloadBlob(blob, att.originalFileName || att.fileName || 'file');
        } else {
          toast.error('File is no longer available');
        }
      } catch (err) {
        toast.error(
          err?.userMessage || err?.message || 'Failed to download file'
        );
      } finally {
        setDownloadingIds((prev) => {
          const next = new Set(prev);
          next.delete(id);
          return next;
        });
      }
    },
    [toast]
  );

  if (!attachments || attachments.length === 0) return null;

  return (
    <Box className="attachment-list">
      <Typography variant="subtitle2" className="attachment-list__title">
        {title} ({attachments.length})
      </Typography>
      <ul className="attachment-list__items">
        {attachments.map((att, index) => {
          const FileIcon = ICON_MAP[deriveIconKey(att)] || FiFile;
          const previewable = canPreview(att);
          const displayName =
            att.originalFileName || att.fileName || 'Untitled';
          const hasFile = Boolean(att.downloadUrl || att.fileUrl);
          const isDownloading = downloadingIds.has(att.fileAttachmentId);

          return (
            <li
              key={att.fileAttachmentId || `attachment-${index}`}
              className="attachment-list__item"
            >
              <div className="attachment-list__icon">
                <FileIcon size={18} />
              </div>
              <div className="attachment-list__info">
                <span className="attachment-list__name" title={displayName}>
                  {displayName}
                </span>
                {att.fileSize ? (
                  <span className="attachment-list__size">
                    {formatFileSize(att.fileSize)}
                  </span>
                ) : null}
              </div>
              <div className="attachment-list__actions">
                {previewable && hasFile && (
                  <button
                    type="button"
                    className="attachment-list__action attachment-list__action--preview"
                    onClick={() => handlePreview(att)}
                    title="Preview"
                    aria-label={`Preview ${displayName}`}
                  >
                    <FiEye size={16} />
                  </button>
                )}
                {hasFile && (
                  <button
                    type="button"
                    className="attachment-list__action attachment-list__action--download"
                    onClick={() => handleDownload(att)}
                    disabled={isDownloading}
                    title="Download"
                    aria-label={`Download ${displayName}`}
                  >
                    {isDownloading ? (
                      <CircularProgress size={14} sx={{ color: 'inherit' }} />
                    ) : (
                      <FiDownload size={16} />
                    )}
                  </button>
                )}
              </div>
            </li>
          );
        })}
      </ul>

      <FilePreview
        isOpen={Boolean(previewAtt)}
        file={previewAtt}
        onClose={handleClosePreview}
      />
    </Box>
  );
};

export default AttachmentList;