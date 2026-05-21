import { z } from 'zod';

// Base schema with common validations
const stringField = (max = 100, required = false) => {
  let schema = z.string().max(max, `Maximum ${max} characters`);
  if (!required) schema = schema.optional().or(z.literal('')).or(z.null());
  else schema = schema.min(1, 'This field is required');
  return schema;
};

const numberField = (required = false, min = 0, max = undefined) => {
  let schema = z.coerce.number().min(min, `Minimum value is ${min}`);
  if (max !== undefined) schema = schema.max(max, `Maximum value is ${max}`);
  if (!required) schema = schema.optional().or(z.null());
  return schema;
};

const dateField = (required = false) => {
  let schema = z.string().or(z.null()).optional();
  if (required) schema = z.string().min(1, 'Date is required');
  return schema;
};

export const assetSchema = z.object({
  // Required fields
  assetCode: stringField(50, true),
  assetName: stringField(100, true),
  categoryId: numberField(true, 1),
  
  // Optional fields
  brand: stringField(50),
  model: stringField(50),
  serialNumber: stringField(50),
  imei: stringField(50),
  macAddress: stringField(50),
  hostname: stringField(50),
  ipAddress: stringField(50),
  invoiceNumber: stringField(50),
  notes: stringField(500),
  
  // Number fields
  purchasePrice: numberField(false, 0),
  residualValue: numberField(false, 0),
  warrantyPeriod: numberField(false, 0),
  usefulLife: numberField(false, 1),
  
  // ID fields (FK)
  supplierId: numberField(false),
  officeId: numberField(false),
  assetCondition: numberField(false, 1, 3),
  assetConditionPurchase: numberField(false, 1, 2),
  
  // Date fields
  purchaseDate: dateField(),
  warrantyExpiryDate: dateField(),
  depreciationStartDate: dateField(),
  
  // Boolean field
  operasionalOffice: z.boolean().optional().or(z.null()),
});

export type AssetFormData = z.infer<typeof assetSchema>;

export default assetSchema;