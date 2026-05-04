/**
 * EMPLOYEE SUMMARY - NEW API CONTRACTS
 * 
 * These endpoints do NOT exist in the current backend.
 */

/**
 * GET /api/Employee/{id}/summary
 * 
 * Returns complete aggregated summary for a single employee.
 * 
 * Response:
 * {
 *   "isSuccess": true,
 *   "data": {
 *     "employeeId": 1,
 *     "employeeCode": "EMP-001",
 *     "fullName": "John Doe",
 *     "department": "IT",
 *     "position": "Developer",
 *     "email": "john@example.com",
 *     "phoneNumber": "+62...",
 *     "employmentStatus": "Active",
 *     "joinDate": "2023-01-15T00:00:00",
 *     "totalAssets": 5,
 *     "totalAssetValue": 15000000,
 *     "totalTransactions": 25,
 *     "lastTransactionDate": "2024-01-20T10:30:00",
 *     "pendingReturns": 1,
 *     "recentAssets": [...],
 *     "recentTransactions": [...],
 *     "assetsByStatus": { "available": 2, "assigned": 3, "underRepair": 0, "retired": 0 }
 *   }
 * }
 */

/**
 * GET /api/Employee/summary-list
 * 
 * Returns lightweight summary for all employees (for selector/dropdown).
 * Supports: ?search=&status=&department=&page=&pageSize=
 */