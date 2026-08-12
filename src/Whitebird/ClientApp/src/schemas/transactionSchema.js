// src/core/schemas/transactionSchema.js
import { z } from 'zod';
import { TRANSACTION_TYPES } from '../constants/transactionTypes';

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
  // Required
  assetId: numberField(true),
  transactionType: numberField(true),
  transactionDate: datetimeField(true), // DATETIME

  // Single employee & office
  employeeId: numberField(),
  officeId: numberField(),

  // Renamed from expectedReturnDate
  expectedLoanReturnDate: dateField(),

  // Single condition (replaces conditionBefore / conditionAfter)
  condition: numberField(),

  notes: stringField(),
  maintenanceType: numberField(),
  maintenanceCost: numberField(),
  fromAssetTransactionId: numberField(),
}).superRefine((data, ctx) => {
  const type = data.transactionType;

  // LOAN requires expectedLoanReturnDate
  if (type === TRANSACTION_TYPES.LOAN && !data.expectedLoanReturnDate) {
    ctx.addIssue({
      code: z.ZodIssueCode.custom,
      path: ['expectedLoanReturnDate'],
      message: 'Expected return date is required for LOAN',
    });
  }

  // HANDOVER / TRANSFER / LOAN require employeeId
  if ([TRANSACTION_TYPES.HANDOVER, TRANSACTION_TYPES.TRANSFER, TRANSACTION_TYPES.LOAN].includes(type) && !data.employeeId) {
    ctx.addIssue({
      code: z.ZodIssueCode.custom,
      path: ['employeeId'],
      message: 'Employee is required for this transaction type',
    });
  }

  // OPERATIONAL_OFFICE requires officeId
  if (type === TRANSACTION_TYPES.OPERATIONAL_OFFICE && !data.officeId) {
    ctx.addIssue({
      code: z.ZodIssueCode.custom,
      path: ['officeId'],
      message: 'Office is required for OPERATIONAL OFFICE',
    });
  }

  // LOAN_RETURN / POST_MAINTENANCE require fromAssetTransactionId
  if ([TRANSACTION_TYPES.LOAN_RETURN, TRANSACTION_TYPES.POST_MAINTENANCE].includes(type) && !data.fromAssetTransactionId) {
    ctx.addIssue({
      code: z.ZodIssueCode.custom,
      path: ['fromAssetTransactionId'],
      message: 'Paired transaction is required',
    });
  }

  // Expected return date must be after transaction date
  if (data.expectedLoanReturnDate && data.transactionDate) {
    const expected = new Date(data.expectedLoanReturnDate);
    const trans = new Date(data.transactionDate);
    if (expected <= trans) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        path: ['expectedLoanReturnDate'],
        message: 'Expected return date must be after transaction date',
      });
    }
  }
});

export type TransactionFormData = z.infer<typeof transactionSchema>;
export default transactionSchema;