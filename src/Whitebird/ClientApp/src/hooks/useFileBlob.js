// src/hooks/useFileBlob.js
//
// Fetches a backend file (photo / attachment) via the authenticated axios
// instance and returns a renderable blob URL.
//
// Why a hook:
//   - The backend requires X-Session-Token on every file-serving endpoint.
//     <img src="/api/..."> cannot send custom headers → 401. Blob fetch via
//     axios injects the header; the resulting blob is served locally by the
//     browser (no auth needed for the blob URL itself).
//
// Why TanStack Query:
//   - Deduplication: UserMenu and Profile both show the same user photo;
//     a single network call serves both.
//   - Cache invalidation: dispatching `queryClient.invalidateQueries({
//     queryKey: ['file-blob'] })` after a photo upload triggers a fresh fetch
//     everywhere.
//   - Automatic GC: blob references are dropped after `gcTime`, releasing
//     memory.
//
// Lifecycle:
//   - Blob URL created on data arrival; revoked on:
//       * component unmount
//       * query data change (refetch, invalidation)
//       * backendPath prop change
//   - No leaked object URLs.
//
// Return shape:
//   { blob, blobUrl, loading, error }
//     blob     — raw Blob, or null (404 = no file) / undefined (loading or disabled)
//     blobUrl  — renderable blob: URL, or null when not ready
//     loading  — true while the fetch is in flight
//     error    — axios error on non-404 failure
//
// Usage:
//   const { blobUrl } = useFileBlob(user?.profilePhotoUrl);
//   {blobUrl ? <Avatar src={blobUrl} /> : <span>initials</span>}

import { useEffect, useRef, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import apiService from '../core/services/api.service';

export const useFileBlob = (backendPath) => {
  const query = useQuery({
    queryKey: ['file-blob', backendPath || '__disabled__'],
    queryFn: async () => {
      if (!backendPath) return null;
      return await apiService.getFileBlob(backendPath);
    },
    enabled: Boolean(backendPath),
    staleTime: 5 * 60 * 1000,
    gcTime: 10 * 60 * 1000,
  });

  const [blobUrl, setBlobUrl] = useState(null);
  const blobUrlRef = useRef(null);

  useEffect(() => {
    // Revoke previous URL (if any) before creating a new one.
    if (blobUrlRef.current) {
      URL.revokeObjectURL(blobUrlRef.current);
      blobUrlRef.current = null;
    }
    setBlobUrl(null);

    if (query.data instanceof Blob) {
      const url = URL.createObjectURL(query.data);
      blobUrlRef.current = url;
      setBlobUrl(url);
    }

    return () => {
      if (blobUrlRef.current) {
        URL.revokeObjectURL(blobUrlRef.current);
        blobUrlRef.current = null;
      }
    };
  }, [query.data]);

  return {
    blob: query.data,
    blobUrl,
    loading: query.isLoading,
    error: query.error,
  };
};

export default useFileBlob;