// src/components/molecules/FileUploader/FileUploader.jsx
import React, { useState, useCallback, useEffect, useRef } from "react";
import {
  FiUpload,
  FiX,
  FiImage,
  FiFile,
  FiTrash2,
  FiStar,
  FiEye,
  FiPaperclip,
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
import "./FileUploader.scss";

/**
 * Generic File Uploader Component
 * Supports:
 * - Photo Profile (single file, with preview)
 * - Attachments (multiple files, list view)
 *
 * @param {Object} props
 * @param {string} props.uploadEndpoint - API endpoint for upload (e.g., "/Asset/1/photo")
 * @param {number} props.referenceId - Reference ID for the entity
 * @param {Function} props.onUploadComplete - Callback after upload
 * @param {Function} props.onFileDeleted - Callback after file deleted
 * @param {Array} props.allowedTypes - Allowed file extensions or MIME types
 * @param {number} props.maxSizeMB - Maximum file size in MB
 * @param {string} props.entityType - Entity type for messages (e.g., "asset", "employee")
 * @param {boolean} props.disableDelete - Disable delete functionality
 * @param {boolean} props.disablePreview - Disable preview functionality
 * @param {string} props.title - Custom title for the uploader
 * @param {boolean} props.isAttachment - If true, multiple files mode (attachment)
 */
const FileUploader = ({
  uploadEndpoint,
  referenceId,
  onUploadComplete,
  onFileDeleted,
  allowedTypes = ["image/jpeg", "image/png", "image/gif", "image/webp"],
  maxSizeMB = 5,
  entityType = "record",
  disableDelete = false,
  disablePreview = false,
  title = null,
  isAttachment = false,
}) => {
  const [uploading, setUploading] = useState(false);
  const [uploadProgress, setUploadProgress] = useState(0);
  const [hasFiles, setHasFiles] = useState(false);
  const [loading, setLoading] = useState(false);
  const [previewOpen, setPreviewOpen] = useState(false);
  const [previewUrl, setPreviewUrl] = useState(null);
  const [fileName, setFileName] = useState(null);
  const [fileList, setFileList] = useState([]);
  const isMountedRef = useRef(true);
  const fileInputRef = useRef(null);

  // Check existing files
  const checkFiles = useCallback(async () => {
    if (!referenceId || !uploadEndpoint) {
      setHasFiles(false);
      setFileList([]);
      return;
    }
    setLoading(true);
    try {
      if (isAttachment) {
        // For attachments: GET returns list of files
        const response = await apiService.get(uploadEndpoint);
        const files = response.data?.data || response.data || [];
        if (Array.isArray(files) && files.length > 0) {
          setHasFiles(true);
          setFileList(files);
        } else {
          setHasFiles(false);
          setFileList([]);
        }
      } else {
        // For photo profile: GET returns blob
        const response = await apiService.get(uploadEndpoint, {
          responseType: "blob",
        });
        if (response.data && response.data.size > 0) {
          setHasFiles(true);
          const url = URL.createObjectURL(response.data);
          setPreviewUrl(url);
          setFileName(`${entityType}_photo_${referenceId}`);
        } else {
          setHasFiles(false);
          setPreviewUrl(null);
        }
      }
    } catch (error) {
      // 404 means no files
      if (error.response?.status === 404) {
        setHasFiles(false);
        if (!isAttachment) setPreviewUrl(null);
        else setFileList([]);
      } else if (error?.isCanceled) {
        // Ignore canceled requests
      } else {
        console.warn("Failed to check files:", error);
        setHasFiles(false);
      }
    } finally {
      setLoading(false);
    }
  }, [referenceId, uploadEndpoint, isAttachment, entityType]);

  useEffect(() => {
    isMountedRef.current = true;
    if (referenceId && uploadEndpoint) {
      checkFiles();
    }
    return () => {
      isMountedRef.current = false;
      if (previewUrl) {
        URL.revokeObjectURL(previewUrl);
      }
    };
  }, [referenceId, uploadEndpoint, checkFiles]);

  const handleFileSelect = useCallback(
    async (event) => {
      const files = event.target.files;
      if (!files || files.length === 0) return;

      if (!uploadEndpoint) {
        ConfirmDialog.toast.error(
          "Upload endpoint not available. Please save the record first."
        );
        return;
      }

      // Validate each file
      for (const file of files) {
        const ext = `.${file.name.split('.').pop()?.toLowerCase()}`;
        const isValidExt = allowedTypes.some(t => 
          t.startsWith('.') ? t === ext : file.type === t
        );
        
        if (!isValidExt) {
          ConfirmDialog.toast.error(`File type not allowed: ${file.name}`);
          return;
        }
        if (file.size > maxSizeMB * 1024 * 1024) {
          ConfirmDialog.toast.error(
            `File too large: ${file.name} (max ${maxSizeMB}MB)`
          );
          return;
        }
      }

      setUploading(true);
      setUploadProgress(0);

      const formData = new FormData();
      if (isAttachment) {
        // For attachments: upload one by one
        let successCount = 0;
        let errorCount = 0;
        const totalFiles = files.length;

        for (const file of files) {
          const singleFormData = new FormData();
          singleFormData.append("file", file);
          // Add description and order if needed
          singleFormData.append("description", `Attachment for ${entityType}`);
          singleFormData.append("order", 0);

          try {
            await apiService.post(uploadEndpoint, singleFormData, {
              headers: { "Content-Type": "multipart/form-data" },
              onUploadProgress: (progressEvent) => {
                const percent = Math.round(
                  (progressEvent.loaded * 100) / progressEvent.total
                );
                setUploadProgress(percent);
              },
            });
            successCount++;
          } catch (error) {
            errorCount++;
            console.error("Upload failed for file:", file.name, error);
          }
        }

        setUploading(false);
        setUploadProgress(0);

        if (successCount > 0) {
          await checkFiles();
          if (onUploadComplete) onUploadComplete();
          
          if (successCount === totalFiles) {
            ConfirmDialog.toast.success(`${successCount} file(s) uploaded successfully`);
          } else {
            ConfirmDialog.toast.warning(
              `${successCount} uploaded, ${errorCount} failed`
            );
          }
        } else {
          ConfirmDialog.toast.error("Failed to upload files");
        }
      } else {
        // For photo profile: single file
        const file = files[0];
        const singleFormData = new FormData();
        singleFormData.append("file", file);

        try {
          const response = await apiService.post(uploadEndpoint, singleFormData, {
            headers: { "Content-Type": "multipart/form-data" },
            onUploadProgress: (progressEvent) => {
              const percent = Math.round(
                (progressEvent.loaded * 100) / progressEvent.total
              );
              setUploadProgress(percent);
            },
          });

          if (response.data?.isSuccess) {
            handleSuccess({
              operation: "upload",
              entityName: "photo",
              message: `${entityType} photo uploaded successfully`,
            });
            await checkFiles();
            if (onUploadComplete) onUploadComplete();
          } else {
            const errorMsg =
              response.data?.errors?.join("\n") ||
              response.data?.message ||
              "Failed to upload";
            ConfirmDialog.toast.error(errorMsg);
          }
        } catch (error) {
          const errorMsg =
            error.userMessage || error.message || "Failed to upload";
          ConfirmDialog.toast.error(errorMsg);
        } finally {
          setUploading(false);
          setUploadProgress(0);
        }
      }
    },
    [allowedTypes, maxSizeMB, uploadEndpoint, isAttachment, entityType, checkFiles, onUploadComplete]
  );

  const handleDelete = useCallback(
    async (fileId) => {
      if (disableDelete) {
        ConfirmDialog.toast.warning("Delete is not available for this entity");
        return;
      }

      const confirmed = await ConfirmDialog.show({
        title: "Delete File",
        text: `Are you sure you want to delete this ${entityType} file?`,
        icon: "warning",
        confirmButtonText: "Delete",
      });
      if (!confirmed) return;

      try {
        const deleteEndpoint = isAttachment
          ? `${uploadEndpoint}/${fileId}`
          : uploadEndpoint;
          
        const response = await apiService.delete(deleteEndpoint);
        if (response.data?.isSuccess || response.status === 200) {
          ConfirmDialog.toast.success(`${entityType} file deleted`);
          await checkFiles();
          if (onFileDeleted) onFileDeleted();
          if (onUploadComplete) onUploadComplete();
        } else {
          const errorMsg =
            response.data?.errors?.join("\n") ||
            response.data?.message ||
            "Failed to delete";
          ConfirmDialog.toast.error(errorMsg);
        }
      } catch (error) {
        const errorMsg = error.userMessage || error.message || "Failed to delete";
        ConfirmDialog.toast.error(errorMsg);
      }
    },
    [disableDelete, entityType, uploadEndpoint, isAttachment, checkFiles, onFileDeleted, onUploadComplete]
  );

  const handlePreview = useCallback(() => {
    if (disablePreview) {
      ConfirmDialog.toast.warning("Preview is not available");
      return;
    }
    if (previewUrl) {
      setPreviewOpen(true);
    } else {
      ConfirmDialog.toast.warning("No file to preview");
    }
  }, [previewUrl, disablePreview]);

  const handleClosePreview = useCallback(() => {
    setPreviewOpen(false);
  }, []);

  if (!referenceId) {
    return (
      <Box className="file-uploader-placeholder">
        <Typography variant="body2" color="text.secondary">
          Save the {entityType} first to enable file upload
        </Typography>
      </Box>
    );
  }

  const headerTitle = title || `${entityType.charAt(0).toUpperCase() + entityType.slice(1)} File`;
  const fileCount = isAttachment ? fileList.length : (hasFiles ? 1 : 0);

  return (
    <div className={`file-uploader ${isAttachment ? 'file-uploader--attachment' : 'file-uploader--photo'}`}>
      <div className="file-uploader__header">
        <Typography variant="subtitle2">
          {isAttachment ? <FiPaperclip style={{ marginRight: 8 }} /> : <FiImage style={{ marginRight: 8 }} />}
          {headerTitle}
          {fileCount > 0 && (
            <span className="file-uploader__status">
              {isAttachment ? ` (${fileCount} file(s))` : " ✓ Uploaded"}
            </span>
          )}
        </Typography>
        <div className="file-uploader__actions">
          {hasFiles && !isAttachment && !disablePreview && (
            <IconButton
              size="small"
              onClick={handlePreview}
              title="Preview"
              className="file-uploader__action-btn"
            >
              <FiEye size={16} />
            </IconButton>
          )}
          {hasFiles && !disableDelete && (
            <IconButton
              size="small"
              onClick={() => handleDelete(isAttachment ? null : null)}
              title="Delete All"
              className="file-uploader__action-btn file-uploader__action-btn--danger"
            >
              <FiTrash2 size={16} />
            </IconButton>
          )}
          <label className="file-uploader__button">
            <FiUpload size={16} />
            {hasFiles ? (isAttachment ? "Add More" : "Replace") : "Upload"}
            <input
              type="file"
              onChange={handleFileSelect}
              disabled={uploading || !uploadEndpoint}
              accept={allowedTypes.filter(t => t.startsWith('.')).join(',')}
              multiple={isAttachment}
              style={{ display: "none" }}
              ref={fileInputRef}
            />
          </label>
        </div>
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
        <Typography variant="body2" color="text.secondary">
          Loading...
        </Typography>
      ) : isAttachment ? (
        // Attachment mode - show list of files
        <div className="file-uploader__attachment-list">
          {fileList.length === 0 ? (
            <div className="file-uploader__empty">
              <FiPaperclip size={32} />
              <Typography variant="body2" color="text.secondary">
                No attachments uploaded yet
              </Typography>
              <Typography variant="caption" color="text.secondary">
                Click Upload to add files (max {maxSizeMB}MB each)
              </Typography>
            </div>
          ) : (
            fileList.map((file, index) => (
              <div key={file.fileAttachmentId || index} className="file-uploader__attachment-item">
                <FiFile size={18} className="file-uploader__attachment-icon" />
                <span className="file-uploader__attachment-name">
                  {file.originalFileName || file.fileName || `File ${index + 1}`}
                </span>
                <span className="file-uploader__attachment-size">
                  {file.fileSize ? `${(file.fileSize / 1024).toFixed(1)} KB` : ''}
                </span>
                {!disableDelete && (
                  <button
                    className="file-uploader__attachment-delete"
                    onClick={() => handleDelete(file.fileAttachmentId || file.id)}
                    title="Delete"
                  >
                    <FiX size={14} />
                  </button>
                )}
              </div>
            ))
          )}
        </div>
      ) : (
        // Photo profile mode - show preview or empty state
        !hasFiles ? (
          <div className="file-uploader__empty">
            <FiImage size={32} />
            <Typography variant="body2" color="text.secondary">
              No photo uploaded yet
            </Typography>
            <Typography variant="caption" color="text.secondary">
              Click Upload to add a photo (max {maxSizeMB}MB)
            </Typography>
          </div>
        ) : (
          <div className="file-uploader__photo-preview">
            <img
              src={previewUrl || undefined}
              alt={`${entityType} photo`}
              className="file-uploader__photo-img"
              onError={() => setPreviewUrl(null)}
            />
          </div>
        )
      )}

      {/* Preview Dialog for Photo Profile */}
      <Dialog
        open={previewOpen}
        onClose={handleClosePreview}
        maxWidth="md"
        fullWidth
        PaperProps={{
          sx: {
            backgroundColor: "var(--card-bg)",
            borderRadius: "12px",
          },
        }}
      >
        <DialogContent sx={{ p: 0, position: "relative", minHeight: 400 }}>
          <MuiIconButton
            onClick={handleClosePreview}
            sx={{
              position: "absolute",
              top: 8,
              right: 8,
              zIndex: 1,
              backgroundColor: "rgba(0,0,0,0.5)",
              color: "white",
              "&:hover": { backgroundColor: "rgba(0,0,0,0.7)" },
            }}
          >
            <FiX size={20} />
          </MuiIconButton>
          {previewUrl && (
            <Box
              sx={{
                width: "100%",
                height: "70vh",
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
              }}
            >
              <img
                src={previewUrl}
                alt="Profile photo preview"
                style={{
                  maxWidth: "100%",
                  maxHeight: "100%",
                  objectFit: "contain",
                }}
              />
            </Box>
          )}
        </DialogContent>
      </Dialog>
    </div>
  );
};

export default FileUploader;