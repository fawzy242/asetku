/**
 * Builds the fully qualified profile photo URL for the current session user.
 * The endpoint is per-session, not per-user ID — the userId is only used
 * to confirm a user is actually logged in.
 *
 * @param {number|string|null|undefined} userId
 * @returns {string|null}
 */
export const getProfilePhotoUrl = (userId) => {
  if (!userId) return null;
  if (typeof window === 'undefined') return null;

  const scheme = window.location.protocol.replace(':', '');
  const host = window.location.host;

  return `${scheme}://${host}/api/User/profile-photo`;
};

export default { getProfilePhotoUrl };