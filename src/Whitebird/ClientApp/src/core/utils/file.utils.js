export const fileToBase64 = (file) => {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = () => {
      const base64 = reader.result.split(',')[1];
      resolve(base64);
    };
    reader.onerror = reject;
    reader.readAsDataURL(file);
  });
};

export const downloadBlob = (blob, fileName) => {
  const url = window.URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.setAttribute('download', fileName);
  document.body.appendChild(link);
  link.click();
  link.remove();
  window.URL.revokeObjectURL(url);
};

export const formatFileSize = (bytes) => {
  if (bytes === 0) return '0 Bytes';
  const k = 1024;
  const sizes = ['Bytes', 'KB', 'MB', 'GB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
};

export const isImageFile = (fileName) => {
  if (!fileName) return false;
  const ext = `.${fileName.split('.').pop()?.toLowerCase()}`;
  const imageExtensions = ['.jpg', '.jpeg', '.png', '.gif', '.webp', '.bmp', '.svg'];
  return imageExtensions.includes(ext);
};

export const getFileExtension = (fileName) => {
  if (!fileName) return '';
  const ext = fileName.split('.').pop()?.toLowerCase();
  return ext ? `.${ext}` : '';
};

export const getFileIcon = (fileName) => {
  if (!fileName) return 'file';
  const ext = `.${fileName.split('.').pop()?.toLowerCase()}`;
  const imageExts = ['.jpg', '.jpeg', '.png', '.gif', '.webp'];
  if (imageExts.includes(ext)) return 'image';
  const pdfExts = ['.pdf'];
  if (pdfExts.includes(ext)) return 'pdf';
  const docExts = ['.doc', '.docx'];
  if (docExts.includes(ext)) return 'document';
  const excelExts = ['.xls', '.xlsx'];
  if (excelExts.includes(ext)) return 'excel';
  const textExts = ['.txt', '.csv'];
  if (textExts.includes(ext)) return 'text';
  return 'file';
};