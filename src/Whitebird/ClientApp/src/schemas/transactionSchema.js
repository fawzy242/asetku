import { z } from 'zod';

const stringField = (max = 500, required = false) => {
  let schema = z.string().max(max, `Maximum ${max} characters`);
  if (!required) schema = schema.optional().or(z.literal('')).or(z.null());
  return schema;
};

const numberField = (required = false) => {
  let schema = z.coerce.number().optional().or(z.null());
  if (required) schema = z.coerce.number().min(1, 'This field is required');
  return schema;
};

const dateField = (required = false) => {
  let schema = z.string().optional().or(z.null());
  if (required) schema = z.string().min(1, 'Date is required');
  return schema;
};

const datetimeField = (required = true) => {
  let schema = z.string().min(1, 'Date and time is required');
  if (!required) schema = schema.optional();
  return schema;
};

export const transactionSchema = z.object({
  // Required fields
  assetId: numberField(true),
  transactionType: numberField(true),
  transactionDate: datetimeField(true),
  
  // Optional fields
  fromEmployeeId: numberField(),
  toEmployeeId: numberField(),
  toLocationId: numberField(),
  expectedReturnDate: dateField(),
  actualReturnDate: dateField(),
  notes: stringField(),
  conditionBefore: numberField(),
  conditionAfter: numberField(),
  maintenanceType: numberField(),
  maintenanceCost: numberField(),
  fromAssetTransactionId: numberField(),
  
  // Conditional validation
}).superRefine((data, ctx) => {
  // LOAN requires expectedReturnDate
  if (data.transactionType === 3 && !data.expectedReturnDate) {
    ctx.addIssue({
      code: z.ZodIssueCode.custom,
      path: ['expectedReturnDate'],
      message: 'Expected return date is required for LOAN',
    });
  }
  
  // LOAN_RETURN requires fromAssetTransactionId
  if (data.transactionType === 5 && !data.fromAssetTransactionId) {
    ctx.addIssue({
      code: z.ZodIssueCode.custom,
      path: ['fromAssetTransactionId'],
      message: 'Paired transaction is required for LOAN_RETURN',
    });
  }
  
  // POST_MAINTENANCE requires fromAssetTransactionId
  if (data.transactionType === 7 && !data.fromAssetTransactionId) {
    ctx.addIssue({
      code: z.ZodIssueCode.custom,
      path: ['fromAssetTransactionId'],
      message: 'Paired transaction is required for POST_MAINTENANCE',
    });
  }
  
  // TRANSFER requires both from and to employee
  if (data.transactionType === 2) {
    if (!data.fromEmployeeId) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        path: ['fromEmployeeId'],
        message: 'From employee is required for TRANSFER',
      });
    }
    if (!data.toEmployeeId) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        path: ['toEmployeeId'],
        message: 'To employee is required for TRANSFER',
      });
    }
  }
  
  // Expected return date must be after transaction date
  if (data.expectedReturnDate && data.transactionDate) {
    const expectedDate = new Date(data.expectedReturnDate);
    const transDate = new Date(data.transactionDate);
    if (expectedDate <= transDate) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        path: ['expectedReturnDate'],
        message: 'Expected return date must be after transaction date',
      });
    }
  }
});

export type TransactionFormData = z.infer<typeof transactionSchema>;

export default transactionSchema;