/**
 * DASHBOARD - NEW API CONTRACTS
 * 
 * GET /api/Reports/dashboard/monthly-stats
 * 
 * Returns monthly transaction counts for the dashboard chart.
 * Falls back gracefully if not implemented (chart shows empty state).
 * 
 * Query Parameters: None
 * 
 * Response:
 * {
 *   "isSuccess": true,
 *   "data": {
 *     "months": ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"],
 *     "counts": [65, 59, 80, 81, 56, 55, 40, 45, 60, 70, 75, 80]
 *   }
 * }
 */