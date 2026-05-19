/**
 * PROFILE - NEW API CONTRACT
 * 
 * PUT /api/Auth/profile
 * 
 * Update the currently authenticated user's profile.
 * 
 * Request Body:
 * {
 *   "fullName": "string",
 *   "email": "string",
 *   "phoneNumber": "string (optional)",
 *   "username": "string (optional)"
 * }
 * 
 * Response:
 * {
 *   "isSuccess": true,
 *   "data": { ...updatedUser },
 *   "message": "Profile updated successfully"
 * }
 */