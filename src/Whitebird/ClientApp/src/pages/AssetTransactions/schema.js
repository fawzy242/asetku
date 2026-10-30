// src/pages/AssetTransactions/schema.js
import { z } from 'zod';
import { TRANSACTION_TYPES } from '../../core/constants/transactionTypes';

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
  assetId: numberField(true),
  transactionType: numberField(true),
  transactionDate: datetimeField(true),
  employeeId: numberField(),
  officeId: numberField(),
  expectedLoanReturnDate: dateField(),
  condition: numberField(),
  notes: stringField(),
  maintenanceType: numberField(),
  maintenanceCost: numberField(),
  fromAssetTransactionId: numberField(),
}).superRefine((data, ctx) => {
  const type = data.transactionType;

  if (type === TRANSACTION_TYPES.LOAN && !data.expectedLoanReturnDate) {
    ctx.addIssue({
      code: z.ZodIssueCode.custom,
      path: ['expectedLoanReturnDate'],
      message: 'Expected return date is required for LOAN',
    });
  }

  if ([TRANSACTION_TYPES.HANDOVER, TRANSACTION_TYPES.TRANSFER, TRANSACTION_TYPES.LOAN].includes(type) && !data.employeeId) {
    ctx.addIssue({
      code: z.ZodIssueCode.custom,
      path: ['employeeId'],
      message: 'Employee is required for this transaction type',
    });
  }

  if (type === TRANSACTION_TYPES.OPERATIONAL_OFFICE && !data.officeId) {
    ctx.addIssue({
      code: z.ZodIssueCode.custom,
      path: ['officeId'],
      message: 'Office is required for OPERATIONAL OFFICE',
    });
  }

  if ([TRANSACTION_TYPES.LOAN_RETURN, TRANSACTION_TYPES.POST_MAINTENANCE].includes(type) && !data.fromAssetTransactionId) {
    ctx.addIssue({
      code: z.ZodIssueCode.custom,
      path: ['fromAssetTransactionId'],
      message: 'Paired transaction is required',
    });
  }

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

export const getTransactionDefaultValues = () => ({
  assetId: '',
  transactionType: '',
  employeeId: '',
  officeId: '',
  expectedLoanReturnDate: '',
  notes: '',
  condition: '',
  maintenanceType: '',
  maintenanceCost: '',
  fromAssetTransactionId: '',
  transactionDate: new Date().toISOString(),
});

export default transactionSchema;